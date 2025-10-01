using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObstacleManager : ExceptionalPlacement
{
    [Header("Speeds Of Obstacles")]
    [SerializeField]
    private float CutterSpeed = 0.5f;
    [SerializeField]
    private float MovedHazardSpeed = 0.5f;

    [Header("Rates of obstacles")]
    [SerializeField]
    private float DistanceOfMovedHazars = 2f;

    [SerializeField]
    private PlatformManager platformManager;
    [Header("Linked Managers")]
    [SerializeField]
    private EventManager eventManager;
    public bool Progress_ => progress;

    public List<GameObject> Cutters { get; private set; }

    private Dictionary<GameObject, Vector3> MovedHazardVertical;
    private Dictionary<GameObject, Vector3> MovedHazardHorizontal;
    
    private float _volume = 0f;
    public ObstacleManager(int Stage, int UnSolutionCount) : base(Stage, UnSolutionCount)
    {

    }
    protected override void Awake()
    {
        base.Awake();
        ObstacleRenderers = new(20);
        Cutters = new List<GameObject>(2); // Ýt might be extented , if it needs to activated at the middles.
        MovedHazardVertical = new Dictionary<GameObject, Vector3>(2);
        MovedHazardHorizontal = new Dictionary<GameObject, Vector3>(2);
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
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.5f;
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 6:
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.5f;
                DistanceOfMovedHazars = 2f;
                MovedHazardSpeed = 0.37f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 7:
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.5f;
                DistanceOfMovedHazars = 3f;
                MovedHazardSpeed = 3f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 8:
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.5f;
                DistanceOfMovedHazars = 4f;
                MovedHazardSpeed = 4f;
                if (Random.Range(0, 2) == 0)
                    PlaceMovedHazardVertical(2);
                else
                    PlaceMovedHazardHorizontal(2);
                PlaceSpikes(1, 1, 1, 1);
                break;
            case 9:
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.5f;
                DistanceOfMovedHazars = 4.5f;
                MovedHazardSpeed = 4.45f;
                CutterSpeed = 1f;
                BladeSpeed = 1.325f;
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlades(false,false,1,1,1,1);
                break;
            case 10:
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                SpikeSpeed = 0.51f;
                CutterSpeed = 1f;
                DistanceOfMovedHazars = 4.5f;
                MovedHazardSpeed = 4.65f;
                BladeSpeed = 1.3f;
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlades(true,true,0, 0, 0, 0);
                break;
            case 11:
                SpikeSpeed = 0.51f;
                CutterSpeed = 1f;
                DistanceOfMovedHazars = 5f;
                MovedHazardSpeed = 4.8f;
                BladeSpeed = 1.75f;
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[2].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                PlaceSpikes(1, 1, 1, 1);
                PlaceMovedHazardVertical(2);
                PlaceMovedHazardHorizontal(2);
                PlaceBlades(false, false, 1 , 1,  1, 1);
                break;
            case 12:
                CutterSpeed = 0.95f;
                Obstacles[3].transform.GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                PlaceCutters(true, false, false);
                platformManager.CreateDynamics();
                break;
            default:
                break;       
        }
        progress = true;
        platformManager.UnSolution.Clear();
    }
    protected override void PlaceBlades(bool Vertical, bool Horizontal, int FirstRegion, int SecondRegion, int ThirdRegion, int FourthRegion) 
    {
        MaterialPropertyBlock Hazard = new(), Head = new(), Rod = new();
        Hazard.SetColor("_ColorBottom", Obstacles[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Hazard.SetColor("_ColorTop", Obstacles[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Head.SetColor("_ColorBottom", Obstacles[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Head.SetColor("_ColorTop", Obstacles[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Rod.SetColor("_ColorBottom", Obstacles[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Rod.SetColor("_ColorTop", Obstacles[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        List<Vector2Int> placed = ExceptionalPlacementOfBlade(platformManager.UnSolution, platformManager.SolutionPath ,Vertical, Horizontal, FirstRegion, SecondRegion, ThirdRegion, FourthRegion);
        BladeOffset = Spikes.Count + MovedHazardVertical.Count + MovedHazardHorizontal.Count;
        while(placed.Count > 0) // If placable blade was found or not
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            var _Hazard = Instantiate(Obstacles[2], new Vector3(placed[0].x, Obstacles[2].transform.position.y, placed[0].y), Quaternion.identity, transform);
            _Hazard.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(Hazard);
            _Hazard.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(Head);
            _Hazard.transform.GetChild(2).GetComponent<Renderer>().SetPropertyBlock(Rod);
            ObstacleRenderers.Add(_Hazard.transform.GetChild(0).GetComponent<Renderer>());
            ObstacleRenderers.Add(_Hazard.transform.GetChild(1).GetComponent<Renderer>());
            ObstacleRenderers.Add(_Hazard.transform.GetChild(2).GetComponent<Renderer>());
            Blades.Add(_Hazard);
            placed.Remove(placed[0]);
        }
        platformManager.CreateDynamics();
    }
    private void PlaceCutters (bool final, bool start , bool diamonds)
    {
        MaterialPropertyBlock _cutterInner = new(), _cutterKnife = new();

        _cutterInner.SetColor("_ColorBottom", Obstacles[3].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterInner.SetColor("_ColorTop", Obstacles[3].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));

        _cutterKnife.SetColor("_ColorBottom", Obstacles[3].GetComponent<Renderer>().sharedMaterials[0].GetColor("_ColorBottom"));
        _cutterKnife.SetColor("_ColorTop", Obstacles[3].GetComponent<Renderer>().sharedMaterials[1].GetColor("_ColorTop"));
        List<Vector2Int> placed = ExceptionalPlacementOfCutter(final,start,diamonds);
        while (placed.Count > 0)
        {
            var _cutter = Instantiate(Obstacles[3], new Vector3(placed[0].x, -1f, placed[0].y), Quaternion.Euler(0f, 0f, 0f), transform);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterInner, 0);
            _cutter.GetComponent<Renderer>().SetPropertyBlock(_cutterKnife, 1);
            Cutters.Add(_cutter);
            ObstacleRenderers.Add(_cutter.GetComponent<Renderer>());
            placed.Remove(placed[0]);
        }
    }
    private void PlaceMovedHazardHorizontal (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", Obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", Obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = ExceptionalPlacementOfHazard(false,true,0,count);
        while(placed.Count > 0)
        {
            var _MHH = Instantiate(Obstacles[1], new Vector3(placed[0].x, 0.425f, placed[0].y), Quaternion.identity, transform);
            _MHH.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            MovedHazardHorizontal.Add(_MHH, new Vector3(placed[0].x, 0.425f, placed[0].y));
            ObstacleRenderers.Add(_MHH.GetComponent<Renderer>());
            placed.Remove(placed[0]);
        }
        AdjustSpinTrail(MovedHazardHorizontal,new Vector3(0f, 90f, 90f));
    }
    private void PlaceMovedHazardVertical (int count)
    {
        MaterialPropertyBlock MovedHazardMPB = new();
        MovedHazardMPB.SetColor("_ColorBottom", Obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        MovedHazardMPB.SetColor("_ColorTop", Obstacles[1].transform.GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        List<Vector2> placed = ExceptionalPlacementOfHazard(true,false,count,0);
        while (placed.Count > 0)
        {
            var _MHV = Instantiate(Obstacles[1], new Vector3(placed[0].x, 0.425f, placed[0].y), Quaternion.Euler(0, 90f, 0), transform);
            _MHV.transform.GetComponent<Renderer>().SetPropertyBlock(MovedHazardMPB);
            ObstacleRenderers.Add(_MHV.GetComponent<Renderer>());
            MovedHazardVertical.Add(_MHV, new Vector3(placed[0].x, 0.425f, placed[0].y));
            placed.Remove(placed[0]);
        }
        AdjustSpinTrail(MovedHazardVertical,new Vector3(90f, 0f, 90f));

    }
    private void AdjustSpinTrail (Dictionary<GameObject,Vector3> movedHazards,Vector3 rotation)
    {
        foreach (var movedHazard in movedHazards) // movedHazards.ToList() is previous 
        {
            var shape = movedHazard.Key.transform.GetChild(0).GetComponent<ParticleSystem>().shape;
            shape.enabled = true;
            shape.rotation = rotation;
        }
    }
    private void PlaceSpikes (int firstCount,int secondCount, int thirdCount, int fourthCount) 
    {
        MaterialPropertyBlock _SpikeMPB = new(), SpikeFace = new();
        _SpikeMPB.SetColor("_ColorBottom", Obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        _SpikeMPB.SetColor("_ColorTop", Obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
       
        List<Vector2Int> Placed = ExceptionalPlacementOfSpike(platformManager.SolutionPath, null, firstCount, secondCount, thirdCount, fourthCount);
        List<Vector2Int> FacePos = new();
        while (Placed.Count > 0)
        {
            if (Placed[0] == Vector2Int.zero)
            {
                Placed.Remove(Placed[0]);
                continue;
            }
            GameObject spike = Instantiate(Obstacles[0], new Vector3(Placed[0].x, 0.29f, Placed[0].y), Quaternion.identity, transform);
            spike.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(_SpikeMPB);
            ObstacleRenderers.Add(spike.transform.GetChild(1).GetComponent<Renderer>());
            Material SurfaceMat = platformManager.GetTileMat(Placed[0]);
            SpikeFace.SetColor("_ColorBottom", SurfaceMat.GetColor("_ColorBottom"));
            SpikeFace.SetColor("_ColorTop", SurfaceMat.GetColor("_ColorTop"));
            spike.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(SpikeFace);
            Spikes.Add(spike);
            FacePos.Add(Placed[0]);
            Placed.Remove(Placed[0]);
            SpikeFace.Clear();
        }
        platformManager.Replace(FacePos, Spikes);
        FacePos.Clear();
        platformManager.CreateDynamics();
    }
    private void LateUpdate()
    {
        
        for (int i = 0; i < Spikes.Count; i++)
        {
            Spikes[i].transform.GetChild(1).localPosition = MovedParts(Vector3.zero, Vector3.down, 1f, SpikeSpeed, false);
            ObstacleRenderers[i].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i].bounds);
        }

        if (MovedHazardVertical.Count > 0)
        {
            Quaternion hazardRotationVertical = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.right);

            for (int i = 0; i < MovedHazardVertical.Count; i++)
            {
                var Key = MovedHazardVertical.ElementAt(i).Key;

                var value = MovedHazardVertical.ElementAt(i).Value;

                Key.transform.SetLocalPositionAndRotation(MovedParts(value, SetDirectionOfMovedHazard(value, false), DistanceOfMovedHazars, MovedHazardSpeed, false), Quaternion.Lerp(Key.transform.localRotation, hazardRotationVertical * Key.transform.localRotation, 20f));
                Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f));
                ObstacleRenderers[i + Spikes.Count].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i+Spikes.Count].bounds);
            }
        }

        if (MovedHazardHorizontal.Count > 0)
        {
            Quaternion hazardRotationHorizontal = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.forward);

            for (int i = 0; i < MovedHazardHorizontal.Count; i++)
            {
                var Key = MovedHazardHorizontal.ElementAt(i).Key;

                var Value = MovedHazardHorizontal.ElementAt(i).Value;

                Key.transform.SetLocalPositionAndRotation(MovedParts(Value, SetDirectionOfMovedHazard(Value, true), DistanceOfMovedHazars, MovedHazardSpeed, false), Quaternion.Lerp(Key.transform.localRotation, hazardRotationHorizontal * Key.transform.localRotation, 20f));
                Key.transform.GetChild(0).rotation = Quaternion.Euler(new Vector3(90f, 0f, 90f));
                ObstacleRenderers[i + Spikes.Count + MovedHazardVertical.Count].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i + Spikes.Count + MovedHazardVertical.Count].bounds);

            }
        }

        if (Blades.Count != 0)
        {
            Quaternion bladeRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * Obstacles[2].transform.GetChild(0).localRotation;

            for (int i = 0; i < Blades.Count; i++)
            {
                Blades[i].transform.GetChild(0).SetLocalPositionAndRotation(MovedParts(new Vector3(0f, -1f, 0f), Vector3.up, 2f, BladeSpeed, false), Quaternion.Lerp(Blades[i].transform.GetChild(0).localRotation, bladeRotation * Blades[i].transform.GetChild(0).localRotation, 1.5f));
                Blades[i].transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90f, 0f, 90f);
                ObstacleRenderers[i * 3 + BladeOffset + 0].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 0].bounds);
                ObstacleRenderers[i * 3 + BladeOffset + 1].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 1].bounds);
                ObstacleRenderers[i * 3 + BladeOffset + 2].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 2].bounds);
            }
        }

        for (int i = 0; i < Cutters.Count; i++)
        {
            Cutters[i].transform.localPosition = MovedParts(Cutters[i].transform.localPosition, Vector3.down, 1.25f, CutterSpeed, true);
            ObstacleRenderers[i].enabled = GeometryUtility.TestPlanesAABB(platformManager.Frustum_, ObstacleRenderers[i].bounds);
        }
    }
}
