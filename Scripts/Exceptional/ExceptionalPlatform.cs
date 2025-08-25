using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ExceptionalPlatform : MonoBehaviour
{

    public int _Stage;

    public bool Progress = false;

    [SerializeField]
    protected GameObject[] Prefabs;

    [SerializeField]
    protected ParticleSystem[] Weather;

    [SerializeField]
    protected ParticleSystem Smoke_Burst;

    protected List<Object> AllMaterials = new();

    public List<Vector2Int> SolutionPath = new();

    public List<Vector2Int> UnSolution = new();

    protected List<Matrix4x4> Tile = new();
    protected List<Matrix4x4> Frame = new();
    protected List<Matrix4x4> Surface = new();

    protected List<Renderer> Renderers = new();

    protected Mesh FrameMesh, TileMesh, SurfacesMesh;
    protected Material TileMat, FrameMat, SurfacesMat;


    protected void ParallelFrustumCulling()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        ConcurrentBag<Matrix4x4> visibleTile = new ConcurrentBag<Matrix4x4>();
        ConcurrentBag<Matrix4x4> visibleFrame = new ConcurrentBag<Matrix4x4>();
        ConcurrentBag<Matrix4x4> visibleSurfaces = new ConcurrentBag<Matrix4x4>();

        int total = Tile.Count;

        Parallel.For(0, total, index =>
        {
            if (index < Surface.Count)
            {
                Vector3 pos = Surface[index].GetColumn(3);
                Bounds bounds = new Bounds(pos, Vector3.one);
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

        foreach (var renderer in Renderers)
            renderer.enabled = GeometryUtility.TestPlanesAABB(Frustum, renderer.bounds);
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

        bool found = DepthFirstSearch(start, goal, path, visited);
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
        TileMat = Prefabs[0].GetComponent<MeshRenderer>().sharedMaterial;

        SurfacesMesh = Prefabs[0].transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        SurfacesMat = Prefabs[0].transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

        FrameMesh = Prefabs[0].transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        FrameMat = Prefabs[0].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;
    }

    protected virtual void PlaceFlag() { }
    protected virtual void InitializeWeather(int status) { }
    protected virtual void InitializeSolution() { }
    protected virtual void RandomMaterialSelection() { }
    protected virtual void CreateSolutionPath() { }
    public virtual void CreateDynamics() { }
    public virtual Material GetTileMat(Vector2Int Position) { return null; }
    public virtual void SetTileMat(Material Mat, Vector2Int Position) { }
    public virtual MaterialProperties AdjustColorAccordingToTile(Vector2Int pos) { return new MaterialProperties(); }

}
