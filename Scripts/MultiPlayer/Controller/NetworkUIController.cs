using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUIController : ExceptionalUI
{
    public NetworkCubeController cubeController;

    public RenderTexture rt;
    
    public static int currentIndex = 1;

    [SerializeField] private SceneLoader _sceneLoader;
    public SceneLoader SceneLoader => _sceneLoader;

    public Text Info => texts[1];

    [SerializeField] private Transform _rivalGPS;  

    [SerializeField] private Transform target;

    [SerializeField] private ServerManager manager;

    [SerializeField] private Camera mapCamera;

    [SerializeField] private Image _pauseMenu, Winning, Fade;

    [SerializeField] private ParticleSystem Confetie;

    private Vector2 touchStart;

    private int OriginalCameraCulling;

    private readonly int[] events = { 3, 10, 3, 5 }; // 0 : Diamonds, 1: Coins, 2 : Shields, 3 : Clues. It shows events count.
    private void Start()
    {
        InitializeUserPrefs();
        InitializeScoreTimes();
        Buttons.Find(c => c.name == "CloseMap").gameObject.SetActive(false);
        StartCoroutine(InitializeMapCamera());
        MapCamController(RawImage);
        if (manager.IsHost)
            StartCoroutine(WaitAndPlayer("Host"));
        else
            StartCoroutine(WaitAndPlayer("Client"));
    }
    private void LateUpdate()
    {
        TimeSinceLastUpdate += Time.unscaledDeltaTime;

        if (manager.Rival != null)
            _rivalGPS.position = manager.Rival.position;

        if (texts[0].enabled && TimeSinceLastUpdate >= 1f)
        {
            texts[0].text = "FPS : " + ((int)(1f / Time.unscaledDeltaTime));
            TimeSinceLastUpdate = 0f;
        }

#if UNITY_STANDALONE_WIN

        if (Input.GetKeyDown(KeyCode.A))
            Left();
        else if (Input.GetKeyDown(KeyCode.D))
            Right();
        else if (Input.GetKeyDown(KeyCode.W))
            Forward();
        else if (Input.GetKeyDown(KeyCode.S))
            Backward();

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (currentIndex == 0)
                CloseMap();
            else
                OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Aspect.index = Aspect.index == 0 ? 3 : --Aspect.index;
            Aspect.LeftSwipe();
            Platform.MainLight_.transform.eulerAngles = Aspect.GetAngleAccordingToPlayerAspect();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Aspect.index = Aspect.index == 3 ? 0 : ++Aspect.index;
            Aspect.RightSwipe();
            Platform.MainLight_.transform.eulerAngles = Aspect.GetAngleAccordingToPlayerAspect();
        }

        if (Aspect.target != null)
            Aspect.PivotAspect();
#else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float deltaX = touch.position.x - touchStart.x;
                if (Mathf.Abs(deltaX) > SwipeThreshold)
                {
                    IsRotating = true;
                    if (deltaX > 0)
                    {
                        Aspect.index = Aspect.index == 3 ? 0 : ++Aspect.index;
                        Aspect.rightSwipe();
                    }
                    else if (deltaX < 0)
                    {
                        Aspect.index = Aspect.index == 0 ? 3 : --Aspect.index;
                        Aspect.leftSwipe();
                    }
                }
                IsRotating = false;
            }
        }
        if (Aspect.target != null)
            Aspect.pivotAspect();
