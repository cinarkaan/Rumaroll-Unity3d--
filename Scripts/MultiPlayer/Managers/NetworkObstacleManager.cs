using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObstacleManager : ExceptionalPlacement
{
    [SerializeField]
    private NetworkPlatformManager NetworkPlatformManager;

    [SerializeField]
    private ServerManager ServerManager;

    public NetworkObstacleManager(int Stage, int UnSolutionCount) : base(Stage, UnSolutionCount)
    {
    }
    public bool Progress_ => progress;

    protected override void Awake()
    {
        base.Awake();
        ObstacleRenderers = new(20);
        StartCoroutine(WaitUntilPlatform());
    }

    private IEnumerator WaitUntilPlatform()
    {
        yield return new WaitUntil(() => NetworkPlatformManager.Progress);
        Stage = NetworkPlatformManager.Stage;
        UnSolutionCount = NetworkPlatformManager.UnSolution.Count;
        AdjustObstacles();
    }
    private void AdjustObstacles()
    {
        int IsEnableVfx = PlayerPrefs.GetInt("Vfx");
        switch (ServerManager.Difficulty.Value)
        {
            case 0:
                StartCoroutine(NetworkPlatformManager.LaunchDynamics());
                break;
            case 1: // Spikes , Moved-Blades
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                PlaceSpikes();
                break;
            case 2: // Spikes , Blades , MovedBlades (Enemy is only placed at the stage 12
                Obstacles[0].transform.GetChild(1).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(0).GetChild(0).gameObject.SetActive(IsEnableVfx == 1);
                Obstacles[1].transform.GetChild(3).gameObject.SetActive(IsEnableVfx == 1);
                PlaceSpikes();
                if (Stage == 12 || Stage == 10)
                {
                    BladeSpeed = (Stage == 12) ? 1.55f : 1.35f;
                    PlaceBlades(true, true, 0, 0, 0, 0);
                }
                else
                {
                    BladeSpeed = 1.5f;
                    PlaceBlades(false, false, 1, 1, 1, 1);
                }
                break;
            default:
                break;
        }
        progress = true;
        NetworkPlatformManager.UnSolution.Clear();
        NetworkPlatformManager.SolutionPath.Clear();

    }
    private void PlaceSpikes ()
    {
        MaterialPropertyBlock spikeMPB = new(), SpikeSurfaceMPB = new();
        List<Vector2Int> Placed = new();
        List<Vector2Int> FacePos = new();

        spikeMPB.SetColor("_ColorBottom", Obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        spikeMPB.SetColor("_ColorTop", Obstacles[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));

        // Only host is able to initialize for placements
        if (ServerManager.IsHost)
        {
            Placed = ExceptionalPlacementOfSpike(NetworkPlatformManager.SolutionPath, null, 1, 1, 1, 1);
            Placed.ForEach(p => ServerManager._Spikes.Add(p));
        }
        else
            foreach (Vector2Int pos in ServerManager._Spikes)
                Placed.Add(pos);

        while (Placed.Count > 0)
        {
            if (Placed[0] == Vector2Int.zero)
            {
                Placed.Remove(Placed[0]);
                continue;
            }
            GameObject spike = Instantiate(Obstacles[0], new Vector3(Placed[0].x, 0.29f, Placed[0].y), Quaternion.identity, transform);
            spike.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(spikeMPB);
            Material Surface =  NetworkPlatformManager.GetTileMat(Placed[0]);
            SpikeSurfaceMPB.SetColor("_ColorBottom", Surface.GetColor("_ColorBottom"));
            SpikeSurfaceMPB.SetColor("_ColorTop", Surface.GetColor("_ColorTop"));
            spike.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(SpikeSurfaceMPB);
            ObstacleRenderers.Add(spike.transform.GetChild(1).GetComponent<Renderer>());
            Spikes.Add(spike);
            FacePos.Add(Placed[0]);
            Placed.Remove(Placed[0]);
        }
        NetworkPlatformManager.Replace(FacePos, Spikes);
        FacePos.Clear();
    }
    protected override void PlaceBlades (bool Vertical, bool Horizontal, int FirstRegion, int SecondRegion, int ThirdRegion, int FourthRegion)
    {
        MaterialPropertyBlock Hazard = new(), Head = new(), Rod = new();
        List<Vector2Int> placed = new();
        Hazard.SetColor("_ColorBottom", Obstacles[1].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Hazard.SetColor("_ColorTop", Obstacles[1].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Head.SetColor("_ColorBottom", Obstacles[1].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Head.SetColor("_ColorTop", Obstacles[1].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        Rod.SetColor("_ColorBottom", Obstacles[1].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorBottom"));
        Rod.SetColor("_ColorTop", Obstacles[1].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial.GetColor("_ColorTop"));
        BladeOffset = Spikes.Count;
        if (ServerManager.IsHost)
        {
            placed = ExceptionalPlacementOfBlade(NetworkPlatformManager.UnSolution, NetworkPlatformManager.SolutionPath, Vertical, Horizontal, FirstRegion, SecondRegion, ThirdRegion, FourthRegion);
            placed.ForEach(p => ServerManager._Blades.Add(p));
        }
        else
            foreach (Vector2Int pos in ServerManager._Blades)
                placed.Add(pos);

        while (placed.Count > 0) // If placable blade was found or not
        {
            if (placed[0] == Vector2Int.zero)
            {
                placed.Remove(placed[0]);
                continue;
            }
            var _Hazard = Instantiate(Obstacles[1], new Vector3(placed[0].x, Obstacles[1].transform.position.y, placed[0].y), Quaternion.identity, transform);
            _Hazard.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(Hazard);
            _Hazard.transform.GetChild(1).GetComponent<Renderer>().SetPropertyBlock(Head);
            _Hazard.transform.GetChild(2).GetComponent<Renderer>().SetPropertyBlock(Rod);
            ObstacleRenderers.Add(_Hazard.transform.GetChild(0).GetComponent<Renderer>());
            ObstacleRenderers.Add(_Hazard.transform.GetChild(1).GetComponent<Renderer>());
            ObstacleRenderers.Add(_Hazard.transform.GetChild(2).GetComponent<Renderer>());
            Blades.Add(_Hazard);
            placed.Remove(placed[0]);
        }
    }
    
    public void LateUpdate() 
    {
        if (!ServerManager.Launch.Value) return;
        for (int i = 0; i < Spikes.Count; i++)
        {
            Spikes[i].transform.GetChild(1).localPosition = MovedParts(Vector3.zero, Vector3.down, 1f, SpikeSpeed, false);
            ObstacleRenderers[i].enabled = GeometryUtility.TestPlanesAABB(NetworkPlatformManager.Frustum_, ObstacleRenderers[i].bounds);
        }
        if (Blades.Count != 0)
        {
            Quaternion bladeRotation = Quaternion.AngleAxis(360f * Time.deltaTime, Vector3.up) * Obstacles[1].transform.GetChild(0).localRotation;

            for (int i = 0; i < Blades.Count; i++)
            {
                Blades[i].transform.GetChild(0).SetLocalPositionAndRotation(MovedParts(new Vector3(0f, -1f, 0f), Vector3.up, 2f, BladeSpeed, false), Quaternion.Lerp(Blades[i].transform.GetChild(0).localRotation, bladeRotation * Blades[i].transform.GetChild(0).localRotation, 1.5f));
                Blades[i].transform.GetChild(0).GetChild(0).rotation = Quaternion.Euler(90f, 0f, 90f);
                ObstacleRenderers[i * 3 + BladeOffset + 0].enabled = GeometryUtility.TestPlanesAABB(NetworkPlatformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 0].bounds);
                ObstacleRenderers[i * 3 + BladeOffset + 1].enabled = GeometryUtility.TestPlanesAABB(NetworkPlatformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 1].bounds);
                ObstacleRenderers[i * 3 + BladeOffset + 2].enabled = GeometryUtility.TestPlanesAABB(NetworkPlatformManager.Frustum_, ObstacleRenderers[i * 3 + BladeOffset + 2].bounds);

            }
        }
    }
}




