using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{

    [Header("Speeds Of Obstacles")]
    [SerializeField]
    private float bladeSpeed = 1f;
    [SerializeField]
    private float cutterSpeed = 0.5f;
    [SerializeField]
    private float spikeSpeed = 0.5f;
    [SerializeField]
    private float movedHazardSpeed = 0.5f;
    [SerializeField]
    private float cannonShootSpeed = 1f;

    [Header("Rates of obstacles")]
    [SerializeField]
    private float distanceOfMovedHazars = 2f;
    [SerializeField]
    private float repeatRateCannonShoot = 4.3f;

    [Header("Obstacles prefabs")]
    public GameObject[] obstacles; // Cannon = 0, Blade = 1, Cutter = 2, MovedHazard = 3, Spike = 4

    [SerializeField]
    private PlatformManager platformManager;
    [SerializeField]
    private EventManager eventManager;

    [SerializeField]
    private AudioClip[] _obstacles_Sfx;

    private List<AudioSource> _cannonShoot = new List<AudioSource>();

    public List<GameObject> blade { get; private set; }  // The dynamic colorfulface can be placed one cell that is surrounded center of blade. Should it be public , get and private set method must be activated   
    public List<GameObject> spikes { get; private set; }
    public List<GameObject> cutters { get; private set; }
    private List<GameObject> cannon = new List<GameObject>();
    private Dictionary<GameObject,Vector3> movedHazardVertical = new Dictionary<GameObject, Vector3>(); 
    private Dictionary<GameObject, Vector3> movedHazardHorizontal = new Dictionary<GameObject, Vector3>();

    private float _volume = 0f;

    public static bool progress = false;

    private bool isShooting = false;

    private void Awake()
    {
        _volume = PlayerPrefs.GetInt("Sfx");
        ObstacleManager.progress = false;
        blade = new List<GameObject>();
        spikes = new List<GameObject> ();
        cutters = new List<GameObject> ();
        StartCoroutine(adjustObstacles());
    }
    private IEnumerator adjustObstacles ()
    {
        yield return new WaitUntil(() => EventManager.progress);
        int pathSize = platformManager.solutionPath.Count;
        int unSolvedPathSize = platformManager.unSolutionPath.Count;
        int IsEnableVfx = PlayerPrefs.GetInt("Vfx");
        switch (platformManager.stage)
        {
            case 5:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                placeSpikes(1, 1, 1, 1);
                break;
            case 6:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 2f;
                movedHazardSpeed = 0.37f;
                if (Random.Range(0, 2) == 0)
                    placeMovedHazardVertical(2);
                else
                    placeMovedHazardHorizontal(2);
                placeSpikes(1, 1, 1, 1);
                break;
            case 7:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[0].transform.GetChild(0).GetChild(2).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 3f;
                movedHazardSpeed = 3f;
                repeatRateCannonShoot = 4.25f;
                cannonShootSpeed = 1f;
                if (Random.Range(0, 2) == 0)
                    placeMovedHazardVertical(2);
                else
                    placeMovedHazardHorizontal(2);
                placeSpikes(1, 1, 1, 1);
                placeCannon(unSolvedPathSize, false, true, false, false);
                break;
            case 8:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[0].transform.GetChild(0).GetChild(2).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 4f;
                movedHazardSpeed = 4f;
                repeatRateCannonShoot = 4.25f;
                cannonShootSpeed = 1f;
                if (Random.Range(0, 2) == 0)
                    placeMovedHazardVertical(2);
                else
                    placeMovedHazardHorizontal(2);
                placeSpikes(1, 1, 1, 1);
                placeCannon(unSolvedPathSize, false, true, true, false);
                break;
            case 9:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 4.5f;
                movedHazardSpeed = 4.45f;
                cutterSpeed = 1f;
                bladeSpeed = 1.15f;
                placeSpikes(1, 1, 1, 1);
                placeMovedHazardVertical(2);
                placeMovedHazardHorizontal(2);
                placeBlade(false,false,1,1,1,1);
                break;
            case 10:
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.51f;
                cutterSpeed = 1f;
                distanceOfMovedHazars = 4.5f;
                movedHazardSpeed = 4.65f;
                bladeSpeed = 1.15f;
                placeSpikes(1, 1, 1, 1);
                placeMovedHazardVertical(2);
                placeMovedHazardHorizontal(2);
                placeBlade(true,true,0, 0, 0, 0);
                break;
            case 11:
                spikeSpeed = 0.51f;
                cutterSpeed = 1f;
                distanceOfMovedHazars = 5f;
                movedHazardSpeed = 4.8f;
                bladeSpeed = 1.15f;
                obstacles[4].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                placeSpikes(1, 1, 1, 1);
                placeMovedHazardVertical(2);
                placeMovedHazardHorizontal(2);
                placeBlade(false, false, 1 , 1,  1, 1);
                break;
            case 12:
                cutterSpeed = 0.95f;
                obstacles[2].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                placeCutters(true, false, false);
                platformManager.CreateDynamicPath();
                break;
            default:
                break;       
        }
        ObstacleManager.progress = true;
        platformManager.unSolutionPath.Clear();
    }
    private void placeCannon(int Size, bool startRegion, bool secondRegion, bool thirdRegion, bool finalRegion )
    {
        if (startRegion)
        {
            Vector2Int Angle_135 = exceptionalPlacementOfCannon(Size, 135);
            if (Angle_135 != Vector2Int.zero)
                cannon.Add(Instantiate(obstacles[0], new Vector3(Angle_135.x + 0.05f, 0.8f, Angle_135.y - 0.05f), Quaternion.Euler(new Vector3(0f, 135f, 0f)), transform));
            var last = cannon.Last();
            CannonSetMPB(ref last);
        }
        if (secondRegion)
        {
            Vector2Int Angle_225 = exceptionalPlacementOfCannon(Size, 225);
            if (Angle_225 != Vector2Int.zero)
                cannon.Add(Instantiate(obstacles[0], new Vector3(Angle_225.x + 0.05f, 0.8f, Angle_225.y - 0.05f), Quaternion.Euler(new Vector3(0f, 225f, 0f)), transform));
            var last = cannon.Last();
            _cannonShoot.Add(last.GetComponent<AudioSource>());
            _cannonShoot[0].clip = _obstacles_Sfx[0];
            _cannonShoot.Last().minDistance = Vector3.Distance(last.transform.position, new Vector3(6f, 0.99f, 6f));
            _cannonShoot.Last().maxDistance = _cannonShoot.Last().minDistance + 4.2f;
            CannonSetMPB(ref last);
        }
        if (thirdRegion)
        {
            Vector2Int Angle_45 = exceptionalPlacementOfCannon(Size, 45);
            if (Angle_45 != Vector2Int.zero)
                cannon.Add(Instantiate(obstacles[0], new Vector3(Angle_45.x + 0.05f, 0.8f, Angle_45.y - 0.05f), Quaternion.Euler(new Vector3(0f, 45f, 0f)), transform));
            var last = cannon.Last();
            _cannonShoot.Add(last.GetComponent<AudioSource>());
            _cannonShoot[1].clip = _obstacles_Sfx[1];
            _cannonShoot.Last().minDistance = Vector3.Distance(last.transform.position, new Vector3(6f, 0.99f, 6f));
            _cannonShoot.Last().maxDistance = _cannonShoot.Last().minDistance + 4.2f;
            CannonSetMPB(ref last);
        }
        if (finalRegion)
        {
            Vector2Int Angle_315 = exceptionalPlacementOfCannon(Size, 315);
            if (Angle_315 != Vector2Int.zero)
                cannon.Add(Instantiate(obstacles[0], new Vector3(Angle_315.x + 0.05f, 0.8f, Angle_315.y - 0.05f), Quaternion.Euler(new Vector3(0f, 315f, 0f)), transform));
            var last = cannon.Last();
            CannonSetMPB(ref last);
        }
        if (cannon.Count > 0)
            InvokeRepeating("cannonShoot", 1f, repeatRateCannonShoot);
    }
    private void CannonSetMPB (ref GameObject _cannon)
    {
        MaterialPropertyBlock Cannon = new(), Wheel = new(), Wooden = new();
        Cannon.SetColor("_ColorBottom", obstacles[0].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Cannon.SetColor("_ColorTop", obstacles[0].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Wheel.SetColor("_ColorBottom", obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Wheel.SetColor("_ColorTop", obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        _cannon.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(Cannon);
        _cannon.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(Wheel);
    }
    private void placeBlade(bool verticalM, bool horizontalM,int firstRegion, int secondRegion, int thirdRegion, int fourthRegion) 
    {
        MaterialPropertyBlock Hazard = new(), Head = new(), Rod = new();
        Hazard.SetColor("_ColorBottom", obstacles[1].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Hazard.SetColor("_ColorTop", obstacles[1].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Head.SetColor("_ColorBottom", obstacles[1].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Head.SetColor("_ColorTop", obstacles[1].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Rod.SetColor("_ColorBottom", obstacles[1].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Rod.SetColor("_ColorTop", obstacles[1].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        List<Vector2Int> placed = exceptionalPlacementOfBlade(verticalM, horizontalM, firstRegion, secondRegion, thirdRegion, fourthRegion);
        while(placed.Count > 0) // If placable blade was found or not
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            var _Hazard = Instantiate(obstacles[1], new Vector3(placed[0].x, 1.55f, placed[0].y), Quaternion.identity, transform);
            _Hazard.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(Hazard);
            _Hazard.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(Head);
            _Hazard.transform.GetChild(2).GetComponent<Renderer>().SetPropertyBlock(Rod);
            blade.Add(_Hazard);
            placed.Remove(placed[0]);
        }
        blade.ForEach(t => t.transform.GetChild(0).localScale = new Vector3(1.2f, 1.2f, 1.2f));
        blade.ForEach(t => t.transform.GetChild(0).localPosition = new Vector3(0f, -1f, 0f));

        platformManager.CreateDynamicPath();
    }
    private void placeCutters (bool final, bool start , bool diamonds)
    {
        MaterialPropertyBlock _cutterInner = new(), _cutterKnife = new();

        _cutterInner.SetColor("_ColorBottom", obstacles[2].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterInner.SetColor("_ColorTop", obstacles[2].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));

        _cutterKnife.SetColor("_ColorBottom", obstacles[2].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterKnife.SetColor("_ColorTop", obstacles[2].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));
        List<Vector2Int> placed = exceptionalPlacementOfCutter(final,start,diamonds);
        while (placed.Count > 0)
        {
            var _cutter = Instantiate(obstacles[2], new Vector3(placed[0].x, -1f, placed[0].y), Quaternion.Euler(0f, 0f, 0f), transform);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterInner, 0);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterKnife, 1);
            cutters.Add(_cutter);
            placed.Remove(placed[0]);
        }
    }
    private void placeMovedHazardHorizontal (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", obstacles[3].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", obstacles[3].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = exceptionalPlacementOfHazard(false,true,0,count);
        while(placed.Count > 0)
        {
            var _MHH = Instantiate(obstacles[3], new Vector3(placed[0].x, 0.5f, placed[0].y), Quaternion.identity, transform);
            _MHH.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            movedHazardHorizontal.Add(_MHH, new Vector3(placed[0].x, 0.5f, placed[0].y));
            placed.Remove(placed[0]);
        }
        movedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localScale = new Vector3(1f, 1f, 1f));
        AdjustSpinTrail(movedHazardHorizontal,new Vector3(0f, 90f, 90f));
    }
    private void placeMovedHazardVertical (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", obstacles[3].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", obstacles[3].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = exceptionalPlacementOfHazard(true,false,count,0);
        while (placed.Count > 0)
        {
            var _MHV = Instantiate(obstacles[3], new Vector3(placed[0].x, 0.5f, placed[0].y), Quaternion.Euler(0, 90f, 0), transform);
            _MHV.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            movedHazardVertical.Add(_MHV, new Vector3(placed[0].x, 0.5f, placed[0].y));
            placed.Remove(placed[0]);
        }
        movedHazardHorizontal.ToList().ForEach(m => m.Key.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB));
        movedHazardVertical.ToList().ForEach(m => m.Key.transform.localScale = new Vector3(1f, 1f, 1f));
        AdjustSpinTrail(movedHazardVertical,new Vector3(90f, 0f, 90f));

    }
    private void AdjustSpinTrail (Dictionary<GameObject,Vector3> movedHazards,Vector3 rotation)
    {
        foreach (var movedHazard in movedHazards.ToList())
        {
            var shape = movedHazard.Key.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            shape.enabled = true;
            shape.rotation = rotation;
        }
    }
    private void placeSpikes (int firstCount,int secondCount, int thirdCount, int fourthCount) 
    {
        MaterialPropertyBlock spikeMPB = new();
        spikeMPB.SetColor("_ColorBottom", obstacles[4].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        spikeMPB.SetColor("_ColorTop", obstacles[4].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2Int> placed = exceptionalPlacementOfSpike(platformManager.solutionPath, firstCount, secondCount, thirdCount, fourthCount);
        while (placed.Count > 0)
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            GameObject spike = Instantiate(obstacles[4], new Vector3(placed[0].x, 0.29f, placed[0].y), Quaternion.identity, transform);
            spike.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(spikeMPB);
            spike.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = platformManager.GetTileMatAtPosition(placed[0]);
            spikes.Add(spike);
            platformManager.Replace(placed[0],spike.transform.GetChild(0).gameObject);
            placed.Remove(placed[0]);
        }
        platformManager.CreateDynamicPath();
    }
    private Vector2Int exceptionalPlacementOfCannon (int Size ,float angle)
    {
        Vector2Int pos = Vector2Int.zero;
        List<Vector2Int> place = new List<Vector2Int>();

        if (angle == 45f)
        {
            int startPointX = platformManager.stage / 2 + 7;
            int middleY = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6;
            for (int x = startPointX; x <= platformManager.stage + 6; x++)
            {
                for (int y = 6; y <= middleY; y++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            return place.Count == 0 ? Vector2Int.zero : place[Random.Range(0, place.Count)];
        } else if (angle == 135f)
        {
            int middleXY = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6;
            for (int x = 6; x <= middleXY; x++)
            {
                for (int y = 6; y <= middleXY; y++)
                {
                    if (x == 6 && y == 6)
                        continue;
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            return place.Count == 0 ? Vector2Int.zero : place[Random.Range(0, place.Count)];
        } else if (angle == 225f)
        {
            int startPointY = platformManager.stage / 2 + 7;
            int middleX = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6;
            for (int x = 6; x <= middleX; x++)
            {
                for (int y = startPointY; y <= platformManager.stage + 6; y++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            return place.Count == 0 ? Vector2Int.zero : place[Random.Range(0, place.Count)];
        } else
        {
            int middleXY = (platformManager.stage / 2) + 7;
            for (int x = middleXY; x <= platformManager.stage + 6; x++)
            {
                for (int y = middleXY; y <= platformManager.stage + 6; y++)
                {
                    if (x == platformManager.stage + 6 && y == platformManager.stage + 6)
                        continue;
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            return place.Count == 0 ? Vector2Int.zero : place[Random.Range(0, place.Count)];
        }
    }
    private List<Vector2Int> exceptionalPlacementOfBlade (bool vertical, bool horizontal, int firstRegion, int secondRegion, int thirdRegion, int fourthRegion)
    {
        List<Vector2Int> placed = new List<Vector2Int> (); // To be placed hazards list
        List<Vector2Int> finded = new List<Vector2Int>(); // the coordinates that scanned in the place whether it is appropriate or not as relative(if it include solution next to tile or not) 
        if (platformManager.stage % 2  == 0) { // If the stage is even , just place it acoording to the vertical or horizontal at the middle of
            int middlePoint = platformManager.stage / 2 + 6;
            if (vertical)
            {
                for (int y = 6; y < middlePoint ; y++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(y, middlePoint))) // Find unsolutions that is to be placed 
                        finded.Add(new Vector2Int(y, middlePoint));
                }
                finded = checkOutNeighbors(finded); // if the coordinates that funded includes the solution at next to or not
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero); // If the placeable hazard coordinat was founded , add it's coord
                finded.Clear(); // Clearing finded list
                for (int y = middlePoint; y <= platformManager.stage + 6; y++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(y, middlePoint))) // Same way on the above but this time do it for second half of path
                        finded.Add(new Vector2Int(y, middlePoint));
                }
                finded = checkOutNeighbors(finded);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
            }
            if (horizontal) // It works same way on the above , but this time horizontal
            {
                for (int x = 6; x < middlePoint; x++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(middlePoint, x)))
                        finded.Add(new Vector2Int(middlePoint, x));
                }
                finded = checkOutNeighbors(finded);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
                for (int x = middlePoint; x <= platformManager.stage + 6; x++)
                {
                    if (platformManager.unSolutionPath.Contains(new Vector2Int(middlePoint, x)))
                        finded.Add(new Vector2Int(middlePoint, x));
                }

                finded = checkOutNeighbors(finded);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
            }
            return placed;
        } else // If the stage is not even so odd then place all according to the region system just like spikes 
            placed = exceptionalPlacementOfSpike(platformManager.unSolutionPath, firstRegion, secondRegion, thirdRegion, fourthRegion);
            
        return placed;
    }
    private List<Vector2Int> checkOutNeighbors (List<Vector2Int> path)
    {
        if (path.Count == 0) // If there are not either solutionable or unsolotionable path , then return zero
            return path;
        foreach (var item in path.ToList()) // Check out how many solution path contains these solutions. Four way must be checked
        {
            bool horizontal = platformManager.solutionPath.Contains(item + Vector2Int.right) || platformManager.solutionPath.Contains(Vector2Int.left + item);
            bool vertical = platformManager.solutionPath.Contains(item + Vector2Int.up) || platformManager.solutionPath.Contains(Vector2Int.down + item);
            if (vertical == false && horizontal == false)
                path.Remove(item);
        }
        return path;
    }
    private List<Vector2> exceptionalPlacementOfHazard (bool vertical,bool horizontal,int verticalCount, int horizontalCount)
    {
        List<Vector2> pos = new List<Vector2>();

        float middle = platformManager.stage / 2 + 6 + 0.5f;
        middle -= (platformManager.stage % 2 != 0) ? 0 : Random.Range(0, 2); // Exact the middle of grid

        if (vertical)
        {
            while(verticalCount > 0)
            {
                pos.Add(new Vector2(middle, 6));
                if (platformManager.stage % 2 == 0) // Which side it must be place
                    middle += ((platformManager.stage / 2 + 6 + 0.5f) > middle) ? 1f : -1f;
                pos.Add(new Vector2(middle, platformManager.stage + 6));
                verticalCount -= 2;
            }
        }
        else if (horizontal)
        {
            while(horizontalCount > 0 )
            {
                pos.Add(new Vector2(platformManager.stage + 6, middle));
                if (platformManager.stage % 2 == 0) // Which side it must be place left or right
                    middle += ((platformManager.stage / 2 + 6 + 0.5f) > middle) ? 1f : -1f;
                pos.Add(new Vector2(6, middle));
                horizontalCount -= 2;
            }
        }

        return pos;
    }
    private List<Vector2Int> exceptionalPlacementOfCutter(bool final , bool start , bool diamond)
    {
        List<Vector2Int> placed = new List<Vector2Int>();

        if (final) // In finish tile
            placed.Add(new Vector2Int(platformManager.stage + 6, platformManager.stage + 6));
        if (start) // In origin tile
            placed.Add(new Vector2Int(6, 6));
        if (diamond)
        {
            List<Transform> _dims = eventManager.precious.Where(p => p.layer == 13).Select(p => p.GetComponent<Transform>()).ToList();
            foreach (var item in _dims)
            {
                Vector3 pos = item.position;
                placed.Add(new Vector2Int((int)pos.x, (int)pos.z));
            }
        } // In diamonds tile(Unsolutions that is next to solutions)
        return placed;
    }
    private List<Vector2Int> exceptionalPlacementOfSpike (List<Vector2Int> path, int firstRegionCount, int secondRegionCount, int thirdRegionCount, int fourthRegionCount)
    {
        Vector2Int first = Vector2Int.zero;
        Vector2Int second = Vector2Int.zero;
        Vector2Int third = Vector2Int.zero;
        Vector2Int fourth = Vector2Int.zero;

        List<Vector2Int> place = new List<Vector2Int>();

        if (firstRegionCount > 0) // Started region
        {
            int middleXY = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6; // Exact middle point of grid
            for (int x = 6; x <= middleXY; x++)
            {
                for (int y = 6; y <= middleXY; y++)
                {
                    if (((x == 6 && y == 7) || (x == 7 && y == 6)) && platformManager.unSolutionPath.Count == path.Count) // Blade is not placed on next step of origin
                        continue;
                    if (x == 6 && y == 6) // Blade or spike can not placed on the origin 
                        continue;
                    if (path.Contains(new Vector2Int(x, y))) // If path includes unsolutions or solutions according to the object type(Blade or Spike)
                        place.Add(new Vector2Int(x,y));
                }
            }
            place = path.Count == platformManager.unSolutionPath.Count ? checkOutNeighbors(place) : place; // Warning this step only works when blades are placed , so not spike.
            first = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)]; // If it does not contains placeble path , return zero
            place.Clear(); // Clear path so that it will be able to used other regions
        }
        if (secondRegionCount > 0)
        {                        
            int startPointY = platformManager.stage / 2 + 7;
            int middleX = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6;
            for (int x = 6; x <= middleX; x++)
            {
                for (int y = startPointY; y <= platformManager.stage + 6; y++)
                {
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == platformManager.unSolutionPath.Count ? checkOutNeighbors(place) : place;
            second = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        if (thirdRegionCount > 0)
        {
            int middleXY = (platformManager.stage / 2) + 7;
            for (int x = middleXY; x <= platformManager.stage + 6; x++)
            {
                for (int y = middleXY; y <= platformManager.stage + 6; y++)
                {
                    if (x == platformManager.stage + 6 && y == platformManager.stage + 6)
                            continue;
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == platformManager.unSolutionPath.Count ? checkOutNeighbors(place) : place;
            third = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        if (fourthRegionCount > 0)
        {
            int startPointX = platformManager.stage / 2 + 7;
            int middleY = (platformManager.stage % 2 == 0) ? platformManager.stage / 2 + 5 : platformManager.stage / 2 + 6;
            for (int x = startPointX; x <= platformManager.stage + 6; x++)
            {
                for (int y = 6; y <= middleY; y++)
                {
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == platformManager.unSolutionPath.Count ? checkOutNeighbors(place) : place;
            fourth = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        place.Add(first);
        place.Add(second);
        place.Add(third);
        place.Add(fourth);
        return place;
    }
    private Vector3 directionMovedHazard (Vector3 pos, bool Vertical)
    {
        int middle = (platformManager.stage + 12) / 2;

        if (Vertical) // Exact the which way does has to move as stage
            return middle <= pos.x ? Vector3.left : Vector3.right; 
        else
            return middle <= pos.z ? Vector3.back : Vector3.forward;
    }
    private void cannonShoot()
    {
        if (!isShooting)
            StartCoroutine(shoot(cannonShootSpeed));
    }
    private IEnumerator shoot(float duration)
    {
        isShooting = true;
        cannon.ForEach(c => c.transform.GetChild(0).GetChild(0).gameObject.SetActive(true));
        Vector3 start = new(-2.5f, 0f, -0.25f);
        Vector3 final = start * (2.5f * platformManager.stage);
        final.y = -0.1f;
        float elapsed = 0f;
        cannon.ForEach(c => c.transform.GetChild(0).GetChild(2).GetComponent<ParticleSystem>().Play());
        _cannonShoot.ForEach(c => c.PlayOneShot(c.clip, _volume));
        while (elapsed < duration)
        {
            float time = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, final, time);
            cannon.ForEach(c => c.transform.GetChild(0).GetChild(0).localPosition = pos);
            elapsed += Time.deltaTime;
            yield return null;
        }
        isShooting = false;
        cannon.ForEach(c => c.transform.GetChild(0).GetChild(0).gameObject.SetActive(false));
    }
    private Vector3 movedParts(Vector3 pos, Vector3 dir, float dis, float speed,bool isY)
    {
        if (isY)
           pos.y = 0.7f;
        // The move must be continious on the indicated range
        float offset = Mathf.PingPong(Time.time * speed, dis);
        Vector3 final = pos + dir * offset;
        return final;
    }
    private void Update()
    {
        if (spikes.Count != 0)
            spikes.ForEach(s => s.transform.GetChild(1).localPosition = movedParts(Vector3.zero, Vector3.down, 1f, spikeSpeed, false));

        if (movedHazardVertical.Count != 0)
        {
            Quaternion hazardRotationVertical = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.right);
            
            movedHazardVertical.ToList().ForEach(m => m.Key.transform.localRotation = Quaternion.Lerp(m.Key.transform.localRotation, hazardRotationVertical * m.Key.transform.localRotation, 10f));

            movedHazardVertical.ToList().ForEach(m => m.Key.transform.localPosition = movedParts(m.Value,directionMovedHazard(m.Value,false), distanceOfMovedHazars,movedHazardSpeed,false));

            movedHazardVertical.ToList().ForEach(m => m.Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f)));
        }

        if (movedHazardHorizontal.Count != 0)
        {
            Quaternion hazardRotationHorizontal = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.forward);

            movedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localRotation = Quaternion.Lerp(m.Key.transform.localRotation, hazardRotationHorizontal * m.Key.transform.localRotation, 10f));

            movedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localPosition = movedParts(m.Value, directionMovedHazard(m.Value,true), distanceOfMovedHazars, movedHazardSpeed, false));

            movedHazardHorizontal.ToList().ForEach(m => m.Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f)));
        }

        if (blade.Count != 0)
        {
            Quaternion bladeRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * obstacles[1].transform.GetChild(0).localRotation;

            blade.ToList().ForEach(b => b.transform.GetChild(0).localRotation = Quaternion.Lerp(b.transform.GetChild(0).localRotation, bladeRotation * b.transform.GetChild(0).localRotation, 1.5f));

            blade.ToList().ForEach(b => b.transform.GetChild(0).localPosition = movedParts(new Vector3(0f, -1f, 0f), Vector3.up, 2f,bladeSpeed,false));

            blade.ToList().ForEach(b => b.transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90f, 0f, 90f));
        }

        if (cutters.Count != 0)
            cutters.ForEach(c => c.transform.localPosition = movedParts(c.transform.localPosition, Vector3.down, 1.25f, cutterSpeed, true));
    }
}
