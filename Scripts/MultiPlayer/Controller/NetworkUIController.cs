using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{

    public NetworkCubeController cubeController;

    public RenderTexture rt;

    public static int currentIndex = 1;

    private int OriginalCameraCulling;

    [SerializeField]
    private Transform _rivalGPS;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private List<RawImage> rawImages = new List<RawImage>();

    [SerializeField]
    private float fadeDuration = 0.5f;

    [SerializeField]
    private List<TMP_Text> texts = new List<TMP_Text>();

    [SerializeField]
    private List<Button> buttons = new List<Button>();

    [SerializeField]
    private AspectController aspect;

    [SerializeField]
    private ServerManager manager;

    [SerializeField]
    private Camera mapCamera;

    [SerializeField]
    private Image _pauseMenu, Winning;

    [SerializeField]
    private float swipeThreshold = 70f;

    private float timeSinceLastUpdate = 0;

    private bool isRotating = false;

    public static float _volume = 0f;

    private Vector2 touchStart;
    private void Start()
    {
        initUserPrefs();
        buttons.Find(c => c.name == "CloseMap").gameObject.SetActive(false);
        StartCoroutine(initializeMapCamera());
        MapCamController(rawImages.Find(r => r.name == "GameMap"));
        if (manager.IsHost)
            StartCoroutine(WaitAndPlayer("Host"));
        else
            StartCoroutine(WaitAndPlayer("Client"));
    }
    private void Update()
    {
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (manager._rival != null)
            _rivalGPS.position = manager._rival.position;

        if (texts[0].enabled && timeSinceLastUpdate >= 1f)
        {
            texts[0].text = "FPS : " + ((int)(1f / Time.unscaledDeltaTime)).ToString();
            timeSinceLastUpdate = 0f;
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
            OpenMenu();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            aspect.index = aspect.index == 0 ? 3 : --aspect.index;
            aspect.leftSwipe();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            aspect.index = aspect.index == 3 ? 0 : ++aspect.index;
            aspect.rightSwipe();
        }

        if (aspect.target != null)
            aspect.pivotAspect();

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
                if (Mathf.Abs(deltaX) > swipeThreshold)
                {
                    isRotating = true;
                    if (deltaX > 0)
                    {
                        aspectController.index = aspectController.index == 3 ? 0 : ++aspectController.index;
                        aspectController.rightSwipe();
                    }
                    else if (deltaX < 0)
                    {
                        aspectController.index = aspectController.index == 0 ? 3 : --aspectController.index;
                        aspectController.leftSwipe();
                    }
                }
                isRotating = false;
            }
            aspect.pivotAspect();
        }

