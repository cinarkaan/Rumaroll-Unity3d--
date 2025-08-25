using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : NetworkBehaviour
{

    [SerializeField]
    private NetworkUIController UIControler;
    public NetworkUIController _UIController => UIControler;

    public Transform Rival;
    
    private bool EndGame = false;

    // General
    public NetworkVariable<int> Stage = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> Difficulty = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> Launch = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Obstacle & Enemy
    public NetworkList<Vector2Int> _Spikes = new(new List<Vector2Int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<Vector2Int> _Blades = new(new List<Vector2Int>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<Vector3> _Path = new(new List<Vector3>(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Platform
    public NetworkList<CubeMaterials> CubeMaterials = new NetworkList<CubeMaterials>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<Tiles> tiles = new NetworkList<Tiles>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkList<ClientFaceIndicates> ClientCube = new NetworkList<ClientFaceIndicates>(null, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> WeatherCode = new NetworkVariable<int>(0);


    public NetworkManager Manager { get; private set; }
    
    public bool progress { get; private set; }

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
        progress = true;
    }
    private void OnClientConnected (ulong clientID)
    {
        Clients = new List<ulong>(Manager.ConnectedClientsIds);
        HostLocalId = Manager.LocalClientId;

        int clientCounts = Manager.ConnectedClients.Count;
        if (clientCounts >= 2)
            UIControler.SceneLoader.loadingText.text = $"Waiting for players {clientCounts}/{2}";
        Manager.GetComponent<RoomBroadcaster>().StopBroadcast();
        Rival = GameObject.Find("Client").transform;
    }
    private void OnClientDisconnected(ulong clientId)
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
    private IEnumerator CheckHostConnection ()
    { 
        while (true)
        {
            if (!Manager.IsConnectedClient && !EndGame)
            {
                UIControler.Info.rectTransform.localPosition = new Vector3(-272f, 65f, 0f);
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

    [ServerRpc(RequireOwnership = false)]
    public void NoticationWonPlayerServerRpc (string name)
    {
        if (name.Equals("Host"))
        {
            UIControler.Info.rectTransform.localPosition = new Vector3(-225f, 65f, 0f);
            UIControler.Info.text = "CONGRATULATIONS , YOU WON !!!";
            EndGame = true;
            NotificateClientRpc("YOU LOST !!!", false ,new Vector3(-14, 65f, 0f));
        }
        else
        {
            UIControler.Info.rectTransform.localPosition = new Vector3(-14, 65f, 0f);
            UIControler.Info.text = "YOU LOST !!!";
            NotificateClientRpc("CONGRATULATIONS , YOU WON !!!", true, new Vector3(-225f, 65f, 0f));
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

    [ServerRpc(RequireOwnership = false)]
    public void RequestClearServerRpc()
    {
        UIControler.SceneLoader.operation = 2; // Client is ready , remove waiting screen for host
        Launch.Value = true;
        //Stage.Dispose();
        Difficulty.Dispose();
        _Spikes.Clear();
        _Blades.Clear();
        _Path.Clear();

    }
    public IEnumerator DisconnectFromGame (float second)
    {
        yield return new WaitForSeconds(second);
        Manager.Shutdown();
        Destroy (Manager.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

}
