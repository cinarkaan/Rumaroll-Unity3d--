using System;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum Difficulty
{
    Easy,
    Normal,
    Hard
}
public class MultiPlayerMenu : ExceptionalMenu
{
    [SerializeField] private NetworkManager manager;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private RectTransform roomList;
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private TMP_Dropdown stageMenu;

    private RoomDiscovery Discovery;
    private RoomBroadcaster Broadcast;
    private MainMenu MainMenu;

    public GameObject roomPrefab;

    protected override void Start()
    {
        manager = FindObjectsOfType<NetworkManager>().First();
        transport = manager.GetComponent<UnityTransport>();
        TMPTool = new(Events.Take(2).Cast<TextMeshProUGUI>().ToArray(), 0, 0);
        MainMenu = GetComponent<MainMenu>();
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

        MainMenu.TMPTool_.SetHeader(SceneLoader.Header);

        StartCoroutine(SceneLoader.LoadSceneMultiplayer(asyncOperation)); // Load target scene as async
    }
    public void BackMenuFromMultiplayer()
    {
        PlaySFX();
        Vector3 start = SelectiveIcon.localPosition;
        Vector3 end = new(-600f, -45f, 0f);
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[0], ""));
        StartCoroutine(FadeCanvasGroup(0, 1, UIMenu[1], ""));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
        StartCoroutine(MoveIcon(start, end));
        MainMenu.TMPTool_.SetHeader(Events[2]);
        enabled = false;
    }
    public void CreateGame()
    {
        PlaySFX();
        Vector3 TargetPosition = new(-600, 120f, 0);
        Broadcast = manager.gameObject.AddComponent<RoomBroadcaster>();
        StartCoroutine(MoveIcon(SelectiveIcon.localPosition,TargetPosition));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
        StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[2], ""));
        StartCoroutine(FadeCanvasGroup(0f, 0.75f, UIMenu[^1], ""));
    }
    public void JoinGame()
    {
        PlaySFX();
        roomList.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "No available hosts";
        Discovery = manager.gameObject.AddComponent<RoomDiscovery>();
        Vector3 TargetPosition = new(-600, -125f, 0f);
        StartCoroutine(MoveIcon(SelectiveIcon.localPosition, TargetPosition));
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[3], ""));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[^1], ""));
    }
    public void GetRoomNameInput()
    {
        Broadcast.roomName = roomName.text;
    }
    public void GetSelectedStage()
    {
        int selectedStage = stageMenu.value;
        Broadcast.stage += selectedStage;
    }
    public void Create()
    {
        PlaySFX();
        Broadcast.StartBroadcast();
        transport.SetConnectionData(Broadcast.localIP, 7777);
        manager.StartHost();
        manager.SceneManager.OnLoad += OnSceneLoading;
        manager.SceneManager.LoadScene("Multiplayer", LoadSceneMode.Additive);
        manager.SceneManager.OnLoadComplete += OnSceneLoaded;
    }
    public void BackMultiplayerFromCreate()
    {
        PlaySFX();
        Destroy(Broadcast);
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[2], ""));
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[^1], ""));
    }
    public void BackMultiplayerFromJoin()
    {
        PlaySFX();
        foreach (Transform obj in roomList)
            if (obj.name != "Information")
                Destroy(obj.gameObject);
        Destroy(Discovery);
        roomList.GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = "No available hosts";
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[3], ""));
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[^1], ""));
    }
    public void Refresh()
    {
        PlaySFX();
        Discovery.StopAllCoroutines();
        Discovery.StartScanning(ref roomList, ref roomPrefab, ref manager);
    }
    public void EasyCheck()
    {
        if (Toggle[0].isOn)
        {
            Broadcast.difficulty = Difficulty.Easy;
            Toggle[1].isOn = false;
            Toggle[2].isOn = false;
        }
    }
    public void NormalCheck()
    {
        if (Toggle[1].isOn)
        {
            Broadcast.difficulty = Difficulty.Normal;
            Toggle[0].isOn = false;
            Toggle[2].isOn = false;
        }
    }
    public void HardCheck()
    {
        if (Toggle[2].isOn)
        {
            Broadcast.difficulty = Difficulty.Hard;
            Toggle[0].isOn = false;
            Toggle[1].isOn = false;
        }
    }
    public void OnEnable()
    {
        manager.OnClientStarted += ClientStarted();
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