#endif

    }
    protected override void InitializeUserPrefs()
    {
        texts[0].enabled = PlayerPrefs.GetInt("Fps") == 1;
        SwipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
        NetworkUIController._Volume = PlayerPrefs.GetInt("Sfx");
    }
    private IEnumerator WaitAndPlayer (string playerType)
    {
        yield return new WaitUntil(() => GameObject.Find(playerType) != null);
        
        cubeController = GameObject.Find(playerType).GetComponent<NetworkCubeController>();

        Aspect.target = GameObject.Find(playerType).GetComponent<Transform>();

        cubeController._gps.position = cubeController.transform.position;
    }
    public override void Forward ()
    {
        if (!IsRotating)
            cubeController.TryMove(Aspect.Dirs[0]);
    }
    public override void GameOver(int SoundIndex, string name)
    {
        if (name != cubeController.name) return;
        if (SoundIndex == 2)
            AudioSource.PlayOneShot(AudioClips[0], _Volume);
        else if (SoundIndex == 3)
            AudioSource.PlayOneShot(AudioClips[1], _Volume);
        StartCoroutine(ScalerMenu(Vector3.zero, Vector3.one, 0.5f, Images.Find(f => f.name == "GameOverMenu")));
        cubeController.Render(false);
        Buttons.ForEach(b => b.gameObject.SetActive(false));
    }
    public override void Backward()
    {
        if (!IsRotating)
            cubeController.TryMove(Aspect.Dirs[2]);
    }
    public override void Left()
    {
        if (!IsRotating)
            cubeController.TryMove(Aspect.Dirs[3]);
    }
    public override void Right()
    {
        if (!IsRotating)
            cubeController.TryMove(Aspect.Dirs[1]);
    }
    public override void OpenMap ()
    {
        NetworkUIController.currentIndex = 0;
        MapCamController(RawImage);
        ButtonsManager(false);
        Buttons.Find(b => b.gameObject.name == "CloseMap").gameObject.SetActive(true);
        StartCoroutine(MapFade(new Color(1f ,1f ,1f ,1f)));
        cubeController._gps.GetChild(0).GetComponent<ParticleSystem>().Play();
        _rivalGPS.GetChild(0).GetComponent<ParticleSystem>().Play();
    }
    public override void CloseMap()
    {
        NetworkUIController.currentIndex = 1;
        ButtonsManager(true);
        Buttons.Find(b => b.gameObject.name == "CloseMap").gameObject.SetActive(false);
        Camera.main.cullingMask = OriginalCameraCulling;
        StartCoroutine(MapFade(new Color(1f, 1f, 1f, 0f)));
        MapCamController(RawImage);
        cubeController._gps.GetChild(0).GetComponent<ParticleSystem>().Stop();
        _rivalGPS.GetChild(0).GetComponent<ParticleSystem>().Stop();
    }
    protected override IEnumerator MapFade(Color targetColor)
    {
        Color startColor = RawImage.color;
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            RawImage.color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(time / FadeDuration));
            yield return null;
        }

        RawImage.color = targetColor;

        if (NetworkUIController.currentIndex == 1)
            MapCamController(RawImage);
    }
    public void MapCamController(RawImage gameMap)
    {
        if (NetworkUIController.currentIndex == 0)
        {
            Camera.main.cullingMask = LayerMask.GetMask("UI");
            mapCamera.GetComponent<Camera>().enabled = true;
            mapCamera.GetComponent<Camera>().targetTexture = rt;
            mapCamera.GetComponent<Camera>().Render();
            gameMap.texture = rt;
        }
        else
        {
            mapCamera.GetComponent<Camera>().enabled = false;
            mapCamera.GetComponent<Camera>().targetTexture = null;
            gameMap.texture = null;
        }
    }
    private IEnumerator InitializeMapCamera()
    {
        Vector3 originPos = new(9, 0, 9);
        float ort = 5.36f;
        yield return new WaitUntil(() => manager.Stage.Value != 0);
        float factor = (manager.Stage.Value - 6) * 0.5f;
        mapCamera.GetComponent<Camera>().orthographicSize = ort + factor;
        mapCamera.transform.position = new Vector3(originPos.x + factor, 0, originPos.z + factor);
        if (manager.IsHost)
            target.position = new Vector3(manager.Stage.Value + 6, 0.6f, manager.Stage.Value + 6);
        else
            target.position = new Vector3(6f, 0.6f, 6f);

        _rivalGPS.position = new Vector3(target.position.x, 0.5f, target.position.z);

        yield return new WaitUntil(() => Camera.main != null);

        OriginalCameraCulling = Camera.main.cullingMask;
    }
    public override void Pause ()
    {
        Buttons.Last().gameObject.SetActive(false);
        StartCoroutine(PauseMenu(new Vector3(1f ,1f, 1f)));
    }
    public override void Continue()
    {
        Buttons.Last().gameObject.SetActive(true);
        StartCoroutine(PauseMenu(new Vector3(0f, 0f, 1f)));
    }
    public override void Menu()
    {
        if (manager.Manager.IsHost) // !!! WARNING !!! If the host has shutdown and exit to mainmenu , the client has been still staying on the scene... If the host kick out to the client , do not destroy will no be destroyed on the client.
        {
            if (manager.Manager.ConnectedClientsList.Count > 1)
                manager.KickOutAllClients();
            else
            {
                manager.Manager.Shutdown();
                Destroy(manager.Manager.gameObject);
                SceneManager.LoadScene("MainMenu");
            }
        }
        else  // No problem , first client will be disconected and then host , then go to main menu
        {
            manager.Manager.Shutdown();
            Destroy(manager.Manager.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
    }
    public override void Restart () 
    {
        StartCoroutine(FadeInOut(Color.clear, Color.black));
        if (manager.IsHost)
            cubeController.transform.position = new Vector3(6f, 0.815f, 6f);
        else
            cubeController.transform.position = new Vector3(manager.Stage.Value + 6f, 0.815f, manager.Stage.Value + 6f);

        cubeController.Render(true);
        Images.Find(f => f.name == "GameOverMenu").rectTransform.localScale = Vector3.zero;
        Buttons.ForEach(b => b.gameObject.SetActive(true));
        Buttons[4].gameObject.SetActive(false);
        cubeController.Origin();
        StartCoroutine(FadeInOut(Color.black, Color.clear));
    }
    public void DistributeRewards ()
    {
        Images.Last().transform.localScale = Vector3.zero;
        StartCoroutine(OpenChest());
    }
    private IEnumerator PauseMenu (Vector3 target)
    {
        Vector3 start = _pauseMenu.transform.localScale;
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            _pauseMenu.transform.localScale = Vector3.Lerp(start, target, Mathf.Clamp01(time / 0.5f));
            yield return null;
        }
        _pauseMenu.transform.localScale = target;
    }
    public IEnumerator OpenChest ()
    {
        Info.rectTransform.localPosition = new Vector3(-280f, 60f, 0f); // Set the localPositions of information => -113f , 140f
        Info.text = "YOUR REWARDS WILL BE READY AT 2 SECONDS"; // Write it "TAP TO OPEN CHEST"
        Info.fontSize = 30;
        
        yield return new WaitForSeconds(2f);

        Info.text = "";

        Winning.gameObject.SetActive(true);

        Vector2 velocity = Vector2.zero;

        RectTransform startRect = Winning.rectTransform.GetChild(4).GetComponent<RectTransform>();

        Vector2 target = new(350f, 350f);

        while (Vector2.Distance(startRect.sizeDelta, target) > 0.01f)
        {
            startRect.sizeDelta = Vector2.SmoothDamp(
                startRect.sizeDelta,
                target,
                ref velocity,
                0.3f
            );

            yield return null;
        }

        Winning.rectTransform.GetChild(4).gameObject.SetActive(false);

        Winning.rectTransform.GetChild(5).gameObject.SetActive(true);

        texts.First().gameObject.SetActive(false);

        StartCoroutine(GatherRewards());
    }
    private IEnumerator GatherRewards ()
    {
        Info.rectTransform.localPosition = new Vector3(-185f, 170f, 0f); // Set the localPositions of information => -113f , 140f
        Info.fontSize = 32;
        Info.text = "YOUR REWARDS HAS BEEN READY"; // Write it "TAP TO OPEN CHEST"

        Reward reward = Rewards();

        Winning.rectTransform.GetChild(reward.FirstRewardIndex).gameObject.SetActive(true);

        Vector2 velocity = Vector2.zero;

        float angleVelocity = 0.0f, currentAngle = 0f , targetAngle = 359f;

        RectTransform startRect = Winning.rectTransform.GetChild(reward.FirstRewardIndex).GetComponent<RectTransform>();

        Winning.rectTransform.GetChild(reward.FirstRewardIndex).GetChild(0).GetComponent<TMP_Text>().text = "X" + reward.AmountOfFirst;

        startRect.localPosition = new Vector3(-260f, 0f, 0f);

        Vector2 target = new(300f, 300f);

        while (Vector2.Distance(startRect.sizeDelta, target) > 0.01f)
        {
            startRect.sizeDelta = Vector2.SmoothDamp(
                startRect.sizeDelta,
                target,
                ref velocity,
                0.45f
            );

            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, 0.45f); 

            startRect.localEulerAngles = new Vector3(0f, 0f, currentAngle * 360f);

            yield return null;
        }

        yield return new WaitForSeconds(2);

        Winning.rectTransform.GetChild(reward.SecondRewardIndex).gameObject.SetActive(true);

        velocity = Vector2.zero;

        angleVelocity = 0.0f;

        currentAngle = 0f;

        startRect = Winning.rectTransform.GetChild(reward.SecondRewardIndex).GetComponent<RectTransform>();

        Winning.rectTransform.GetChild(reward.SecondRewardIndex).GetChild(0).GetComponent<TMP_Text>().text = "X" + reward.AmountOfSecond;

        startRect.localPosition = new Vector3(260f, 0f, 0f);

        while (Vector2.Distance(startRect.sizeDelta, target) > 0.01f)
        {
            startRect.sizeDelta = Vector2.SmoothDamp(
                startRect.sizeDelta,
                target,
                ref velocity,
                0.45f
            );

            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, 0.45f);

            startRect.localEulerAngles = new Vector3(0f, 0f, currentAngle * 360f);

            yield return null;
        }

        StartCoroutine(manager.DisconnectFromGame(2.5f));
    }
    public Reward Rewards()
    {
        int firstRewardsIndex = Random.Range(0, 4); // Indicating whether it has the rewards at that index. 
        int secondRewardsIndex = Random.Range(0, 4); // Indicating whether it has the rewards at that index.

        return new Reward(firstRewardsIndex, secondRewardsIndex, events[firstRewardsIndex], events[secondRewardsIndex]);
    }
    public IEnumerator FadeInOut (Color from , Color to)
    {
        float time = 0f;

        while (time < 0.75)
        {
            time += Time.deltaTime;
            Fade.GetComponent<Image>().color = Color.Lerp(from, to, time / 0.75f);
            yield return null;
        }
    }
}

public struct Reward
{
    public int FirstRewardIndex { get; private set; }

    public int SecondRewardIndex { get; private set; }

    public int AmountOfFirst { get; private set; }

    public int AmountOfSecond { get; private set; }

    public Reward(int firstRewardIndex, int secondRewardIndex, int AmountOfFirst, int AmountOfSecond)
    {
        FirstRewardIndex = firstRewardIndex;
        SecondRewardIndex = secondRewardIndex;
        this.AmountOfFirst = AmountOfFirst;
        this.AmountOfSecond = AmountOfSecond;
    }
}