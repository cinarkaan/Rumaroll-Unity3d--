using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : NetworkBehaviour
{

    [SerializeField]
    private TMP_Text Info;

    public Transform _rival;
    
    public SceneLoader _sceneLoader;

    private readonly int[] events = { 3, 10, 3, 5 }; // 0 : Diamonds, 1: Coins, 2 : Shields, 3 : Clues. It shows events count.

    private bool EndGame = false;

    public NetworkManager Manager {  get; private set; }
    public NetworkVariable<int> _stage = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private List<ulong> clients;
    private ulong hostLocalId;
    
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
            StartCoroutine(_sceneLoader.RemoveWaiting(2, "Host"));
            _stage.Value = Manager.gameObject.GetComponent<RoomBroadcaster>().stage;
            Manager.OnClientConnectedCallback += OnClientConnected;
            Manager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            StartCoroutine(_sceneLoader.RemoveWaiting(2, "Client"));
            StartCoroutine(CheckHostConnection());
            _rival = GameObject.Find("Host").transform;
        }
    }
    private void OnClientConnected (ulong clientID)
    {
        clients = new List<ulong>(Manager.ConnectedClientsIds);
        hostLocalId = Manager.LocalClientId;

        int clientCounts = Manager.ConnectedClients.Count;
        if (clientCounts >= 2)
            _sceneLoader.loadingText.text = $"Waiting for players {clientCounts}/{2}";
        Manager.GetComponent<RoomBroadcaster>().StopBroadcast();
        _rival = GameObject.Find("Client").transform;
    }
    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count <= 1 && !EndGame)
        {
            Manager.Shutdown();
            Destroy(Manager.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void KickOutAllClients()
    {
        foreach (ulong client in clients)
        {
            if (client != hostLocalId)
                KickClientRpc(client);
        }
    }
    private IEnumerator CheckHostConnection ()
    { 
        while (true)
        {
            if (!Manager.IsConnectedClient && !EndGame)
            {
                Info.rectTransform.localPosition = new Vector3(-272f, 65f, 0f);
                Info.text = "THE HOST HAS BEEN DISCONNECTED!!!";
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
            Info.rectTransform.localPosition = new Vector3(-225f, 65f, 0f);
            Info.text = "CONGRATULATIONS , YOU WON !!!";
            EndGame = true;
            NotificateClientRpc("YOU LOST !!!", false ,new Vector3(-14, 65f, 0f));
        }
        else
        {
            Info.rectTransform.localPosition = new Vector3(-14, 65f, 0f);
            Info.text = "YOU LOST !!!";
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
        Info.rectTransform.localPosition = textPos;
        Info.text = clientMessage;
        if (!hasWon)
            StartCoroutine(DisconnectFromGame(2.5f));
    }
    public IEnumerator DisconnectFromGame (float second)
    {
        yield return new WaitForSeconds(second);
        Manager.Shutdown();
        Destroy (Manager.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
    public Reward Rewards ()
    {
        int firstRewardsIndex = Random.Range(0, 4); // Indicating whether it has the rewards at that index. 
        int secondRewardsIndex = Random.Range(0, 4); // Indicating whether it has the rewards at that index.

        return new Reward(firstRewardsIndex, secondRewardsIndex, events[firstRewardsIndex], events[secondRewardsIndex]);
    }
    public TMP_Text GetInfo ()
    {
        return this.Info;
    }

}

public struct Reward
{
    public int FirstRewardIndex { get; private set; }

    public int SecondRewardIndex { get; private set; }

    public int AmountOfFirst { get; private set; }

    public int AmountOfSecond { get; private set; }

    public Reward (int firstRewardIndex, int secondRewardIndex, int AmountOfFirst, int AmountOfSecond)
    {
        FirstRewardIndex = firstRewardIndex;
        SecondRewardIndex = secondRewardIndex;
        this.AmountOfFirst = AmountOfFirst;
        this.AmountOfSecond = AmountOfSecond;
    }
}
