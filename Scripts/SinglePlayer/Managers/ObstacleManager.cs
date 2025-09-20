using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : ExceptionalPlacement
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

    [Header("Rates of obstacles")]
    [SerializeField]
    private float distanceOfMovedHazars = 2f;

    [Header("Obstacles prefabs")]
    public GameObject[] obstacles; //  Blade = 2, Cutter = 3, MovedHazard = 1, Spike = 0

    [Header("Linked Managers")]
    [SerializeField]
    private PlatformManager platformManager;
    [SerializeField]
    private EventManager eventManager;

    public List<GameObject> Blade { get; private set; }  // The dynamic colorfulface can be placed one cell that is surrounded center of blade. Should it be public , get and private set method must be activated   
    public List<GameObject> Spikes { get; private set; }
    public List<GameObject> Cutters { get; private set; }

    private Dictionary<GameObject, Vector3> MovedHazardVertical;
    private Dictionary<GameObject, Vector3> MovedHazardHorizontal;

    private float _volume = 0f;

    public static bool progress = false;

    public ObstacleManager(int Stage, int UnSolutionCount) : base(Stage, UnSolutionCount)
    {

    }

    private void Awake()
    {
        Blade = new List<GameObject>();
        Spikes = new List<GameObject>();
        Cutters = new List<GameObject>();
        MovedHazardVertical = new Dictionary<GameObject, Vector3>();
        MovedHazardHorizontal = new Dictionary<GameObject, Vector3>();
        _volume = PlayerPrefs.GetInt("Sfx");

        StartCoroutine(AdjustObstacles());
    }
    private IEnumerator AdjustObstacles ()
    {
        yield return new WaitUntil(() => EventManager.progress);
        base.Stage = platformManager.Stage;
        base.UnSolutionCount = platformManager.UnSolution.Count;
        int pathSize = platformManager.SolutionPath.Count;
        int unSolvedPathSize = platformManager.UnSolution.Count;
        int IsEnableVfx = PlayerPrefs.GetInt("Vfx");
        switch (platformManager.Stage)
        {
            case 5:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 6:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 2f;
                movedHazardSpeed = 0.37f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 7:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 3f;
                movedHazardSpeed = 3f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 8:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 4f;
                movedHazardSpeed = 4f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 9:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.5f;
                distanceOfMovedHazars = 4.5f;
                movedHazardSpeed = 4.45f;
                cutterSpeed = 1f;
                bladeSpeed = 1.325f;
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlade(false,false,1,1,1,1);
                break;
            case 10:
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                spikeSpeed = 0.51f;
                cutterSpeed = 1f;
                distanceOfMovedHazars = 4.5f;
                movedHazardSpeed = 4.65f;
                bladeSpeed = 1.3f;
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlade(true,true,0, 0, 0, 0);
                break;
            case 11:
                spikeSpeed = 0.51f;
                cutterSpeed = 1f;
                distanceOfMovedHazars = 5f;
                movedHazardSpeed = 4.8f;
                bladeSpeed = 1.75f;
                obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlade(false, false, 1 , 1,  1, 1);
                break;
            case 12:
                cutterSpeed = 0.95f;
                obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                PlaceCutters(true, false, false);
                platformManager.CreateDynamics();
                break;
            default:
                break;       
        }
        ObstacleManager.progress = true;
        platformManager.UnSolution.Clear();
    }
    private void PlaceBlade(bool verticalM, bool horizontalM,int firstRegion, int secondRegion, int thirdRegion, int fourthRegion) 
    {
        MaterialPropertyBlock Hazard = new(), Head = new(), Rod = new();
        Hazard.SetColor("_ColorBottom", obstacles[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Hazard.SetColor("_ColorTop", obstacles[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Head.SetColor("_ColorBottom", obstacles[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Head.SetColor("_ColorTop", obstacles[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Rod.SetColor("_ColorBottom", obstacles[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Rod.SetColor("_ColorTop", obstacles[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        List<Vector2Int> placed = ExceptionalPlacementOfBlade(platformManager.UnSolution, platformManager.SolutionPath ,verticalM, horizontalM, firstRegion, secondRegion, thirdRegion, fourthRegion);
        while(placed.Count > 0) // If placable blade was found or not
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            var _Hazard = Instantiate(obstacles[2], new Vector3(placed[0].x, obstacles[2].transform.position.y, placed[0].y), Quaternion.identity, transform);
            _Hazard.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(Hazard);
            _Hazard.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(Head);
            _Hazard.transform.GetChild(2).GetComponent<Renderer>().SetPropertyBlock(Rod);
            Blade.Add(_Hazard);
            placed.Remove(placed[0]);
        }
        platformManager.CreateDynamics();
    }
    private void PlaceCutters (bool final, bool start , bool diamonds)
    {
        MaterialPropertyBlock _cutterInner = new(), _cutterKnife = new();

        _cutterInner.SetColor("_ColorBottom", obstacles[3].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterInner.SetColor("_ColorTop", obstacles[3].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));

        _cutterKnife.SetColor("_ColorBottom", obstacles[3].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterKnife.SetColor("_ColorTop", obstacles[3].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));
        List<Vector2Int> placed = ExceptionalPlacementOfCutter(final,start,diamonds);
        while (placed.Count > 0)
        {
            var _cutter = Instantiate(obstacles[3], new Vector3(placed[0].x, -1f, placed[0].y), Quaternion.Euler(0f, 0f, 0f), transform);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterInner, 0);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterKnife, 1);
            Cutters.Add(_cutter);
            placed.Remove(placed[0]);
        }
    }
    private void PlaceMovedHazardHorizontal (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = ExceptionalPlacementOfHazard(false,true,0,count);
        while(placed.Count > 0)
        {
            var _MHH = Instantiate(obstacles[1], new Vector3(placed[0].x, 0.5f, placed[0].y), Quaternion.identity, transform);
            _MHH.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            MovedHazardHorizontal.Add(_MHH, new Vector3(placed[0].x, 0.5f, placed[0].y));
            placed.Remove(placed[0]);
        }
        MovedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localScale = new Vector3(1f, 1f, 1f));
        AdjustSpinTrail(MovedHazardHorizontal,new Vector3(0f, 90f, 90f));
    }
    private void PlaceMovedHazardVertical (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = ExceptionalPlacementOfHazard(true,false,count,0);
        while (placed.Count > 0)
        {
            var _MHV = Instantiate(obstacles[1], new Vector3(placed[0].x, 0.5f, placed[0].y), Quaternion.Euler(0, 90f, 0), transform);
            _MHV.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            MovedHazardVertical.Add(_MHV, new Vector3(placed[0].x, 0.5f, placed[0].y));
            placed.Remove(placed[0]);
        }
        MovedHazardHorizontal.ToList().ForEach(m => m.Key.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB));
        MovedHazardVertical.ToList().ForEach(m => m.Key.transform.localScale = new Vector3(1f, 1f, 1f));
        AdjustSpinTrail(MovedHazardVertical,new Vector3(90f, 0f, 90f));

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
    private void PlaceSpikes (int firstCount,int secondCount, int thirdCount, int fourthCount) 
    {
        MaterialPropertyBlock _SpikeMPB = new(), SpikeFace = new();
        _SpikeMPB.SetColor("_ColorBottom", obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        _SpikeMPB.SetColor("_ColorTop", obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        

        List<Vector2Int> placed = ExceptionalPlacementOfSpike(platformManager.SolutionPath, null, firstCount, secondCount, thirdCount, fourthCount);
        while (placed.Count > 0)
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            GameObject spike = Instantiate(obstacles[0], new Vector3(placed[0].x, 0.29f, placed[0].y), Quaternion.identity, transform);
            spike.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(_SpikeMPB);
            Material SurfaceMat = platformManager.GetTileMat(placed[0]);
            SpikeFace.SetColor("_ColorBottom", SurfaceMat.GetColor("_ColorBottom").gamma);
            SpikeFace.SetColor("_ColorTop", SurfaceMat.GetColor("_ColorTop").gamma);
            spike.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(SpikeFace);
            Spikes.Add(spike);
            platformManager.Replace(placed[0],spike.transform.GetChild(0).gameObject);
            placed.Remove(placed[0]);
            SpikeFace.Clear();
        }
        platformManager.CreateDynamics();
    }
    private void LateUpdate()
    {
        if (Spikes.Count != 0)
            for (int i = 0; i < Spikes.Count; i++)
                Spikes[i].transform.GetChild(1).localPosition = MovedParts(Vector3.zero, Vector3.down, 1f, spikeSpeed, false);
            
            
        //Spikes.ForEach(s => s.transform.GetChild(1).localPosition = MovedParts(Vector3.zero, Vector3.down, 1f, spikeSpeed, false)); // To avoid GC Allocation

        if (MovedHazardVertical.Count > 0)
        {
            Quaternion hazardRotationVertical = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.right);

            for (int i = 0; i < MovedHazardVertical.Count; i++)
            {
                var Key = MovedHazardVertical.ElementAt(i).Key;

                var value = MovedHazardVertical.ElementAt(i).Value;

                Key.transform.SetLocalPositionAndRotation(MovedParts(value, SetDirectionOfMovedHazard(value, false), distanceOfMovedHazars, movedHazardSpeed, false), Quaternion.Lerp(Key.transform.localRotation, hazardRotationVertical * Key.transform.localRotation, 10f));
                Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f));

            }

            // To avoid GC Allocation

            //MovedHazardVertical.ToList().ForEach(m => m.Key.transform.localRotation = Quaternion.Lerp(m.Key.transform.localRotation, hazardRotationVertical * m.Key.transform.localRotation, 10f));

            //MovedHazardVertical.ToList().ForEach(m => m.Key.transform.localPosition = MovedParts(m.Value,SetDirectionOfMovedHazard(m.Value,false), distanceOfMovedHazars,movedHazardSpeed,false));

            //MovedHazardVertical.ToList().ForEach(m => m.Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f)));
        }

        if (MovedHazardHorizontal.Count > 0)
        {
            Quaternion hazardRotationHorizontal = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.forward);

            for (int i = 0; i < MovedHazardHorizontal.Count; i++)
            {
                var Key = MovedHazardHorizontal.ElementAt(i).Key;

                var Value = MovedHazardHorizontal.ElementAt(i).Value;

                Key.transform.SetLocalPositionAndRotation(MovedParts(Value, SetDirectionOfMovedHazard(Value, true), distanceOfMovedHazars, movedHazardSpeed, false), Quaternion.Lerp(Key.transform.localRotation, hazardRotationHorizontal * Key.transform.localRotation, 10f));
                Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f));

            }

            // To Avoid GC Allocation 

            //MovedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localRotation = Quaternion.Lerp(m.Key.transform.localRotation, hazardRotationHorizontal * m.Key.transform.localRotation, 10f));

            //MovedHazardHorizontal.ToList().ForEach(m => m.Key.transform.localPosition = MovedParts(m.Value, SetDirectionOfMovedHazard(m.Value,true), distanceOfMovedHazars, movedHazardSpeed, false));

            //MovedHazardHorizontal.ToList().ForEach(m => m.Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f)));
        }

        if (Blade.Count != 0)
        {
            Quaternion bladeRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * obstacles[2].transform.GetChild(0).localRotation;

            for (int i = 0; i < Blade.Count; i++)
            {
                Blade[i].transform.GetChild(0).SetLocalPositionAndRotation(MovedParts(new Vector3(0f, -1f, 0f), Vector3.up, 2f, bladeSpeed, false), Quaternion.Lerp(Blade[i].transform.GetChild(0).localRotation, bladeRotation * Blade[i].transform.GetChild(0).localRotation, 1.5f));
                Blade[i].transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90f, 0f, 90f);
            }

            // To avoid GC Allocation
            
            //Blade.ToList().ForEach(b => b.transform.GetChild(0).localRotation = Quaternion.Lerp(b.transform.GetChild(0).localRotation, bladeRotation * b.transform.GetChild(0).localRotation, 1.5f));

            //Blade.ToList().ForEach(b => b.transform.GetChild(0).localPosition = MovedParts(new Vector3(0f, -1f, 0f), Vector3.up, 2f,bladeSpeed,false));

            //Blade.ToList().ForEach(b => b.transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90f, 0f, 90f));
        }

        if (Cutters.Count != 0)
            for (int i = 0; i < Cutters.Count; i++)
                Cutters[i].transform.localPosition = MovedParts(Cutters[i].transform.localPosition, Vector3.down, 1.25f, cutterSpeed, true);
            //Cutters.ForEach(c => c.transform.localPosition = MovedParts(c.transform.localPosition, Vector3.down, 1.25f, cutterSpeed, true));
    }
}
