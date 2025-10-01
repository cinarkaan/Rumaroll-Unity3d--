using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ExceptionalPlatform : MonoBehaviour
{
    public bool Progress { get; private set; }

    public int Stage_ { get; protected set; }

    [SerializeField] protected GameObject[] Prefabs;

    [SerializeField] protected ParticleSystem[] Weather;

    [SerializeField] protected ParticleSystem Smoke_Burst;

    [SerializeField] protected Transform Water;

    [SerializeField] protected Light MainLight;
    public Light MainLight_ => MainLight;

    public List<Vector2Int> SolutionPath = new(140);
    public List<Vector2Int> UnSolution = new(140);

    protected List<Object> AllMaterials = new(20); // [0]=Bottom, [1]=Top, [2]=Front, [3]=Back, [4]=Left, [5]=Right

    protected List<Matrix4x4> CurvedTile = new(55), CurvedFrame = new(170), CurvedWhite = new(170);
    
    protected List<Matrix4x4> Fence = new(55);

    private readonly List<Matrix4x4> SelectedTile = new(55), SelectedFrame = new(170), SelectedWhite = new(170), SelectedFence = new(100);

    private readonly ConcurrentBag<Matrix4x4> VisibleCurvedTile = new(), VisibleCurvedFrame = new(), VisibleCurvedWhite = new();
    
    private readonly ConcurrentBag<Matrix4x4> VisibleFence = new();

    protected List<Renderer> PlatformObjects = new(180);

    protected readonly Dictionary<Vector2Int, PlatformTile> GridTiles = new(180);

    protected readonly HashSet<Vector2Int> DynamicPath = new(16);
    
    protected Mesh CurvedFrameMesh, CurvedTileMesh, CurvedWhiteMesh, FenceMesh;

    protected Material CurvedTileMat, CurvedFrameMat, WhiteMat, FenceMat;

    protected bool AllActivated = false;

    protected int Bound = 0;

    protected Plane[] Frustum;
    public Plane[] Frustum_ => Frustum;

    protected void ParallelFrustumCulling()
    {   
        int total = CurvedFrame.Count;

        VisibleCurvedTile.Clear();
        VisibleCurvedFrame.Clear();
        VisibleCurvedWhite.Clear();
        VisibleFence.Clear();

        SelectedTile.Clear();
        SelectedFrame.Clear();
        SelectedWhite.Clear();
        SelectedFence.Clear();

        Parallel.For(0, total, index =>
        {
            if (index < CurvedFrame.Count)
            {
                Vector3 curvedframepos = CurvedFrame[index].GetColumn(3);
                curvedframepos.y = 0.45f;
                Vector3 curvedwhitepos = CurvedWhite[index].GetColumn(3);
                curvedwhitepos.y = 0.45f;

                if (IsBoundsInsideFrustum(new Bounds(curvedframepos, new Vector3(0.65f, 0f, 0.65f)), Frustum))
                    VisibleCurvedFrame.Add(CurvedFrame[index]);
                if (IsBoundsInsideFrustum(new Bounds(curvedwhitepos, new Vector3(0.65f, 0f, 0.65f)), Frustum))
                    VisibleCurvedWhite.Add(CurvedWhite[index]);
            }
            if (index < CurvedTile.Count)
            {
                Vector3 curvedtilepos = CurvedTile[index].GetColumn(3);
                curvedtilepos.y = 0.45f;
                if (IsBoundsInsideFrustum(new Bounds(curvedtilepos, new Vector3(0.65f, 0f, 0.65f)), Frustum))
                    VisibleCurvedTile.Add(CurvedTile[index]);
            }
            if (index < Fence.Count)
            {
                Vector3 fencepos = Fence[index].GetColumn(3);
                if (IsBoundsInsideFrustum(new Bounds(fencepos, new Vector3(0.47f, 0f, 0.02f)), Frustum))
                    VisibleFence.Add(Fence[index]);
            }
        });

        SelectedTile.AddRange(VisibleCurvedTile);
        SelectedFrame.AddRange(VisibleCurvedFrame);
        SelectedWhite.AddRange(VisibleCurvedWhite);
        SelectedFence.AddRange(VisibleFence);

        Graphics.DrawMeshInstanced(CurvedTileMesh, 0, CurvedTileMat, SelectedTile);
        Graphics.DrawMeshInstanced(CurvedFrameMesh, 0, CurvedFrameMat, SelectedFrame);
        Graphics.DrawMeshInstanced(CurvedWhiteMesh, 0, WhiteMat, SelectedWhite);
        
        Graphics.DrawMeshInstanced(FenceMesh, 0, FenceMat, SelectedFence);
    }
    protected void FrustumCullingForColorfuls()
    {
        for (int i = 0; i < PlatformObjects.Count; i++)
            PlatformObjects[i].enabled = GeometryUtility.TestPlanesAABB(Frustum, PlatformObjects[i].bounds);
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
        List<Vector2Int> path = new();
        HashSet<Vector2Int> visited = new();

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

            if (next.x < 6 || next.y < 6 || next.x > Stage_ + 6 || next.y > Stage_ + 6)
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

        FenceMesh = Prefabs[3].GetComponent<MeshFilter>().sharedMesh;
        FenceMat = Prefabs[3].GetComponent<Renderer>().sharedMaterial;

        Bound = Stage_ + 6;

        var _offest = (Stage_ % 2 == 0) ? ((Stage_ + 12) / 2) - 0.5f : (Stage_ + 12) / 2;

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
        PlatformObjects.Add(start.GetComponent<Renderer>());
        PlatformObjects.Add(start.transform.GetChild(0).GetComponent<Renderer>());
        PlatformObjects.Add(start.transform.GetChild(1).GetComponent<Renderer>());

        GameObject evacuation = Instantiate(Prefabs[1], new Vector3(Stage_ + 6 + 0.45f, 1.3f, Stage_ + 6 + 0.45f), Quaternion.Euler(0f, 45f, 0f), transform);
        FlagMPB.SetColor("_ColorBottom", new Color(0.1773585f, 0.1773585f, 0.1773585f));
        FlagMPB.SetColor("_ColorTop", new Color(0.3735847f, 0.3735847f, 0.3735847f));
        evacuation.transform.GetChild(0).GetComponent<Renderer>().SetPropertyBlock(FlagMPB);
        PlatformObjects.Add(evacuation.GetComponent<Renderer>());
        PlatformObjects.Add(evacuation.transform.GetChild(0).GetComponent<Renderer>());
        PlatformObjects.Add(evacuation.transform.GetChild(1).GetComponent<Renderer>());

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
    public void Replace(List<Vector2Int> pos, List<GameObject> spike)
    {
        for (int i = 0; i < spike.Count; i++)
        {
            Destroy(GridTiles[pos[i]].tile);
            GridTiles[pos[i]].tile = spike[i].transform.GetChild(0).gameObject;
        }
        for (int i = 0; i < Mathf.Pow(Stage_ + 1, 2); i++)
            PlatformObjects[i] = GridTiles.ElementAt(i).Value.tile.GetComponent<Renderer>();
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
