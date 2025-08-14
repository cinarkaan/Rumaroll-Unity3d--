using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct CubeMaterials : INetworkSerializable, IEquatable<CubeMaterials>, IDisposable
{
    public FixedString64Bytes _surfaceMat;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _surfaceMat);
    }
    public bool Equals(CubeMaterials other)
    {
        return _surfaceMat.Equals(other._surfaceMat);
    }
    public override bool Equals(object obj)
    {
        return obj is CubeMaterials other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(_surfaceMat);
    }
    public void Dispose()
    {
        _surfaceMat = null;
    }
}
public struct Tiles : INetworkSerializable, IEquatable<Tiles> , IDisposable
{
    public Vector2Int positon;
    public FixedString64Bytes material;
    public bool onSolution;
    public bool _markAsDynamic;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref positon);
        serializer.SerializeValue(ref material);
        serializer.SerializeValue(ref onSolution);
        serializer.SerializeValue(ref _markAsDynamic);
    }
    public bool Equals(Tiles other)
    {
        return positon.Equals(other.positon) && material == other.material && onSolution == other.onSolution;
    }
    public override bool Equals(object obj)
    {
        return obj is Tiles other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(positon, material, onSolution, _markAsDynamic);
    }
    public void Dispose()
    {
        material = null;
    }
}
public struct ClientFaceIndicates : INetworkSerializable, IEquatable<ClientFaceIndicates> , IDisposable
{
    public int _faceIndicates;
    public FixedString64Bytes _material;

    public void Dispose()
    {
        _material = null;
    }

    public bool Equals(ClientFaceIndicates other)
    {
        throw new NotImplementedException();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _faceIndicates);
        serializer.SerializeValue(ref _material);
    }
}

public class NetworkPlatformManager : NetworkBehaviour
{

    [SerializeField]
    private GameObject TilePrefab;

    [SerializeField]
    private GameObject Flag;

    [SerializeField]
    private ServerManager ServerManager;

    [SerializeField]
    private ParticleSystem[] WeatherSystem;

    [SerializeField]
    private ParticleSystem Smoke_Burst;

    private GameObject Player;

    public bool Progress { get; private set; }

    public NetworkList<CubeMaterials> CubeMaterials = new NetworkList<CubeMaterials>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<Tiles> tiles = new NetworkList<Tiles>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ClientFaceIndicates> ClientCube = new NetworkList<ClientFaceIndicates>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> WeatherCode = new NetworkVariable<int>(0);

    private Dictionary<Vector2Int, Material> LocalTiles = new Dictionary<Vector2Int, Material>();

    public List<Vector2Int> Solution { get; private set; }
    public List<Vector2Int> UnSolution { get; private set; }

    private Dictionary<Vector2Int,ColorfulTile> dynamics = new Dictionary<Vector2Int,ColorfulTile>();

    private List<Renderer> TilesRenderer = new List<Renderer>();

    private List<UnityEngine.Object> allMaterials;

    private Mesh _frame, _tile, _surfaces;
    private Material tileMat, frameMat, surfacesMat;

    private List<Matrix4x4> tile = new();
    private List<Matrix4x4> frame = new();
    private List<Matrix4x4> surfaces = new();

    MaterialPropertyBlock materialPropertyBlock;

