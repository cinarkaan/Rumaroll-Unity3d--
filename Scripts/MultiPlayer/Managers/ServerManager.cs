using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : NetworkBehaviour
{

    [SerializeField] private NetworkUIController UIControler;
    public NetworkUIController UIController_ => UIControler;

    public Transform Rival;
    
    private bool EndGame = false;

    // General
    public NetworkVariable<int> Stage = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // The Stage which is width and height of the platform
    public NetworkVariable<int> Difficulty = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // The difficulty level
    public NetworkVariable<bool> Launch = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // If the game was stated , the update methods will be run.

    // Obstacle & Enemy
    public NetworkList<Vector2Int> _Spikes = new(new List<Vector2Int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Spikes position in the obstacle
    public NetworkList<Vector2Int> _Blades = new(new List<Vector2Int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // // Blades position in the obstacle
    public NetworkList<Vector3> _Path = new(new List<Vector3>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Paths in the enemy

    // Platform
    public NetworkList<CubeMaterials> HostMaterials = new(new List<CubeMaterials>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // The materials face of the cube 
    public NetworkList<CubeMaterials> ClientMaterials = new(new List<CubeMaterials>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // The client's cube faces of materials
    public NetworkList<Tiles> Tiles = new(new List<Tiles>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // The informations about the platform
    public NetworkVariable<int> WeatherCode = new(0); // Weather status

    public NetworkManager Manager { get; private set; }
    public bool Progress { get; private set; }

    private List<ulong> Clients;
    private ulong HostLocalId;
    private void Start()
    {
        Application.targetFrameRate = -1;
        
        var access = new GameObject("Access");

        DontDestroyOnLoad(access);

        Manager = access.scene.GetRootGameObjects().ToList().Find(h => h.name == "NetworkManager").GetComponent<NetworkManager>();

        Destroy(access.scene.GetRootGameObjects().ToList().Find(h => h.name == "Access"));
    }
    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitForScene());
    }
    public override void OnNetworkDespawn()
    {
        if (Manager.IsServer)
        {
            Manager.OnClientConnectedCallback -= OnClientConnected;
            Manager.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
    private IEnumerator WaitForScene ()
    {
        yield return new WaitForSeconds(1f);

        if (Manager.IsHost)
        {
            StartCoroutine(UIControler.SceneLoader.RemoveWaiting(2, "Host"));
            Stage.Value = Manager.gameObject.GetComponent<RoomBroadcaster>().stage;
            Difficulty.Value = Manager.gameObject.GetComponent<RoomBroadcaster>().difficulty.GetHashCode();
            Manager.OnClientConnectedCallback += OnClientConnected;
            Manager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            StartCoroutine(UIControler.SceneLoader.RemoveWaiting(2, "Client"));
            StartCoroutine(CheckHostConnection());
            Rival = GameObject.Find("Host").transform;
        }
        Progress = true;
    }
    private void OnClientConnected (ulong clientID)
    {
        Clients = new List<ulong>(Manager.ConnectedClientsIds);
        HostLocalId = Manager.LocalClientId;

        int clientCounts = Manager.ConnectedClients.Count;
        if (clientCounts >= 2)
            UIControler.SceneLoader.LoadingText.text = $"Waiting for players {clientCounts}/{2}";
        Manager.GetComponent<RoomBroadcaster>().StopBroadcast();
        Rival = GameObject.Find("Client").transform;
    }
    private void OnClientDisconnected(ulong clientId) // If the client exit from game , then the host will be kicked off itself from ther server.
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count <= 1 && !EndGame)
        {
            Stage.Dispose();
            Difficulty.Dispose();
            Manager.Shutdown();
            Destroy(Manager.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void KickOutAllClients()
    {
        foreach (ulong client in Clients)
        {
            if (client != HostLocalId)
                KickClientRpc(client);
        }
    }
    private IEnumerator CheckHostConnection () // Check out whether the host is exist on the server or not
    {
        Debug.Log("CheckHostConnection");
        while (true)
        {
            if (!Manager.IsConnectedClient && !EndGame)
            {
                UIControler.Info.rectTransform.localPosition = new Vector3(-233.5f, 65f, 0f);
                UIControler.Info.text = "THE HOST HAS BEEN DISCONNECTED!!!";
                yield return new WaitForSeconds(1.2f);
                Manager.Shutdown();
                Destroy(Manager.gameObject);
                SceneManager.LoadScene("MainMenu");
                break;
            }
            yield return new WaitForSeconds(3f);
        }

    }
    public IEnumerator DisconnectFromGame(float second)
    {
        yield return new WaitForSeconds(second);
        Manager.Shutdown();
        Destroy(Manager.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
    public void SetMaterials(ref GameObject Player, NetworkList<CubeMaterials> Materials, List<Object> AllMaterials)
    {
        Player.transform.GetChild(5).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[0]._surfaceMat.ToString()));
        Player.transform.GetChild(1).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[1]._surfaceMat.ToString()));
        Player.transform.GetChild(2).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[2]._surfaceMat.ToString()));
        Player.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[3]._surfaceMat.ToString()));
        Player.transform.GetChild(3).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[4]._surfaceMat.ToString()));
        Player.transform.GetChild(4).GetComponent<Renderer>().sharedMaterial = (Material)AllMaterials.Find(m => ((Material)m).name.Equals(Materials[5]._surfaceMat.ToString()));

    }

    [ServerRpc(RequireOwnership = false)]
    public void NoticationWonPlayerServerRpc (string name)
    {
        if (name.Equals("Host"))
        {
            EndGame = true;
            NotificateClientRpc("YOU LOST !!!", false ,new Vector3(-14, 65f, 0f));
        }
        else
        {
            AllowServerToContinueClientRpc();
            UIControler.Info.rectTransform.localPosition = new Vector3(-14, 65f, 0f);
            UIControler.Info.text = "YOU LOST !!!";
            StartCoroutine(DisconnectFromGame(2.5f));
        }
    }
    [ClientRpc]
    private void KickClientRpc(ulong cliendID)
    {
        if (Manager.LocalClientId == cliendID)
        {
            Manager.Shutdown();
            Destroy(Manager.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
    }
    [ClientRpc]
    private void NotificateClientRpc (string clientMessage,bool hasWon, Vector3 textPos)
    {
        if (Manager.IsHost) return;
        EndGame = true;
        UIControler.Info.rectTransform.localPosition = textPos;
        UIControler.Info.text = clientMessage;
        if (!hasWon)
            StartCoroutine(DisconnectFromGame(2.5f));
    }
    [ClientRpc]
    private void AllowServerToContinueClientRpc()
    {
        EndGame = true;
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestClearServerRpc()
    {
        UIControler.SceneLoader.Operation = 2; // Client is ready , remove waiting screen for host
        Launch.Value = true;
        Stage.Dispose();
        Difficulty.Dispose();
        _Spikes.Clear();
        _Blades.Clear();
        _Path.Clear();
    }
    [ServerRpc(RequireOwnership = false)]
    public void RequestClearPlatformListServerRpc()
    {
        Tiles.Clear();
        ClientMaterials.Clear();
        HostMaterials.Clear();
        WeatherCode.Dispose();
    }

}
