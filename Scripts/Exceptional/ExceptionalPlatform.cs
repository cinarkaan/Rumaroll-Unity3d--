using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ExceptionalPlatform : MonoBehaviour
{
    public bool Progress { get; private set; }

    protected int _Stage;

    [SerializeField]
    protected GameObject[] Prefabs;

    [SerializeField]
    protected ParticleSystem[] Weather;

    [SerializeField]
    protected ParticleSystem Smoke_Burst;

    [SerializeField]
    protected Transform Water;


    public List<Vector2Int> SolutionPath = new();
    public List<Vector2Int> UnSolution = new();

    protected List<Matrix4x4> Tile = new();
    protected List<Matrix4x4> Frame = new();
    protected List<Matrix4x4> Surface = new();

    protected ConcurrentBag<Matrix4x4> visibleTile = new();
    protected ConcurrentBag<Matrix4x4> visibleFrame = new();
    protected ConcurrentBag<Matrix4x4> visibleSurfaces = new();

    protected List<Renderer> Renderers = new();

    protected readonly Dictionary<Vector2Int, PlatformTile> GridTiles = new();

    protected readonly HashSet<Vector2Int> DynamicPath = new();

    protected List<Object> AllMaterials = new(); // [0]=Bottom, [1]=Top, [2]=Front, [3]=Back, [4]=Left, [5]=Right

    protected Mesh FrameMesh, TileMesh, SurfacesMesh;

    protected Material TileMat, FrameMat, SurfacesMat;

    protected bool AllActivated = false;

    protected void ParallelFrustumCulling()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        int total = Tile.Count;

        visibleTile.Clear();
        visibleFrame.Clear();
        visibleSurfaces.Clear();

        Parallel.For(0, total, index =>
        {
            if (index < Surface.Count)
            {
                Vector3 pos = Surface[index].GetColumn(3);
                Bounds bounds = new(pos, Vector3.one);
                if (IsBoundsInsideFrustum(bounds, frustumPlanes))
                    visibleSurfaces.Add(Surface[index]);
            }

            Vector3 tilepos = Tile[index].GetColumn(3);
            Vector3 framepos = Frame[index].GetColumn(3);

            if (IsBoundsInsideFrustum(new Bounds(tilepos, Vector3.one), frustumPlanes))
                visibleTile.Add(Tile[index]);

            if (IsBoundsInsideFrustum(new Bounds(framepos, Vector3.one), frustumPlanes))
                visibleFrame.Add(Frame[index]);
        });

        Graphics.DrawMeshInstanced(TileMesh, 0, TileMat, visibleTile.ToList());
        Graphics.DrawMeshInstanced(FrameMesh, 0, FrameMat, visibleFrame.ToList());
        Graphics.DrawMeshInstanced(SurfacesMesh, 0, SurfacesMat, visibleSurfaces.ToList());
    }
    protected void FrustumCullingForColorfuls()
    {
        Plane[] Frustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        for(int i = 0; i < Renderers.Count; i++)
            Renderers[i].enabled = GeometryUtility.TestPlanesAABB(Frustum, Renderers[i].bounds);
    }
    protected bool IsBoundsInsideFrustum(Bounds bounds, Plane[] planes)
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
    protected List<Vector2Int> GenerateSolutionPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        DepthFirstSearch(start, goal, path, visited);
        path.Reverse();

        return path;
    }
    protected bool DepthFirstSearch(Vector2Int current, Vector2Int goal, List<Vector2Int> path, HashSet<Vector2Int> visited)
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

            if (next.x < 6 || next.y < 6 || next.x > _Stage + 6 || next.y > _Stage + 6)
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
    protected void InitializeEnvironment()
    {
        TileMesh = Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
        TileMat = Prefabs[0].GetComponent<Renderer>().sharedMaterial;

        SurfacesMesh = Prefabs[0].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        SurfacesMat = Prefabs[0].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

        FrameMesh = Prefabs[0].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        FrameMat = Prefabs[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;

        Water.position = new Vector3((_Stage + 12) / 2, Water.position.y, (_Stage + 12) / 2);
    }
    protected void PlaceFlag()
    {
        MaterialPropertyBlock FlagMPB = new();
        GameObject start = Instantiate(Prefabs[1], new Vector3(5.5f, 0.4f, 5.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        FlagMPB.SetColor("_ColorBottom", new Color(0.8415094f, 0.8415094f, 0.8415094f));
        FlagMPB.SetColor("_ColorTop", new Color(0.5924529f, 0.5924529f, 0.5924529f));
        start.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(FlagMPB);
        GameObject evacuation = Instantiate(Prefabs[1], new Vector3(_Stage + 6 + 0.5f, 0.6f, _Stage + 6 + 0.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        FlagMPB.SetColor("_ColorBottom", new Color(0.1773585f, 0.1773585f, 0.1773585f));
        FlagMPB.SetColor("_ColorTop", new Color(0.3735847f, 0.3735847f, 0.3735847f));
        evacuation.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(FlagMPB);
        Progress = true;
    }
    public Material GetTileMat(Vector2Int pos)
    {
        return GridTiles.ContainsKey(pos) ? GridTiles[pos].material : (Material)AllMaterials[6];
    }
    public void SetTileMat(Material dynamic, Vector2Int pos)
    {
        GridTiles[pos].material = dynamic;
    }
    public MaterialProperties AdjustColorAccordingToTile(Vector2Int pos)
    {
        Color _bottomColor = GridTiles[pos].material.GetColor("_ColorBottom");
        Color _topColor = GridTiles[pos].material.GetColor("_ColorTop");

        MaterialProperties _properties = new MaterialProperties(_bottomColor, _topColor);

        return _properties;
    }
    public void Replace(Vector2Int pos, GameObject spike)
    {
        Destroy(GridTiles[pos].tile);
        GridTiles[pos].tile = spike;
        Renderers = GridTiles.Select(value => value.Value.tile.GetComponent<Renderer>()).ToList();
    }
    protected virtual void InitializeWeather(int status) { }
    protected virtual void InitializeSolution() { }
    protected virtual void RandomMaterialSelection() { }
    protected virtual void CreateGrid() { }
    public virtual void CreateDynamics() { }

}
public struct MaterialProperties
{
    public Color _bottom { get; private set; }
    public Color _top { get; private set; }

    public MaterialProperties(Color _bottom, Color _top)
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

    public PlatformTile(Material material, bool OnSolution)
    {
        this.material = material;
        this.OnSolution = OnSolution;
    }

}