    private void Start()
    {
        Solution = new List<Vector2Int>();
        UnSolution = new List<Vector2Int>();
    }
    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitUntilServer());
    }
    private IEnumerator WaitUntilServer ()
    {
        yield return new WaitUntil(() => ServerManager.Stage.Value != 0);

        AdjustWeatherStatus();

        if (ServerManager.Manager.IsHost)
        {
            Player = GameObject.Find("Host");
            Player.GetComponent<NetworkCubeController>().target = new Vector2Int(ServerManager.Stage.Value + 6, ServerManager.Stage.Value + 6);
        }
        else
        {
            yield return new WaitUntil(() => GameObject.Find("Client") != null);
            Player = GameObject.Find("Client");
        }

        RandomMaterialSelection();

        if (ServerManager.Manager.IsHost)
        {
            CreateSolutionPath();
            CreateDynamics();
            InitializePlatformAsHost();
        }
        else
            InitializePlatformAsClient();

        ServerManager.Manager.OnClientConnectedCallback += SetMaterialForEach;
    }
    void Update()
    {
        if (Progress && NetworkUIController.currentIndex == 1)
        {
            ParallelFrustumCulling();
            FrustumCullingForColorfuls();
        }
        else
        {
            if (Progress)
            {
                Graphics.DrawMeshInstanced(_tile, 0, tileMat, tile);
                Graphics.DrawMeshInstanced(_frame, 0, frameMat, frame);
                Graphics.DrawMeshInstanced(_surfaces, 0, surfacesMat, surfaces);
                foreach (var tile in TilesRenderer)
                {
                    var renderer = tile.GetComponent<Renderer>();
                    if (!renderer.enabled)
                        renderer.enabled = true;
                }
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

        Parallel.For(0, total, index =>
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
    private void FrustumCullingForColorfuls()
    {
        Plane[] Frustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        foreach (var renderer in TilesRenderer)
            renderer.enabled = GeometryUtility.TestPlanesAABB(Frustum, renderer.bounds);
    }
    public void RandomMaterialSelection ()
    {
        allMaterials = Resources.LoadAll("Gradient", typeof(Material)).ToList();

        if (ServerManager.Manager.IsHost)
        {
            HashSet<UnityEngine.Object> selected = new HashSet<UnityEngine.Object>();

            while (selected.Count < 6)
            {
                bool isAdded = selected.Add(allMaterials[UnityEngine.Random.Range(0, allMaterials.Count)]);
                if (isAdded)
                {
                    CubeMaterials materials = new CubeMaterials
                    {
                        _surfaceMat = ((Material)selected.Last()).name
                    };
                    CubeMaterials.Add(materials);
                }
            }
            if (ServerManager.Manager.IsHost)
            {
                Player.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[0];
                Player.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[1];
                Player.transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[2];
                Player.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[3];
                Player.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[4];
                Player.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)selected.ToArray()[5];
            }
            selected.Clear();
        } else
        {
            for (int index = 0; index < 6; index++)
            {
                if (index == 0)
                    Player.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 4)
                    Player.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 3)
                    Player.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 5)
                    Player.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else
                    Player.transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                
            }
        }

    }
    private void CreateSolutionPath()
    {
        Vector2Int goal = new Vector2Int(6 + ServerManager.Stage.Value, 6 + ServerManager.Stage.Value);
        List<Vector2Int> solutionPath = GenerateSolutionPath(new Vector2Int(6, 6), goal);

        CubeSimulator cubeSim = new CubeSimulator();

        Tiles tile = new()
        {
            positon = solutionPath[0],
            material = CubeMaterials[0]._surfaceMat,
            onSolution = true
        };
        tiles.Add(tile);
        
        for (int i = 1; i < solutionPath.Count; i++)
        {
            Vector2Int prev = solutionPath[i - 1];
            Vector2Int current = solutionPath[i];
            Vector3 moveDir = new Vector3(current.x - prev.x, 0, current.y - prev.y);

            cubeSim.Roll(moveDir);
            int bottomFace = cubeSim.faceIndices[0];
            Tiles t = new()
            {
                positon = solutionPath[i],
                material = CubeMaterials[bottomFace]._surfaceMat,
                onSolution = true
            };
            tiles.Add(t);
            if (i == solutionPath.Count - 1)
            {
                for (int index = 0; index < 6; index++)
                {
                    ClientFaceIndicates clientFaceMaterials = new()
                    {
                        _faceIndicates = cubeSim.faceIndices[index],
                        _material = CubeMaterials[cubeSim.faceIndices[index]]._surfaceMat
                    };
                    ClientCube.Add(clientFaceMaterials);
                }

            }
        }
        solutionPath.Clear();
    }
    private List<Vector2Int> GenerateSolutionPath(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        bool found = DepthFirstSearch(start, goal, path, visited);
        path.Reverse(); 

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
        }.OrderBy(x => UnityEngine.Random.value).ToList(); 

        foreach (var dir in directions)
        {
            Vector2Int next = current + dir;

            if (next.x < 6 || next.y < 6 || next.x > ServerManager.Stage.Value + 6 || next.y > ServerManager.Stage.Value + 6)
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
    private void CreateDynamics()
    {
        UniqueRandomGenerator uniqueRandomGenerator = new UniqueRandomGenerator();
        switch (ServerManager.Stage.Value)
        {
            case 10:
                uniqueRandomGenerator = SetDynamicProperties(4);
                break;
            case 11:
                uniqueRandomGenerator = SetDynamicProperties(7);
                break;
            case 12:
                uniqueRandomGenerator = SetDynamicProperties(10);
                break;
            default:
                break;
        }
        foreach (var item in uniqueRandomGenerator.uniqueRandoms)
        {
            Tiles tile = tiles[item];
            tile._markAsDynamic = true;
            tiles[item] = tile;
        }            
    }
    private UniqueRandomGenerator SetDynamicProperties (int Count)
    {
        return new UniqueRandomGenerator(1, tiles.Count - 1, Count);
    }
    private void InitializeEnvironment ()
    {
        materialPropertyBlock = new MaterialPropertyBlock();

        _tile = TilePrefab.GetComponent<MeshFilter>().sharedMesh;
        tileMat = TilePrefab.GetComponent<MeshRenderer>().sharedMaterial;

        _surfaces = TilePrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
        surfacesMat = TilePrefab.transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial;

        _frame = TilePrefab.transform.GetChild(1).GetComponent<MeshFilter>().sharedMesh;
        frameMat = TilePrefab.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial;
    }
    private void PlaceFlag ()
    {
        GameObject start = Instantiate(Flag, new Vector3(5.5f, 0.4f, 5.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        start.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => LocalTiles[new Vector2Int(6, 6)]);
        GameObject finish = Instantiate(Flag, new Vector3(ServerManager.Stage.Value + 6 + 0.5f, 0.6f, ServerManager.Stage.Value + 6 + 0.5f), Quaternion.Euler(0f, 45f, 0f), transform);
        finish.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => LocalTiles[new Vector2Int(6 + ServerManager.Stage.Value, 6 + ServerManager.Stage.Value)]);

    }
    private void InitializePlatformAsHost()
    {
        Material temp;

        InitializeEnvironment();

        for (int x = 0; x < ServerManager.Stage.Value + 12; x++)
            for (int z = 0; z < ServerManager.Stage.Value + 12; z++)
            {
                if (x < 6 || z < 6 || x > 6 + ServerManager.Stage.Value || z > 6 + ServerManager.Stage.Value)
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), TilePrefab.transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), TilePrefab.transform.GetChild(1).localRotation, TilePrefab.transform.GetChild(1).localScale));
                    surfaces.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, TilePrefab.transform.GetChild(0).localRotation.y, TilePrefab.transform.GetChild(0).localRotation.z)), TilePrefab.transform.GetChild(0).localScale));
                } else
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, 0.29f, z), TilePrefab.transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.01f, z), TilePrefab.transform.GetChild(1).localRotation, TilePrefab.transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(TilePrefab.transform.GetChild(0).gameObject, new Vector3(x, -0.01f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    TilesRenderer.Add(colorfulTile.GetComponent<Renderer>());
                    var _colorfulTile = HasAtTile(new Vector2Int(x, z), ref colorfulTile);
                    if (_colorfulTile != null)
                    {
                        temp = (Material)allMaterials.Find(m => ((Material)m).name.Equals(_colorfulTile.Value.material.ToString()));
                        materialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom"));
                        materialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop"));
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
                        Solution.Add(new Vector2Int(x, z));
                    }
                    else
                    {
                        Tiles t = new()
                        {
                            positon = new Vector2Int(x, z),
                            material = CubeMaterials[UnityEngine.Random.Range(0, CubeMaterials.Count)]._surfaceMat,
                            onSolution = false
                        };
                        temp = (Material)allMaterials.Find(m => ((Material)m).name.Equals(t.material.ToString()));
                        materialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom"));
                        materialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop"));
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
                        tiles.Add(t);
                        UnSolution.Add(new Vector2Int(x, z));
                    }
                    LocalTiles[new Vector2Int(x, z)] = temp;
                }
            }
        PlaceFlag();
        Progress = true;
    }
    private void InitializePlatformAsClient()
    {
        InitializeEnvironment();

        for (int x = 0; x < ServerManager.Stage.Value + 12; x++)
            for (int z = 0; z < ServerManager.Stage.Value + 12; z++)
            {
                if (x < 6 || z < 6 || x > 6 + ServerManager.Stage.Value || z > 6 + ServerManager.Stage.Value)
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), TilePrefab.transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), TilePrefab.transform.GetChild(1).localRotation, TilePrefab.transform.GetChild(1).localScale));
                    surfaces.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, TilePrefab.transform.GetChild(0).localRotation.y, TilePrefab.transform.GetChild(0).localRotation.z)), TilePrefab.transform.GetChild(0).localScale));
                }
                else
                {
                    tile.Add(Matrix4x4.TRS(new Vector3(x, 0.29f, z), TilePrefab.transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    frame.Add(Matrix4x4.TRS(new Vector3(x, -0.01f, z), TilePrefab.transform.GetChild(1).localRotation, TilePrefab.transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(TilePrefab.transform.GetChild(0).gameObject, new Vector3(x, -0.01f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    TilesRenderer.Add(colorfulTile.GetComponent<Renderer>());
                    Material _tileMat = GetTileMat(new Vector2Int(x, z), ref colorfulTile);
                    materialPropertyBlock.SetColor("_ColorBottom", _tileMat.GetColor("_ColorBottom"));
                    materialPropertyBlock.SetColor("_ColorTop", _tileMat.GetColor("_ColorTop"));
                    colorfulTile.GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
                    LocalTiles[new Vector2Int(x, z)] = _tileMat;
                }
            }
        SetMaterialOfRival();
        PlaceFlag();
        Progress = true;
    }
    public ServerManager getManager()
    {
        return this.ServerManager;
    }
    public Tiles? HasAtTile (Vector2Int pos,ref GameObject colorfulTile)
    {
        foreach (Tiles t in tiles)
        {
            if (t.positon.Equals(pos))
            {
                if (t._markAsDynamic)
                    dynamics.Add(pos, colorfulTile.AddComponent<ColorfulTile>());
                return t;
            }
        }
        return null;
    }
    public Material GetTileMat(Vector2Int pos, ref GameObject _colorfulTile)
    {
        foreach (Tiles t in tiles)
        {
            if (t.positon.Equals(pos))
            {
                if (t.onSolution && t._markAsDynamic)
                {
                    dynamics.Add(pos, _colorfulTile.AddComponent<ColorfulTile>());
                    Solution.Add(pos);
                }
                else
                    UnSolution.Add(pos);
                    return (Material)allMaterials.Find(m => ((Material)m).name.Equals(t.material.ToString()));
            }
        }
        return null;
    }
    public Material FindTileMat (Vector2Int pos)
    {
        return LocalTiles[pos];
    }
    public void SetTileMaterialAsDynamic(Material mat, Vector2Int pos)
    {
        LocalTiles[pos] = mat;
    }
    private void SetMaterialOfRival ()
    {
        if (ServerManager.IsHost)
        {
            GameObject client = GameObject.Find("Client");
            for (int index = 0; index < 6; index++)
            {
                if (index == 0)
                    client.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 4)
                    client.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 3)
                    client.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else if (index == 5)
                    client.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
                else
                    client.transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(ClientCube[index]._material.ToString()));
            }
        } else
        {
            GameObject host = GameObject.Find("Host");
            host.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[0]._surfaceMat.ToString()));
            host.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[1]._surfaceMat.ToString()));
            host.transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[2]._surfaceMat.ToString()));
            host.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[3]._surfaceMat.ToString()));
            host.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[4]._surfaceMat.ToString()));
            host.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)allMaterials.Find(m => ((Material)m).name.Equals(CubeMaterials[5]._surfaceMat.ToString()));
        }
    }
    private void SetMaterialForEach (ulong clientID)
    {
        StartCoroutine(RunAndWaitForClient(clientID));
    }
    private IEnumerator RunAndWaitForClient (ulong clientId)
    {
        yield return new WaitUntil(() => tiles.Count != 0);

        SetMaterialOfRival();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestClearPlatformListServerRpc()
    {
        tiles.Dispose();
        ClientCube.Dispose();
        //CubeMaterials.Dispose();
        WeatherCode.Dispose();
    }
    public IEnumerator LaunchDynamics()
    {
        yield return new WaitUntil(() => ServerManager.Launch.Value);
        if (PlayerPrefs.GetInt("Vfx") == 1) dynamics.ToList().ForEach(d => d.Value.AddSmokeVfx(AdjustColorAccordingToTile(d.Key), Smoke_Burst));
        dynamics.ToList().ForEach(d => d.Value.RepeatColor(surfacesMat, LocalTiles[d.Key]));
    }
    public MaterialProperties AdjustColorAccordingToTile(Vector2Int pos)
    {
        Color _bottomColor = LocalTiles[pos].GetColor("_ColorBottom");
        Color _topColor = LocalTiles[pos].GetColor("_ColorTop");

        MaterialProperties _properties = new MaterialProperties(_bottomColor, _topColor);

        return _properties;
    }
    private void AdjustWeatherStatus ()
    {
        if (PlayerPrefs.GetInt("Vfx") == 0) return;

        int _stage = ServerManager.Stage.Value;

        if (ServerManager.IsHost)
            WeatherCode.Value = UnityEngine.Random.Range(0, 3);

        var shape = WeatherSystem[WeatherCode.Value].shape;
        shape.enabled = true;

        Vector3 pos = Vector3.zero;
        if (WeatherCode.Value == 0 || WeatherCode.Value == 2)
        {
            shape.scale = new Vector3(_stage + 12f, _stage + 12f, 1f);
            pos = new Vector3((_stage + 12) / 2f, 2.5f, (_stage + 12) / 2f);
        }
        else if (WeatherCode.Value == 1)
        {
            shape.scale = new Vector3(_stage + 12f, 1f, _stage + 12f);
            pos = new Vector3((_stage + 12) / 2f, 1.8f, (_stage + 12) / 2f);
        }

        WeatherSystem[WeatherCode.Value].transform.position = pos;
        ParticleSystem weather = Instantiate(WeatherSystem[WeatherCode.Value], pos, Quaternion.Euler(0f, 0f, 0f), transform);
        weather.name = "Weather";
    }
    public void Replace (Vector2Int Pos, ref GameObject Face)
    {
    }
}