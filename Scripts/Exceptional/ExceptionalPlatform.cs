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

    protected List<Object> AllMaterials = new(); // [0]=Bottom, [1]=Top, [2]=Front, [3]=Back, [4]=Left, [5]=Right

    protected List<Matrix4x4> CurvedTile = new(), CurvedFrame = new(), CurvedWhite = new();
    protected List<Matrix4x4> Tile = new(), Frame = new(), Surface = new();

    protected List<Matrix4x4> Fence = new();

    protected ConcurrentBag<Matrix4x4> VisibleCurvedTile = new(), VisibleCurvedFrame = new(), VisibleCurvedWhite = new();
    protected ConcurrentBag<Matrix4x4> VisibleTile = new(), VisibleFrame = new(), VisibleSurfaces = new();

    protected ConcurrentBag<Matrix4x4> VisibleFence = new();

    protected List<Renderer> Renderers = new();

    protected readonly Dictionary<Vector2Int, PlatformTile> GridTiles = new();

    protected readonly HashSet<Vector2Int> DynamicPath = new();
    
    protected Mesh CurvedFrameMesh, CurvedTileMesh, CurvedWhiteMesh, TileMesh, FrameMesh, SurfaceMesh, FenceMesh;

    protected Material CurvedTileMat, CurvedFrameMat, WhiteMat, TileMat, FrameMat, SurfaceMat, FenceMat;

    protected bool AllActivated = false;

    protected int Bound = 0;

    protected void ParallelFrustumCulling()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        int total = Tile.Count;

        VisibleCurvedTile.Clear();
        VisibleCurvedFrame.Clear();
        VisibleCurvedWhite.Clear();

        VisibleFence.Clear();
        
        VisibleTile.Clear();
        VisibleFrame.Clear();
        VisibleSurfaces.Clear();

        Parallel.For(0, total, index =>
        {
            if (index < CurvedTile.Count)
            {

                Vector3 curvedtilepos = CurvedTile[index].GetColumn(3);
                Vector3 curvedframepos = CurvedFrame[index].GetColumn(3);
                Vector3 curvedwhitepos = CurvedWhite[index].GetColumn(3);
                
                if (IsBoundsInsideFrustum(new Bounds(curvedtilepos, Vector3.one), frustumPlanes))
                    VisibleCurvedTile.Add(CurvedTile[index]);
                if (IsBoundsInsideFrustum(new Bounds(curvedframepos, Vector3.one), frustumPlanes))
                    VisibleCurvedFrame.Add(CurvedFrame[index]);
                if (IsBoundsInsideFrustum(new Bounds(curvedwhitepos, Vector3.one), frustumPlanes))
                    VisibleCurvedWhite.Add(CurvedWhite[index]);
            }

            if (index < Fence.Count)
            {
                Vector3 fencepos = Fence[index].GetColumn(3);
                if (IsBoundsInsideFrustum(new Bounds(fencepos, Vector3.one), frustumPlanes))
                    VisibleFence.Add(Fence[index]);
            }

            Vector3 tilepos = Tile[index].GetColumn(3);
            Vector3 framepos = Frame[index].GetColumn(3);
            Vector3 surfacepos = Surface[index].GetColumn(3);


            if (IsBoundsInsideFrustum(new Bounds(tilepos, Vector3.one), frustumPlanes))
                VisibleTile.Add(Tile[index]);
            if (IsBoundsInsideFrustum(new Bounds(framepos, Vector3.one), frustumPlanes))
                VisibleFrame.Add(Frame[index]);
            if (IsBoundsInsideFrustum(new Bounds(surfacepos, Vector3.one), frustumPlanes))
                VisibleSurfaces.Add(Surface[index]);



        });

        Graphics.DrawMeshInstanced(TileMesh, 0, TileMat, VisibleTile.ToList());
        Graphics.DrawMeshInstanced(FrameMesh, 0, FrameMat, VisibleFrame.ToList());
        Graphics.DrawMeshInstanced(SurfaceMesh, 0, SurfaceMat, VisibleSurfaces.ToList());

        Graphics.DrawMeshInstanced(CurvedTileMesh, 0, CurvedTileMat, VisibleCurvedTile.ToList());
        Graphics.DrawMeshInstanced(CurvedFrameMesh, 0, CurvedFrameMat, VisibleCurvedFrame.ToList());
        Graphics.DrawMeshInstanced(CurvedWhiteMesh, 0, WhiteMat, VisibleCurvedWhite.ToList());
        
        Graphics.DrawMeshInstanced(FenceMesh, 0, FenceMat, VisibleFence.ToList());
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
    protected void InitializeEnvironment() // Surrounded of the platform and platfrom 
    {
        
        CurvedTileMesh = Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
        CurvedTileMat = Prefabs[0].GetComponent<Renderer>().sharedMaterial;

        CurvedFrameMesh = Prefabs[0].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        CurvedFrameMat = Prefabs[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;

        CurvedWhiteMesh = Prefabs[0].transform.GetChild(2).GetComponent<MeshFilter>().sharedMesh;
        WhiteMat = Prefabs[0].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial;

        TileMesh = Prefabs[4].GetComponent<MeshFilter>().sharedMesh;
        TileMat = Prefabs[4].GetComponent<Renderer>().sharedMaterial;

        SurfaceMesh = Prefabs[4].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        SurfaceMat = Prefabs[4].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial;

        FrameMesh = Prefabs[4].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        FrameMat = Prefabs[4].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;


        FenceMesh = Prefabs[3].GetComponent<MeshFilter>().sharedMesh;
        FenceMat = Prefabs[3].GetComponent<Renderer>().sharedMaterial;

        Bound = _Stage + 6;

        var _offest = (_Stage % 2 == 0) ? ((_Stage + 12) / 2) - 0.5f : (_Stage + 12) / 2;

        Water.position = new Vector3(_offest, Water.position.y, _offest);

        if (PlayerPrefs.GetInt("Sfx") == 0)
            foreach (Transform transform in Water.transform)
                transform.gameObject.SetActive(false);
    }
    protected void PlaceFlag()
    {
        MaterialPropertyBlock FlagMPB = new();

        GameObject start = Instantiate(Prefabs[1], new Vector3(5.55f, 1.3f, 5.55f), Quaternion.Euler(0f, 45f, 0f), transform);
        FlagMPB.SetColor("_ColorBottom", new Color(0.8415094f, 0.8415094f, 0.8415094f));
        FlagMPB.SetColor("_ColorTop", new Color(0.5924529f, 0.5924529f, 0.5924529f));
        start.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(FlagMPB);
        
        GameObject evacuation = Instantiate(Prefabs[1], new Vector3(_Stage + 6 + 0.45f, 1.3f, _Stage + 6 + 0.45f), Quaternion.Euler(0f, 45f, 0f), transform);
        FlagMPB.SetColor("_ColorBottom", new Color(0.1773585f, 0.1773585f, 0.1773585f));
        FlagMPB.SetColor("_ColorTop", new Color(0.3735847f, 0.3735847f, 0.3735847f));
        evacuation.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(FlagMPB);
        Progress = true;
    }
    protected void PlaceFence(int x, int z)
    {
        if (x > Bound || z > Bound) return;

        if (x == 5 && z >= 6)
            Fence.Add(Matrix4x4.TRS(new Vector3(6 - 0.44f, 0.43f, z), Quaternion.Euler(new Vector3(0f, 90f, 0)), Vector3.one));
        else if (z == 5 && x >= 6)
            Fence.Add(Matrix4x4.TRS(new Vector3(x, 0.43f, 6 - 0.44f), Quaternion.Euler(new Vector3(0f, 180f, 0)), Vector3.one));
        if (x == Bound && z >= 6)
            Fence.Add(Matrix4x4.TRS(new Vector3(Bound + 0.44f, 0.43f, z), Quaternion.Euler(new Vector3(0f, 90f, 0)), Vector3.one));
        if (z == Bound && x >= 6)
            Fence.Add(Matrix4x4.TRS(new Vector3(x, 0.43f, Bound + 0.44f), Quaternion.Euler(new Vector3(0f, 180f, 0)), Vector3.one));
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
