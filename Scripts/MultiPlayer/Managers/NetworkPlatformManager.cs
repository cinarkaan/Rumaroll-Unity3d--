using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct HostMaterials : INetworkSerializable, IEquatable<HostMaterials>
{
    public FixedString64Bytes _surfaceMat;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _surfaceMat);
    }
    public bool Equals(HostMaterials other)
    {
        return _surfaceMat.Equals(other._surfaceMat);
    }
    public override bool Equals(object obj)
    {
        return obj is HostMaterials other && Equals(other);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(_surfaceMat);
    }
}
[Serializable]
public struct Tiles : INetworkSerializable, IEquatable<Tiles>
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
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(positon, material, onSolution, _markAsDynamic);
    }
}
[Serializable]
public struct ClientMaterials : INetworkSerializable, IEquatable<ClientMaterials>
{
    public FixedString64Bytes _material;

    public bool Equals(ClientMaterials other)
    {
        throw new NotImplementedException();
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _material);
    }
}

public class NetworkPlatformManager : ExceptionalPlatform
{
    [SerializeField]
    private ServerManager ServerManager;
    public ServerManager ServerManager_ => ServerManager;
    public int Stage => _Stage;
    
    private void Start()
    {
        StartCoroutine(WaitUntilServer());
    }
    private void LateUpdate()
    {
        if (Progress && NetworkUIController.currentIndex == 1)
        {
            ParallelFrustumCulling();
            FrustumCullingForColorfuls();
            AllActivated = false;
        }
        else
        {
            if (Progress)
            {
                Graphics.DrawMeshInstanced(TileMesh, 0, TileMat, Tile);
                Graphics.DrawMeshInstanced(FrameMesh, 0, TileMat, Frame);
                Graphics.DrawMeshInstanced(SurfacesMesh, 0, TileMat, Surface);
                for (int i = 0; i < Renderers.Count && !AllActivated; i++)
                    if (!Renderers[i].enabled) Renderers[i].enabled = true;
                AllActivated = true;
            } 
        }
    }
    private IEnumerator WaitUntilServer()
    {
        yield return new WaitUntil(() => ServerManager != null && ServerManager.Stage.Value != 0);

        _Stage = ServerManager.Stage.Value;

        InitializeWeather(0);

        if (ServerManager.Manager.IsHost)
        {
            Prefabs[2] = GameObject.Find("Host");
            Prefabs[2].GetComponent<NetworkCubeController>().target = new Vector2Int(_Stage + 6, _Stage + 6);
        }
        else
        {
            yield return new WaitUntil(() => GameObject.Find("Client") != null);
            Prefabs[2] = GameObject.Find("Client");
        }

        RandomMaterialSelection();

        if (ServerManager.Manager.IsHost)
        {
            InitializeSolution();
            CreateDynamics();
        }

        CreateGrid();

        ServerManager.Manager.OnClientConnectedCallback += SetMaterialForEach;
    }
    protected override void RandomMaterialSelection ()
    {
        AllMaterials = Resources.LoadAll("Lit/GradientLit", typeof(Material)).ToList();

        if (ServerManager.Manager.IsHost)
        {
            HashSet<UnityEngine.Object> selected = new();

            while (selected.Count < 6)
            {
                bool isAdded = selected.Add(AllMaterials[UnityEngine.Random.Range(0, AllMaterials.Count)]);
                if (isAdded)
                {
                    HostMaterials materials = new()
                    {
                        _surfaceMat = ((Material)selected.Last()).name
                    };
                    ServerManager.HostMaterials.Add(materials);
                }
            }

            var SelectedArr = selected.ToArray();
            selected.Clear();

            Prefabs[2].transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[0];
            Prefabs[2].transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[1];
            Prefabs[2].transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[2];
            Prefabs[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[3];
            Prefabs[2].transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[4];
            Prefabs[2].transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)SelectedArr[5];

        } else
        {
            for (int index = 0; index < 6; index++)
            {
                if (index == 0)
                    Prefabs[2].transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 4)
                    Prefabs[2].transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 3)
                    Prefabs[2].transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 5)
                    Prefabs[2].transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else
                    Prefabs[2].transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                
            }
        }
        LinearToGamma(ref Prefabs[2]);

    }
    protected override void InitializeSolution()
    {
        Vector2Int goal = new(6 + _Stage, 6 + _Stage); // Evacuation point
        SolutionPath = GenerateSolutionPath(new Vector2Int(6, 6), goal); // Build valid solution

        CubeSimulator cubeSim = new(); // Cube simulator to stimulation the materials that is placed suitable
        Material temp; // Tempotary material to avoid GC allocation 
        
        Tiles tile = new() // Copy of the grid tiles that is placed on the local
        {
            positon = SolutionPath[0],
            material = ServerManager.HostMaterials[0]._surfaceMat,
            onSolution = true
        };
        
        temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[0]._surfaceMat.ToString())); // assign material as temp
        
        GridTiles.Add(SolutionPath[0], new PlatformTile(temp,true)); // Place on the local grid

        ServerManager.Tiles.Add(tile); // Add server tiles
        
        for (int i = 1; i < SolutionPath.Count; i++) // Build the suitable materials each tile
        {
            Vector2Int prev = SolutionPath[i - 1]; // previous solution
            Vector2Int current = SolutionPath[i]; // current solution
            Vector3 moveDir = new(current.x - prev.x, 0, current.y - prev.y); // calculate dir

            cubeSim.Roll(moveDir);// stimulate cube rolling
            int bottomFace = cubeSim.faceIndices[0]; // get new bottom face index of cube

            tile.positon = SolutionPath[i]; // assign solution pos to network tile
            tile.material = ServerManager.HostMaterials[bottomFace]._surfaceMat; // assign solution mat to network tile
            tile.onSolution = true; // // assign whether is on the solution or not
            ServerManager.Tiles.Add(tile); // Adding on the server tiles

            temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[bottomFace]._surfaceMat.ToString())); // assign temp mat 

            GridTiles.Add(SolutionPath[i], new PlatformTile(temp , true)); // Adding on the local tiles

            if (i == SolutionPath.Count - 1)
            {
                for (int index = 0; index < 6; index++)
                {
                    ClientMaterials clientFaceMaterials = new() 
                    {
                        _material = ServerManager.HostMaterials[cubeSim.faceIndices[index]]._surfaceMat
                    };
                    ServerManager.ClientMaterials.Add(clientFaceMaterials);
                }
            }
        }
    }
    public override void CreateDynamics()
    {
        UniqueRandomGenerator uniqueRandomGenerator = new (); // Zero index is the origin point that's why we can not started it from this as well as tiles.count is shown evacuation point with exceed.
        switch (_Stage)
        {
            case 10:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 4); // Get pos of dynamics tile's for the stage 10
                break;
            case 11:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 7); // Get pos of dynamics tile's for the stage 11
                break;
            case 12:
                uniqueRandomGenerator = new UniqueRandomGenerator(1, ServerManager.Tiles.Count - 1, 10); // // Get pos of dynamics tile's for the stage 12
                break;
            default:
                break;
        }
        foreach (var item in uniqueRandomGenerator.UniqueRandoms) // Init the dynamics both server and local side by using random positions
        {
            DynamicPath.Add(GridTiles.ElementAt(item).Key); // Add pos of tile 
            Tiles tile = ServerManager.Tiles[item]; 
            tile._markAsDynamic = true;
            ServerManager.Tiles[item] = tile;
        }            
    }
    protected override void CreateGrid() 
    {
        MaterialPropertyBlock MaterialPropertyBlock = new ();
        Material temp; 
        Tiles tile = new ();

        InitializeEnvironment();

        if (!ServerManager.IsHost && ServerManager.IsClient)
            FromNetworkToLocal();

        for (int x = 0; x < _Stage + 12; x++)
            for (int z = 0; z < _Stage + 12; z++)
            {
                if (x < 6 || z < 6 || x > 6 + _Stage || z > 6 + _Stage) // Build surround of the platform
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, -0.1f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0].transform.GetChild(1).localScale));
                    Surface.Add(Matrix4x4.TRS(new Vector3(x, -0.4f, z), Quaternion.Euler(new Vector3(-90f, Prefabs[0].transform.GetChild(0).localRotation.y, Prefabs[0].transform.GetChild(0).localRotation.z)), Prefabs[0].transform.GetChild(0).localScale));
                }
                else // Build colorfultiles of the platform
                {
                    Tile.Add(Matrix4x4.TRS(new Vector3(x, 0.28f, z), Prefabs[0].transform.localRotation, new Vector3(1f, 0.4f, 1f)));
                    Frame.Add(Matrix4x4.TRS(new Vector3(x, -0.0105f, z), Prefabs[0].transform.GetChild(1).localRotation, Prefabs[0]   .transform.GetChild(1).localScale));
                    GameObject colorfulTile = Instantiate(Prefabs[0].transform.GetChild(0).gameObject, new Vector3(x, -0.0105f, z), Quaternion.Euler(-90f, 0f, 0f), transform);
                    Renderers.Add(colorfulTile.GetComponent<Renderer>()); // We will use it frustum culling 
                    Vector2Int position = new(x, z); // Temp vector2int to avoid GC allocation
                    if (GridTiles.ContainsKey(position)) // On Solution tile
                    {
                        temp = GridTiles[position].material;
                        MaterialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom").gamma);
                        MaterialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop").gamma);
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
                        GridTiles[position].tile = colorfulTile;
                        if (DynamicPath.Contains(position)) // If it dynamic , we will add component that is managed dynamic tiles
                            GridTiles[position].tile.AddComponent<ColorfulTile>(); 
                    } else // UnSolution Tiles
                    {
                        if (ServerManager.IsHost)
                        {
                            string _matName = ServerManager.HostMaterials[UnityEngine.Random.Range(0, ServerManager.HostMaterials.Count)]._surfaceMat.ToString();
                            temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(_matName));
                        }
                        else
                            temp = GridTiles[position].material;
                        MaterialPropertyBlock.SetColor("_ColorBottom", temp.GetColor("_ColorBottom").gamma);
                        MaterialPropertyBlock.SetColor("_ColorTop", temp.GetColor("_ColorTop").gamma);
                        colorfulTile.GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
                        GridTiles[position] = new PlatformTile(colorfulTile.GetComponent<Renderer>().sharedMaterial, false);
                        UnSolution.Add(position);
                        if (ServerManager.IsHost)
                        {
                            tile.positon = position;
                            tile.material = temp.name;
                            tile.onSolution = false;
                            tile._markAsDynamic = false;
                            ServerManager.Tiles.Add(tile);
                        }
                        GridTiles[position].material = temp;
                    }
                    GridTiles[position].tile = colorfulTile;
                }
            }
        if (!ServerManager.Manager.IsHost && ServerManager.Manager.IsClient) // Set materials for the client who has been connected
            SetMaterialOfRival();
        PlaceFlag(); // Place flags   
    }
    protected override void InitializeWeather(int status)
    {
        if (PlayerPrefs.GetInt("Vfx") == 0) return;

        int _stage = _Stage;

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
    private void SetMaterialOfRival ()
    {
        if (ServerManager.IsHost)
        {
            GameObject client = GameObject.Find("Client");
            for (int index = 0; index < 6; index++)
            {
                if (index == 0)
                    client.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 4)
                    client.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 3)
                    client.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else if (index == 5)
                    client.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
                else
                    client.transform.GetChild(index).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.ClientMaterials[index]._material.ToString()));
            }
            LinearToGamma(ref client);
        } else
        {
            GameObject host = GameObject.Find("Host");
            host.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[0]._surfaceMat.ToString()));
            host.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[1]._surfaceMat.ToString()));
            host.transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[2]._surfaceMat.ToString()));
            host.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[3]._surfaceMat.ToString()));
            host.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[4]._surfaceMat.ToString()));
            host.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(ServerManager.HostMaterials[5]._surfaceMat.ToString()));
            LinearToGamma(ref host);
        }
    }
    private void SetMaterialForEach (ulong clientID)
    {
        StartCoroutine(RunAndWaitForClient());
    }
    public void FromNetworkToLocal()
    {
        Material temp;

        foreach (Tiles tile in ServerManager.Tiles)
        {
            temp = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(tile.material.ToString()));
            GridTiles.Add(tile.positon, new PlatformTile(temp, tile.onSolution));
            if (tile._markAsDynamic)
                DynamicPath.Add(tile.positon);
        }
    }
    private IEnumerator RunAndWaitForClient ()
    {
        yield return new WaitUntil(() => ServerManager.ClientMaterials.Count == 6);

        SetMaterialOfRival();
    }
    public IEnumerator LaunchDynamics()
    {
        yield return new WaitUntil(() => ServerManager.Launch.Value);
        
        foreach(Vector2Int pos in DynamicPath)
        {
            if (PlayerPrefs.GetInt("Vfx") == 1)
                GridTiles[pos].tile.GetComponent<ColorfulTile>().AddSmokeVfx(AdjustColorAccordingToTile(pos), Smoke_Burst);

            GridTiles[pos].tile.GetComponent<ColorfulTile>().RepeatColor(SurfacesMat, GridTiles[pos].material);
        }
    }
    public void LinearToGamma(ref GameObject Player)
    {
        MaterialPropertyBlock materialPropertyBlock = new();
        for (int i = 0; i < 6; i++)
        {
            Material material = Player.transform.GetChild(i).GetComponent<Renderer>().sharedMaterial;
            materialPropertyBlock.SetColor("_ColorBottom", material.GetColor("_ColorBottom").gamma);
            materialPropertyBlock.SetColor("_ColorTop", material.GetColor("_ColorTop").gamma);
            Player.transform.GetChild(i).GetComponent<Renderer>().SetPropertyBlock(materialPropertyBlock);
        }
    }
    private void OnDestroy()
    {
        ServerManager.Manager.OnClientConnectedCallback -= SetMaterialForEach;    
    }
}