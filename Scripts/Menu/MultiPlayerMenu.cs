using System;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class MultiPlayerMenu : MonoBehaviour
{

    private MainMenu Menu;

    private RoomDiscovery discovery;
    private RoomBroadcaster broadcast;

    [SerializeField]
    private NetworkManager manager;
    
    [SerializeField]
    private UnityTransport transport;

    [SerializeField]
    private RectTransform roomList;

    [SerializeField]
    private Text roomName;

    [SerializeField]
    private Dropdown stageMenu;

    public GameObject roomPrefab;


    private void Start()
    {
        Menu = GetComponent<MainMenu>();
        manager.OnClientStarted += ClientStarted();   
    }
    private Action ClientStarted()
    {
        return () => {
            manager.SceneManager.OnLoad += OnSceneLoading; // While one of the client's scene loading , it triggers
            manager.SceneManager.OnLoadComplete += OnSceneLoaded; // While end of the loading scene whose one of the client's
        };
    }
    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (clientId != manager.LocalClientId) return; // Everyone trigger own scene loader

        Scene current = SceneManager.GetActiveScene(); // Get current scene

        manager.SceneManager.OnLoad -= OnSceneLoading; // Memory leak cleaning
        manager.OnClientStarted -= ClientStarted(); // Memory leak cleaning
        manager.SceneManager.OnLoadComplete -= OnSceneLoaded; // Memory leak cleaning

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName)); // Set new scene that loaded
        manager.SceneManager.UnloadScene(current); // Remove old scene
    }
    public void OnSceneLoading(ulong clientId, string sceneName, LoadSceneMode loadSceneMode, AsyncOperation asyncOperation)
    {

        if (clientId != manager.LocalClientId) return;

        asyncOperation.allowSceneActivation = false; // Is no to be activated the scene that loaded 

        StartCoroutine(Menu.Loader.LoadSceneMultiplayer(asyncOperation)); // Load target scene as async
    }
    public void backMenuFromMultiplayer()
    {
        Menu.PlaySFX();
        StartCoroutine(Menu.FadeCanvasGroup(1f, 1f, Menu.UIMenu[0], ""));
        StartCoroutine(Menu.FadeCanvasGroup(0, 1, Menu.UIMenu[4], ""));
        enabled = false;
    }
    public void CreateGame()
    {
        Menu.PlaySFX();
        broadcast = manager.gameObject.AddComponent<RoomBroadcaster>();
        StartCoroutine(Menu.SelectedIcon(Menu.Icon.localPosition, new Vector3(-600, 40f, 0)));
        StartCoroutine(Menu.FadeCanvasGroup(1, 1, Menu.UIMenu[5], ""));
        StartCoroutine(Menu.FadeCanvasGroup(0f, 0.75f, Menu.UIMenu[^1], ""));
    }
    public void JoinGame()
    {
        Menu.PlaySFX();
        roomList.GetChild(0).GetChild(0).GetComponent<Text>().text = "No available hosts";
        discovery = manager.gameObject.AddComponent<RoomDiscovery>();
        StartCoroutine(Menu.SelectedIcon(Menu.Icon.localPosition, new Vector3(-600, -199f, 0f)));
        StartCoroutine(Menu.FadeCanvasGroup(1f, 1f, Menu.UIMenu[6], ""));
        StartCoroutine(Menu.FadeCanvasGroup(0f, 1f, Menu.UIMenu[^1], ""));
    }
    public void getRoomNameInput()
    {
        broadcast.roomName = roomName.text;
    }
    public void getSelectedStage()
    {
        int selectedStage = stageMenu.value;
        broadcast.stage += selectedStage;
    }
    public void create()
    {
        Menu.PlaySFX();
        broadcast.StartBroadcast();
        transport.SetConnectionData(broadcast.localIP, 7777);
        manager.StartHost();
        manager.SceneManager.OnLoad += OnSceneLoading;
        manager.SceneManager.LoadScene("Multiplayer", LoadSceneMode.Additive);
        manager.SceneManager.OnLoadComplete += OnSceneLoaded;
    }
    public void BackMultiplayerFromCreate()
    {
        Menu.PlaySFX();
        Destroy(broadcast);
        StartCoroutine(Menu.FadeCanvasGroup(0f, 1f, Menu.UIMenu[5], ""));
        StartCoroutine(Menu.FadeCanvasGroup(1f, 1f, Menu.UIMenu.Last(), ""));
    }
    public void BackMultiplayerFromJoin()
    {
        Menu.PlaySFX();
        foreach (Transform obj in roomList)
            if (obj.name != "Information")
                Destroy(obj.gameObject);
        Destroy(discovery);
        roomList.GetChild(0).GetChild(0).GetComponent<Text>().text = "No available hosts";
        StartCoroutine(Menu.FadeCanvasGroup(0f, 1f, Menu.UIMenu[6], ""));
        StartCoroutine(Menu.FadeCanvasGroup(1f, 1f, Menu.UIMenu[^1], ""));
    }
    public void refresh()
    {
        Menu.PlaySFX();
        discovery.StopAllCoroutines();
        discovery.StartScanning(ref roomList, ref roomPrefab, ref manager);
    }
    public void EasyCheck()
    {
        if (Menu.Toggles[4].isOn)
        {
            broadcast.difficulty = Difficulty.Easy;
            Menu.Toggles[5].isOn = false;
            Menu.Toggles[6].isOn = false;
        }
    }
    public void NormalCheck()
    {
        if (Menu.Toggles[5].isOn)
        {
            broadcast.difficulty = Difficulty.Normal;
            Menu.Toggles[4].isOn = false;
            Menu.Toggles[6].isOn = false;
        }
    }
    public void HardCheck()
    {
        if (Menu.Toggles[6].isOn)
        {
            broadcast.difficulty = Difficulty.Hard;
            Menu.Toggles[5].isOn = false;
            Menu.Toggles[4].isOn = false;
        }
    }
    public void OnEnable()
    {
        manager = FindObjectsOfType<NetworkManager>().First();
        transport = manager.GetComponent<UnityTransport>();
    }
    private void OnDestroy()
    {
        manager.OnClientStarted -= ClientStarted();
    }
    private void OnDisable()
    {
        manager.OnClientStarted -= ClientStarted();
    }
}


