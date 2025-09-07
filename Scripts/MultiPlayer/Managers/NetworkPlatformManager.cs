using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

public class NetworkPlatformManager : ExceptionalPlatform
{

    [SerializeField]
    private ServerManager ServerManager;

    public ServerManager _ServerManager => ServerManager;


    private GameObject Player;

    private Dictionary<Vector2Int, Material> LocalTiles = new Dictionary<Vector2Int, Material>();

    private Dictionary<Vector2Int,ColorfulTile> dynamics = new Dictionary<Vector2Int,ColorfulTile>();


    private MaterialPropertyBlock MaterialPropertyBlock;

    private void Start()
    {
        MaterialPropertyBlock = new MaterialPropertyBlock();
        StartCoroutine(WaitUntilServer());
    }
    private void LateUpdate()
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
                Graphics.DrawMeshInstanced(TileMesh, 0, TileMat, Tile);
                Graphics.DrawMeshInstanced(FrameMesh, 0, TileMat, Frame);
                Graphics.DrawMeshInstanced(SurfacesMesh, 0, TileMat, Surface);
                foreach (var tile in Renderers)
                {
                    if (!tile.enabled)
                        tile.enabled = true;
                }
            } 
        }
    }
    private IEnumerator WaitUntilServer()
    {
        yield return new WaitUntil(() => ServerManager != null && ServerManager.Stage.Value != 0);

        _Stage = ServerManager.Stage.Value;

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
    protected override void RandomMaterialSelection ()
    {
        AllMaterials = Resources.LoadAll("Gradient", typeof(Material)).ToList();

        if (ServerManager.Manager.IsHost)
        {
            HashSet<UnityEngine.Object> selected = new HashSet<UnityEngine.Object>();

            while (selected.Count < 6)
            {
                bool isAdded = selected.Add(AllMaterials[UnityEngine.Random.Range(0, AllMaterials.Count)]);
                if (isAdded)
                {
                    CubeMaterials materials = new CubeMaterials
                    {
                        _surfaceMat = ((Material)selected.Last()).name
                    };
                    ServerManager.CubeMaterials.Add(materials);
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
                    Player.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 4)
                    Player.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 3)
                    Player.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 5)
                    Player.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else
                    Player.transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                
            }
        }

    }
    protected override void CreateSolutionPath()
    {
        Vector2Int goal = new Vector2Int(6 + ServerManager.Stage.Value, 6 + ServerManager.Stage.Value);
        SolutionPath = GenerateSolutionPath(new Vector2Int(6, 6), goal);

        CubeSimulator cubeSim = new CubeSimulator();

        Tiles tile = new()
        {
            positon = SolutionPath[0],
            material = ServerManager.CubeMaterials[0]._surfaceMat,
            onSolution = true
        };
        ServerManager.Tiles.Add(tile);
        
        for (int i = 1; i < SolutionPath.Count; i++)
        {
            Vector2Int prev = SolutionPath[i - 1];
            Vector2Int current = SolutionPath[i];
            Vector3 moveDir = new Vector3(current.x - prev.x, 0, current.y - prev.y);

            cubeSim.Roll(moveDir);
            int bottomFace = cubeSim.faceIndices[0];
            Tiles t = new()
            {
                positon = SolutionPath[i],
                material = ServerManager.CubeMaterials[bottomFace]._surfaceMat,
                onSolution = true
            };
            ServerManager.Tiles.Add(t);
            if (i == SolutionPath.Count - 1)
            {
                for (int index = 0; index < 6; index++)
                {
                    ClientFaceIndicates clientFaceMaterials = new()
                    {
                        _faceIndicates = cubeSim.faceIndices[index],
                        _material = ServerManager.CubeMaterials[cubeSim.faceIndices[index]]._surfaceMat
                    };
                    ServerManager.ClientCube.Add(clientFaceMaterials);
                }

            }
        }
    }
    public override void CreateDynamics()
    {
        // Zero index is the origin point that's why we can not started it from this as well as tiles.count is shown evacuation point with exceed.
        UniqueRandomGenerator uniqueRandomGenerator = new ();
        switch (ServerManager.Stage.Value)
        {
            case 10:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 4);
                break;
            case 11:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 7);
                break;
            case 12:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 10);
                break;
            default:
                break;
        }
        foreach (var item in uniqueRandomGenerator.UniqueRandoms)
        {
            Tiles tile = ServerManager.Tiles[item];
            tile._markAsDynamic = true;
            ServerManager.Tiles[item] = tile;
        }            
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
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    Surface.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, Prefabs[0].transform.GetChild(0).localRotation.y, Prefabs[0].transform.GetChild(0).localRotation.z)), Prefabs[0].transform.GetChild(0).localScale));
                }
                else
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, 0.29f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.01f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0]   .transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(Prefabs[0].transform.GetChild(0).gameObject, new Vector3(x, -0.01f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    Renderers.Add(colorfulTile.GetComponent<Renderer>());
                    var _colorfulTile = HasAtTile(new Vector2Int(x, z), ref colorfulTile);
                    if (_colorfulTile != null)
                    {
                        temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(_colorfulTile.Value.material.ToString()));
                        MaterialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom"));
                        MaterialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop"));
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
                    }
                    else
                    {
                        Tiles t = new()
                        {
                            positon = new Vector2Int(x, z),
                            material = ServerManager.CubeMaterials[UnityEngine.Random.Range(0, ServerManager.CubeMaterials.Count)]._surfaceMat,
                            onSolution = false
                        };
                        temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(t.material.ToString()));
                        MaterialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom"));
                        MaterialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop"));
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
                        ServerManager.Tiles.Add(t);
                        UnSolution.Add(new Vector2Int(x, z));
                    }
                    LocalTiles[new Vector2Int(x, z)] = temp;
                }
            }
        PlaceFlag();
    }
    private void InitializePlatformAsClient()
    {
        InitializeEnvironment();

        for (int x = 0; x < ServerManager.Stage.Value + 12; x++)
            for (int z = 0; z < ServerManager.Stage.Value + 12; z++)
            {
                if (x < 6 || z < 6 || x > 6 + ServerManager.Stage.Value || z > 6 + ServerManager.Stage.Value)
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    Surface.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, Prefabs[0].transform.GetChild(0).localRotation.y, Prefabs[0].transform.GetChild(0).localRotation.z)), Prefabs[0].transform.GetChild(0).localScale));
                }
                else
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, 0.29f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.01f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(Prefabs[0].transform.GetChild(0).gameObject, new Vector3(x, -0.01f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    Renderers.Add(colorfulTile.GetComponent<Renderer>());
                    Material _tileMat = GetTileMat(new Vector2Int(x, z), ref colorfulTile);
                    MaterialPropertyBlock.SetColor("_ColorBottom", _tileMat.GetColor("_ColorBottom"));
                    MaterialPropertyBlock.SetColor("_ColorTop", _tileMat.GetColor("_ColorTop"));
                    colorfulTile.GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
                    LocalTiles[new Vector2Int(x, z)] = _tileMat;
                }
            }
        SetMaterialOfRival();
        PlaceFlag();
    }
    public Tiles? HasAtTile (Vector2Int pos,ref GameObject colorfulTile)
    {
        foreach (Tiles t in ServerManager.Tiles)
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
        foreach (Tiles t in ServerManager.Tiles)
        {
            if (t.positon.Equals(pos))
            {
                if (t.onSolution && t._markAsDynamic)
                {
                    dynamics.Add(pos, _colorfulTile.AddComponent<ColorfulTile>());
                    SolutionPath.Add(pos);
                }
                else
                    UnSolution.Add(pos);
                    return (Material)AllMaterials.Find(m => ((Material)m).name.Equals(t.material.ToString()));
            }
        }
        return null;
    }
    public override Material GetTileMat (Vector2Int pos)
    {
        return LocalTiles[pos];
    }
    public override void SetTileMat(Material mat, Vector2Int pos)
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
                    client.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 4)
                    client.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 3)
                    client.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else if (index == 5)
                    client.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
                else
                    client.transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientCube[index]._material.ToString()));
            }
        } else
        {
            GameObject host = GameObject.Find("Host");
            host.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[0]._surfaceMat.ToString()));
            host.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[1]._surfaceMat.ToString()));
            host.transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[2]._surfaceMat.ToString()));
            host.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[3]._surfaceMat.ToString()));
            host.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[4]._surfaceMat.ToString()));
            host.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.CubeMaterials[5]._surfaceMat.ToString()));
        }
    }
    private void SetMaterialForEach (ulong clientID)
    {
        StartCoroutine(RunAndWaitForClient());
    }
    private IEnumerator RunAndWaitForClient ()
    {
        yield return new WaitUntil(() => ServerManager.Tiles.Count != 0);

        SetMaterialOfRival();
    }
    public IEnumerator LaunchDynamics()
    {
        yield return new WaitUntil(() => ServerManager.Launch.Value);
        if (PlayerPrefs.GetInt("Vfx") == 1) dynamics.ToList().ForEach(d => d.Value.AddSmokeVfx(AdjustColorAccordingToTile(d.Key), Smoke_Burst));
        dynamics.ToList().ForEach(d => d.Value.RepeatColor(SurfacesMat, LocalTiles[d.Key]));
    }
    public override MaterialProperties AdjustColorAccordingToTile(Vector2Int pos)
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
            ServerManager.WeatherCode.Value = UnityEngine.Random.Range(0, 3);

        var shape = Weather[ServerManager.WeatherCode.Value].shape;
        shape.enabled = true;

        Vector3 pos = Vector3.zero;
        if (ServerManager.WeatherCode.Value == 0 || ServerManager.WeatherCode.Value == 2)
        {
            shape.scale = new Vector3(_stage + 12f, _stage + 12f, 1f);
            pos = new Vector3((_stage + 12) / 2f, 2.5f, (_stage + 12) / 2f);
        }
        else if (ServerManager.WeatherCode.Value == 1)
        {
            shape.scale = new Vector3(_stage + 12f, 1f, _stage + 12f);
            pos = new Vector3((_stage + 12) / 2f, 1.8f, (_stage + 12) / 2f);
        }

        Weather[ServerManager.WeatherCode.Value].transform.position = pos;
        ParticleSystem weather = Instantiate(Weather[ServerManager.WeatherCode.Value], pos, Quaternion.Euler(0f, 0f, 0f), transform);
        weather.name = "Weather";
    }

}