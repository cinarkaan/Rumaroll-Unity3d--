using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{

    [Header("Grid Settings")]
    [Range(4, 13)]
    public int stage = 5;

    [SerializeField]
    private GameObject[] prefabs;

    private Object[] tileMaterials; // [0]=Bottom, [1]=Top, [2]=Front, [3]=Back, [4]=Left, [5]=Right

    [SerializeField]
    private ParticleSystem[] weatherSystem;

    [SerializeField]
    private GameMapController gameMapController;

    public ParticleSystem _smoke_Burst,_clue;

    private Dictionary<Vector2Int, PlatformTile> gridTiles = new Dictionary<Vector2Int, PlatformTile>();

    public List<Vector2Int> solutionPath = new List<Vector2Int>();
    public List<Vector2Int> unSolutionPath = new List<Vector2Int>();
    public HashSet<Vector2Int> dynamicPath = new HashSet<Vector2Int>();

    public bool progress = false;

    private Mesh _frame, _tile, _surfaces;
    private Material tileMat, frameMat, surfacesMat;

    private List<Matrix4x4> tile = new();
    private List<Matrix4x4> frame = new();
    private List<Matrix4x4> surfaces = new();

    private List<Renderer> renderers = new();

    private void Awake()
    {
        Application.targetFrameRate = -1;

        progress = false;

        prefabs[2].transform.position = new Vector3(6, 0.99f, 6);

        //stage = PlayerPrefs.GetInt("Stage");

        AdjustPlatformStatus(Random.Range(0, 3));

        RandomSelection();

        InitializeEnvironment();

        InitializeSolution();

        CreateGrid();

        PlaceFlags();
    }
    void Update()
    {
        if (progress && gameMapController.currentIndex == 1)
        {
            ParallelFrustumCulling();
            FrustumCullingForColorfuls();
        }
        else
        {
            if (progress)
            {
                Graphics.DrawMeshInstanced(_tile, 0, tileMat, tile);
                Graphics.DrawMeshInstanced(_frame, 0, frameMat, frame);
                Graphics.DrawMeshInstanced(_surfaces, 0, surfacesMat, surfaces);
                foreach (var renderer in renderers)
                    if (!renderer.enabled)
                        renderer.enabled = true;
            }
        }
    }
    private void ParallelFrustumCulling()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        ConcurrentBag<Matrix4x4> visibleTile = new ConcurrentBag<Matrix4x4>();
        ConcurrentBag<Matrix4x4> visibleFrame = new ConcurrentBag<Matrix4x4>();
        ConcurrentBag<Matrix4x4> visibleSurfaces = new ConcurrentBag<Matrix4x4>();

        int total = tile.Count;

        Parallel.For(0 , total, index =>
        {
            if (index < surfaces.Count)
            {
                Vector3 pos = surfaces[index].GetColumn(3);
                Bounds bounds = new Bounds(pos, Vector3.one);
                if (IsBoundsInsideFrustum(bounds, frustumPlanes))
                    visibleSurfaces.Add(surfaces[index]);
            }

            Vector3 tilepos = tile[index].GetColumn(3);
            Vector3 framepos = frame[index].GetColumn(3);

            if (IsBoundsInsideFrustum(new Bounds(tilepos, Vector3.one), frustumPlanes))
                visibleTile.Add(tile[index]);

            if (IsBoundsInsideFrustum(new Bounds(framepos, Vector3.one), frustumPlanes))
                visibleFrame.Add(frame[index]);
        });
        
        Graphics.DrawMeshInstanced(_tile, 0, tileMat, visibleTile.ToList());
        Graphics.DrawMeshInstanced(_frame, 0, frameMat, visibleFrame.ToList());
        Graphics.DrawMeshInstanced(_surfaces, 0, surfacesMat, visibleSurfaces.ToList());

    }
    private void FrustumCullingForColorfuls ()
    {
        Plane[] Frustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (var renderer in renderers)
            renderer.enabled = GeometryUtility.TestPlanesAABB(Frustum, renderer.bounds);
    }
    private bool IsBoundsInsideFrustum(Bounds bounds, Plane[] planes)
    {
        for (int i = 0; i < planes.Length; i++)
        {
            Plane plane = planes[i];
            Vector3 normal = plane.normal;
            Vector3 point = bounds.center;

            Vector3 extents = bounds.extents;

            // Projection extent along plane normal
            float r = extents.x * Mathf.Abs(normal.x) +
                      extents.y * Mathf.Abs(normal.y) +
                      extents.z * Mathf.Abs(normal.z);

            // Distance from plane
            float d = plane.GetDistanceToPoint(point);

            // If outside
            if (d + r < 0)
                return false;
        }

        return true;
    }
    private void RandomSelection()
    {
        HashSet<Object> selected = new HashSet<Object>();

        tileMaterials = Resources.LoadAll("Gradient", typeof(Material));

        while (selected.Count < 6)
            selected.Add(tileMaterials[Random.Range(0, tileMaterials.Length)]);


        prefabs[2].transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[0];
        prefabs[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[1];
        prefabs[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[2];
        prefabs[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[3];
        prefabs[2].transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[4];
        prefabs[2].transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[5];

        tileMaterials = selected.ToArray();

        selected.Clear();
    }
    private void InitializeEnvironment ()
    {
        _tile = prefabs[0].GetComponent<MeshFilter>().sharedMesh;
        tileMat = prefabs[0].GetComponent<MeshRenderer>().sharedMaterial;

        _surfaces = prefabs[0].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        surfacesMat = prefabs[0].transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

        _frame = prefabs[0].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        frameMat = prefabs[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;
    }
    private void InitializeSolution ()
    {
        Vector2Int goal = new Vector2Int(6 + stage, 6 + stage);
        List<Vector2Int> solutionPath = GenerateSolutionPath(new Vector2Int(6, 6), goal);

        CubeSimulator cubeSim = new CubeSimulator();
        gridTiles[new Vector2Int(6, 6)] = new PlatformTile((Material)tileMaterials[0], true);

        for (int i = 1; i < solutionPath.Count; i++)
        {
            Vector2Int prev = solutionPath[i - 1];
            Vector2Int current = solutionPath[i];
            Vector3 moveDir = new Vector3(current.x - prev.x, 0, current.y - prev.y);

            cubeSim.Roll(moveDir);
            int bottomFace = cubeSim.faceIndices[0];
            gridTiles[solutionPath[i]] = new PlatformTile((Material)tileMaterials[bottomFace], true);
        }
    }
    private List<Vector2Int> GenerateSolutionPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        bool found = DepthFirstSearch(start, goal, path, visited);
        path.Reverse(); // DFS tersten gelir

        return path;
    }
    private bool DepthFirstSearch(Vector2Int current, Vector2Int goal, List<Vector2Int> path, HashSet<Vector2Int> visited)
    {
        if (current == goal)
        {
            path.Add(current);
            return true;
        }

        visited.Add(current);

        List<Vector2Int> directions = new List<Vector2Int> {
            Vector2Int.right,
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.down
        }.OrderBy(x => Random.value).ToList(); // 

        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;

            if (next.x < 6 || next.y < 6 || next.x > stage + 6 || next.y > stage + 6)
                continue;

            if (visited.Contains(next))
                continue;

            if (DepthFirstSearch(next, goal, path, visited))
            {
                path.Add(current);
                return true;
            }
        }

        return false;
    }
    private void CreateGrid()
    {
        MaterialPropertyBlock mpb = new();
        for (int x = 0; x < 12 + stage; x++)
        {
            for (int z = 0; z < 12 + stage; z++)
            {
                if (x < 6 || z < 6 || x > 6 + stage || z > 6 + stage)
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), prefabs[0].transform.GetChild(1).localRotation, prefabs[0].transform.GetChild(1).localScale));
                    surfaces.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, prefabs[0].transform.GetChild(0).localRotation.y, prefabs[0].transform.GetChild(0).localRotation.z)), prefabs[0].transform.GetChild(0).localScale));
                }
                else
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, 0.29f, z), prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.01f, z), prefabs[0].transform.GetChild(1).localRotation, prefabs[0].transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(prefabs[0].transform.GetChild(0).gameObject, new Vector3(x, -0.01f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    Vector2Int pos = new(x, z);
                    if (gridTiles.ContainsKey(pos))
                    {
                        mpb.SetColor("_ColorBottom", AdjustColorAccordingToTile(pos)._bottom);
                        mpb.SetColor("_ColorTop", AdjustColorAccordingToTile(pos)._top);
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(mpb);
                        colorfulTile.AddComponent<ColorfulTile>();
                        gridTiles[pos].tile = colorfulTile;
                        gridTiles[pos].OnSolution = true;
                        colorfulTile.name = "OnSolution";

                    } else
                    {
                        mpb.SetColor("_ColorBottom", ((Material)tileMaterials[Random.Range(0, tileMaterials.Length)]).GetColor("_ColorBottom"));
                        mpb.SetColor("_ColorTop", ((Material)tileMaterials[Random.Range(0, tileMaterials.Length)]).GetColor("_ColorTop"));
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(mpb);
                        gridTiles[pos] = new PlatformTile(colorfulTile.GetComponent<Renderer>().sharedMaterial, false);
                        gridTiles[pos].tile = colorfulTile;
                        colorfulTile.name = "UnSolution";
                    }
                }
                mpb.Clear();
            }
        }
        solutionPath = gridTiles.Where(value => value.Value.OnSolution).Select(key => key.Key).ToList();
        unSolutionPath = gridTiles.Where(value => !value.Value.OnSolution).Select(key => key.Key).ToList();
        renderers = gridTiles.Select(value => value.Value.tile.GetComponent<Renderer>()).ToList();
    }
    private void AdjustPlatformStatus(int status)
    {
        if (PlayerPrefs.GetInt("Vfx") == 0) return;

        var shape = weatherSystem[status].shape;
        shape.enabled = true;

        Vector3 pos = Vector3.zero;
        if (status == 0 || status == 2)
        {
            shape.scale = new Vector3(stage + 12f, stage + 12f, 1f);
            pos = new Vector3((stage + 12) / 2f, 2.5f, (stage + 12) / 2f);
        }
        else if (status == 1)
        {
            shape.scale = new Vector3(stage + 12f, 1f, stage + 12f);
            pos = new Vector3((stage + 12) / 2f, 1.8f, (stage + 12) / 2f);
        }


        weatherSystem[status].transform.position = pos;
        ParticleSystem weather = Instantiate(weatherSystem[status], pos, Quaternion.Euler(0f, 0f, 0f), transform);
        weather.name = "Weather";

    }
    private void PlaceFlags ()
    {
        GameObject start = Instantiate(prefabs[1], new Vector3(5.5f, 0.4f, 5.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        start.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = gridTiles.First().Value.material;
        GameObject evacuation = Instantiate(prefabs[1], new Vector3(stage + 6 + 0.5f, 0.6f, stage + 6 + 0.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        evacuation.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = gridTiles.Last().Value.material;
        progress = true;
    }
    public Material GetTileMatAtPosition (Vector2Int pos)
    {
        return gridTiles.ContainsKey(pos) ? gridTiles[pos].material : (Material)tileMaterials[6];
    }
    public void Replace(Vector2Int pos, GameObject spike)
    {
        Destroy(gridTiles[pos].tile);
        gridTiles[pos].tile = spike;
        renderers = gridTiles.Select(value => value.Value.tile.GetComponent<Renderer>()).ToList();
    }
    public void CreateDynamicPath()
    {
        // If the stage is even , it can be placed both vertical and horizontal. It will be implemented at the Stage 6

        // One cell that is surrounded the center of blade. At the Stage 10 will be inmplemented

        // According to the stage , it can be placed each region but it must not be conflict with spike. At the odd stage will be implemented (7,9)
        if (stage % 2 == 0)
        {
            int middlePoint = stage / 2 + 6;
            int lastPoint = stage + 6;
            switch (stage)
            {
                case 6:
                    for (int x = 6; x <= lastPoint; x++)
                        if (solutionPath.Contains(new Vector2Int(x, middlePoint)) && dynamicPath.Count < 2)
                            dynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (solutionPath.Contains(new Vector2Int(middlePoint, y)) && dynamicPath.Count < 4)
                            dynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                case 8:
                    for (int x = 6; x <= lastPoint; x++)
                        if (solutionPath.Contains(new Vector2Int(x, middlePoint)) && dynamicPath.Count < 3)
                            dynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (solutionPath.Contains(new Vector2Int(middlePoint, y)) && dynamicPath.Count < 6)
                            dynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                case 10:
                    List<GameObject> placed = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (placed.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)placed[0].transform.position.x, (int)placed[0].transform.position.z);
                        if (solutionPath.Contains(pos + Vector2Int.left))
                            dynamicPath.Add(pos + Vector2Int.left);
                        if (solutionPath.Contains(pos + Vector2Int.right))
                            dynamicPath.Add(pos + Vector2Int.right);
                        if (solutionPath.Contains(pos + Vector2Int.up))
                            dynamicPath.Add(pos + Vector2Int.up);
                        if (solutionPath.Contains(pos + Vector2Int.down))
                            dynamicPath.Add(pos + Vector2Int.down);
                        placed.Remove(placed[0]);
                    }
                    break;
                case 12:
                    for (int x = 6; x <= lastPoint; x++)
                        if (solutionPath.Contains(new Vector2Int(x, middlePoint)) && dynamicPath.Count < 4)
                            dynamicPath.Add(new Vector2Int(x, middlePoint));
                    for (int y = 6; y <= lastPoint; y++)
                        if (solutionPath.Contains(new Vector2Int(middlePoint, y)) && dynamicPath.Count < 7)
                            dynamicPath.Add(new Vector2Int(middlePoint, y));
                    break;
                default:
                    return;
            }
            if (PlayerPrefs.GetInt("Vfx") == 1)
                dynamicPath.Where(d => gridTiles[d].tile.GetComponent<ColorfulTile>() != null).ToList().ForEach(d => gridTiles[d].tile.GetComponent<ColorfulTile>().AddSmokeVfx(AdjustColorAccordingToTile(d),_smoke_Burst));
            LaunchDynamicPath();
        }
        else
        {
            List<GameObject> referanced;
            switch (stage)
            {
                case 7:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Spikes.ToList();
                    while (referanced.Count > 0)
                    {
                        int regionCount = 1;
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (solutionPath.Contains(pos + Vector2Int.left) && regionCount > 0)
                        {
                            dynamicPath.Add(pos + Vector2Int.left);
                            regionCount--;
                        }
                        if (solutionPath.Contains(pos + Vector2Int.right) && regionCount > 0)
                        {
                            dynamicPath.Add(pos + Vector2Int.right);
                            regionCount--;
                        }
                        if (solutionPath.Contains(pos + Vector2Int.up) && regionCount > 0)
                        {
                            dynamicPath.Add(pos + Vector2Int.up);
                            regionCount--;
                        }
                        if (solutionPath.Contains(pos + Vector2Int.down) && regionCount > 0)
                        {
                            dynamicPath.Add(pos + Vector2Int.down);
                            regionCount--;
                        }
                        referanced.Remove(referanced[0]);
                    }
                    break;
                case 9:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (referanced.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (solutionPath.Contains(pos + Vector2Int.left))
                            dynamicPath.Add(pos + Vector2Int.left);
                        if (solutionPath.Contains(pos + Vector2Int.right))
                            dynamicPath.Add(pos + Vector2Int.right);
                        if (solutionPath.Contains(pos + Vector2Int.up))
                            dynamicPath.Add(pos + Vector2Int.up);
                        if (solutionPath.Contains(pos + Vector2Int.down))
                            dynamicPath.Add(pos + Vector2Int.down);
                        referanced.Remove(referanced[0]);
                    }
                    break;
                case 11:
                    referanced = GameObject.Find("ObstacleManager").GetComponent<ObstacleManager>().Blade.ToList();
                    while (referanced.Count > 0)
                    {
                        Vector2Int pos = new Vector2Int((int)referanced[0].transform.position.x, (int)referanced[0].transform.position.z);
                        if (solutionPath.Contains(pos + Vector2Int.left))
                            dynamicPath.Add(pos + Vector2Int.left);
                        if (solutionPath.Contains(pos + Vector2Int.right))
                            dynamicPath.Add(pos + Vector2Int.right);
                        if (solutionPath.Contains(pos + Vector2Int.up))
                            dynamicPath.Add(pos + Vector2Int.up);
                        if (solutionPath.Contains(pos + Vector2Int.down))
                            dynamicPath.Add(pos + Vector2Int.down);
                        referanced.Remove(referanced[0]);
                    }
                    break;
                default:
                    return;
            }
            if (PlayerPrefs.GetInt("Vfx") == 1)
                dynamicPath.Where(d => gridTiles[d].tile.GetComponent<ColorfulTile>() != null).ToList().ForEach(d => gridTiles[d].tile.GetComponent<ColorfulTile>().AddSmokeVfx(AdjustColorAccordingToTile(d),_smoke_Burst));
            LaunchDynamicPath();
        }
    }
    private void LaunchDynamicPath()
    {
        foreach (var pos in dynamicPath)
            gridTiles[pos].tile.GetComponent<ColorfulTile>().RepeatColor((Material)tileMaterials[Random.Range(0, tileMaterials.Length)], gridTiles[pos].material);
    }
    public void SetTileMaterialAsDynamic (Material dynamic , Vector2Int pos)
    {
        gridTiles[pos].material = dynamic;
    } 
    public MaterialProperties AdjustColorAccordingToTile (Vector2Int pos)
    {
        Color _bottomColor = gridTiles[pos].material.GetColor("_ColorBottom");
        Color _topColor = gridTiles[pos].material.GetColor("_ColorTop");

        MaterialProperties _properties = new MaterialProperties(_bottomColor, _topColor);

        return _properties;
    }
    public void AdjustColorOfClue (Vector2Int pos)
    {
        Gradient _arrows = new Gradient();
        Gradient _glow = new Gradient();

        var _arrowsColorModule = _clue.colorOverLifetime;
        var glowColorModule = _clue.transform.GetChild(0).GetComponent<ParticleSystem>().colorOverLifetime;

        _arrows.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(AdjustColorAccordingToTile(pos)._bottom,0f),
            new GradientColorKey(AdjustColorAccordingToTile(pos)._top , 1f)
        };

        _glow.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(AdjustColorAccordingToTile(pos)._bottom, 0f),
            new GradientColorKey(AdjustColorAccordingToTile(pos)._top, 1f)
        };

        _arrowsColorModule.color = new ParticleSystem.MinMaxGradient(_arrows);
        glowColorModule.color = new ParticleSystem.MinMaxGradient(_glow);
    }
}

public struct MaterialProperties
{
    public Color _bottom { get; private set; }
    public Color _top { get; private set; }

    public MaterialProperties (Color _bottom, Color _top)
    {
        this._bottom = _bottom;
        this._top = _top;
    }
}

public class PlatformTile
{
    public Vector2Int position;
    public Material material;
    public GameObject tile;
    public bool OnSolution;

    public PlatformTile (Material material, bool OnSolution)
    {
        this.material = material;
        this.OnSolution = OnSolution;
    }

}