#endif

    }
    public void OpenButton ()
    {
        StartCoroutine(OpenChest());
    }
    private void initUserPrefs ()
    {
        NetworkUIController._volume = PlayerPrefs.GetInt("Sfx");
        texts[0].enabled = (PlayerPrefs.GetInt("Fps") == 1);
        var _mainaudio = aspect.GetComponent<AudioSource>();
        _mainaudio.PlayOneShot(_mainaudio.clip, PlayerPrefs.GetInt("Music"));
    }
    private IEnumerator WaitAndPlayer (string playerType)
    {
        yield return new WaitUntil(() => GameObject.Find(playerType) != null);
        
        cubeController = GameObject.Find(playerType).GetComponent<NetworkCubeController>();

        aspect.target = GameObject.Find (playerType).GetComponent<Transform>();

        cubeController._gps.position = cubeController.transform.position;
    }
    private void ButtonManager (bool active)
    {
        buttons.ForEach(b => b.gameObject.SetActive(active));
    }
    public void Forward ()
    {
        if (!isRotating)
            cubeController.TryMove(aspect.dirs[0]);
    }
    public void Backward()
    {
        if (!isRotating)
            cubeController.TryMove(aspect.dirs[2]);
    }
    public void Left()
    {
        if (!isRotating)
            cubeController.TryMove(aspect.dirs[3]);
    }
    public void Right()
    {
        if (!isRotating)
            cubeController.TryMove(aspect.dirs[1]);
    }
    public void OpenMap ()
    {
        NetworkUIController.currentIndex = 0;
        MapCamController(rawImages.Find(r => r.name == "GameMap"));
        ButtonManager(false);
        buttons.Find(b => b.gameObject.name == "CloseMap").gameObject.SetActive(true);
        StartCoroutine(mapFade(new Color(1f ,1f ,1f ,1f)));
        cubeController._gps.GetChild(0).GetComponent<ParticleSystem>().Play();
        _rivalGPS.GetChild(0).GetComponent<ParticleSystem>().Play();
    }
    public void CloseMap()
    {
        NetworkUIController.currentIndex = 1;
        ButtonManager(true);
        buttons.Find(b => b.gameObject.name == "CloseMap").gameObject.SetActive(false);
        Camera.main.cullingMask = OriginalCameraCulling;
        StartCoroutine(mapFade(new Color(1f, 1f, 1f, 0f)));
        cubeController._gps.GetChild(0).GetComponent<ParticleSystem>().Stop();
        _rivalGPS.GetChild(0).GetComponent<ParticleSystem>().Stop();
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
    private IEnumerator initializeMapCamera()
    {
        Vector3 originPos = new Vector3(9, 0, 9);
        float ort = 5.36f;
        yield return new WaitUntil(() => manager._stage.Value != 0);
        float factor = (manager._stage.Value - 6) * 0.5f;
        mapCamera.GetComponent<Camera>().orthographicSize = ort + factor;
        mapCamera.transform.position = new Vector3(originPos.x + factor, 0, originPos.z + factor);
        if (manager.IsHost)
            target.position = new Vector3(manager._stage.Value + 6, 0.6f, manager._stage.Value + 6);
        else
            target.position = new Vector3(6f, 0.6f, 6f);

        _rivalGPS.position = new Vector3(target.position.x, 0.5f, target.position.z);

        yield return new WaitUntil(() => Camera.main != null);

        OriginalCameraCulling = Camera.main.cullingMask;
    }
    private IEnumerator mapFade(Color targetColor)
    {
        Color startColor = rawImages[0].color;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            rawImages[0].color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(time / fadeDuration));
            yield return null;
        }

        rawImages[0].color = targetColor;

        if (NetworkUIController.currentIndex == 1)
            MapCamController(rawImages.Find(r => r.name == "GameMap"));
    }
    public void OpenMenu ()
    {
        buttons.Last().gameObject.SetActive(false);
        StartCoroutine(PauseMenu(new Vector3(1f ,1f, 1f)));
    }
    public void CloseMenu()
    {
        buttons.Last().gameObject.SetActive(true);
        StartCoroutine(PauseMenu(new Vector3(0f, 0f, 1f)));
    }
    public void Menu()
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
    public void DistributeRewards ()
    {
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
        yield return new WaitForSeconds(2f);

        manager.GetInfo().rectTransform.localPosition = new Vector3(-299f, 161f, 0f); // Set the localPositions of information => -113f , 140f
        manager.GetInfo().text = "YOUR REWARDS WILL BE READY AT 2 SECONDS"; // Write it "TAP TO OPEN CHEST"

        Winning.gameObject.SetActive(true);

        Vector2 velocity = Vector2.zero;

        RectTransform startRect = Winning.rectTransform.GetChild(4).GetComponent<RectTransform>();

        Vector2 target = new Vector2(350f, 350f);

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

        StartCoroutine(GatherRewards());

    }
    private IEnumerator GatherRewards ()
    {

        manager.GetInfo().rectTransform.localPosition = new Vector3(-220, 161f, 0f); // Set the localPositions of information => -113f , 140f

        manager.GetInfo().text = "YOUR REWARDS HAS BEEN READY"; // Write it "TAP TO OPEN CHEST"

        Reward reward = manager.Rewards();

        Winning.rectTransform.GetChild(reward.FirstRewardIndex).gameObject.SetActive(true);

        Vector2 velocity = Vector2.zero;

        float angleVelocity = 0.0f, currentAngle = 0f , targetAngle = 359f;

        RectTransform startRect = Winning.rectTransform.GetChild(reward.FirstRewardIndex).GetComponent<RectTransform>();

        Winning.rectTransform.GetChild(reward.FirstRewardIndex).GetChild(0).GetComponent<TMP_Text>().text = "X" + reward.AmountOfFirst;

        startRect.localPosition = new Vector3(-250f, 0f, 0f);

        Vector2 target = new(200f, 300f);

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

        startRect.localPosition = new Vector3(250f, 0f, 0f);

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
}
