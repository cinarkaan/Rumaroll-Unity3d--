using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIController : MonoBehaviour
{
    
    [Header("Screen control")]
    [SerializeField]
    private float fadeDuration = 1f;
    [SerializeField]
    private int counterDurationMin = 10;
    [SerializeField]
    private float swipeThreshold = 70f;
    [SerializeField]
    private float fadeDurationSpiwe = 0.5f;

    [Header("UI objects")]
    [SerializeField]
    private List<Button> buttons = new List<Button>();
    [SerializeField]
    private List<Image> images = new List<Image>();
    
    [SerializeField]
    private RawImage[] rawImages;
    public RawImage RawImage => rawImages[0];
    

    [SerializeField]
    private List<Text> texts = new List<Text>();
    [SerializeField]
    private List<GameObject> objects = new List<GameObject>();
    [SerializeField]
    private AspectController aspectController;
    [SerializeField]
    private SceneLoader loader;
    [SerializeField]
    private GameMapController gameMapController;

    [SerializeField]
    private AudioClip[] audioClips;
    [SerializeField]
    private AudioSource audioSource;

    private AudioSource _mainSource;

    public float _volume { get; private set; }


    [Header("Durations of shield's back counter")]
    [SerializeField]
    private float _shieldDuration = 60f;

    [Header("Requirements")]
    [SerializeField]
    private PlatformManager platformManager;
    public RollingCubeController playerController;

    private Vector2 touchStart;

    private float timeSinceLastUpdate = 0;

    private bool isRotating = false, isCounter;

    void Start()
    {        
        _mainSource = aspectController.transform.GetComponent<AudioSource>();
        _mainSource.PlayOneShot(audioClips.Last(), PlayerPrefs.GetInt("Music"));
        StartCoroutine(placeFlag());
        initializeEvents();
        initializeButtons();
        initializeUserPrefs();
        StartCoroutine(fadeInOut(Color.black, Color.clear, 1f));
    }
    public void initializeEvents ()
    {
        texts.Find(t => t.name == "CoinCount").text = ": X" + PlayerPrefs.GetInt("Coin").ToString();
        if (texts.Find(t => t.name == "DiamondCount") != null)
            texts.Find(t => t.name == "DiamondCount").text = ": X" + PlayerPrefs.GetInt("Diamond").ToString();
        if (texts.Find(t => t.name == "ShieldCount") != null)
            texts.Find(t => t.name == "ShieldCount").text = ": X" + PlayerPrefs.GetInt("Shield").ToString();
        texts.Find(t => t.name == "ClueCount").text = ": X" + PlayerPrefs.GetInt("Clue").ToString();
        texts[0].gameObject.SetActive(PlayerPrefs.GetInt("Fps") > 0 ? true : false);
        swipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
    }
    public void initializeButtons()
    {
        buttons.Find(b => b.name == "Close").gameObject.SetActive(false);
        if (buttons.Find(b => b.name == "Shield") != null)
            buttons.Find(b => b.name == "Shield").interactable = PlayerPrefs.GetInt("Shield") > 0;
        buttons.Find(b => b.name == "Clue").interactable = PlayerPrefs.GetInt("Clue") > 0;
    }
    public void initializeUserPrefs ()
    {
        texts[0].gameObject.SetActive(PlayerPrefs.GetInt("Fps") == 1);
        swipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
        _volume = PlayerPrefs.GetInt("Sfx");
    }
    private void LateUpdate()
    {
        timeSinceLastUpdate += Time.unscaledDeltaTime;

        if (texts[0].enabled && timeSinceLastUpdate >= 1f)
        {
            texts[0].text = "FPS : " + ((int)(1f / Time.unscaledDeltaTime)).ToString();
            timeSinceLastUpdate = 0f;
        }

#if UNITY_STANDALONE_WIN
        
        if (Input.GetKeyDown(KeyCode.A))
            OnMoveLeft();
        else if (Input.GetKeyDown(KeyCode.D))
            OnMoveRight();
        else if (Input.GetKeyDown(KeyCode.W))
            OnMoveForward();
        else if (Input.GetKeyDown(KeyCode.S))
            OnMoveBackward();
        
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (gameMapController.currentIndex == 0)
                closeMap();
            else
                openMap();
        }

        if (Input.GetKeyDown(KeyCode.C) && buttons.Find(b => b.name == "Clue").interactable)
            clue();
        if (SceneManager.GetActiveScene().buildIndex > 1 && Input.GetKeyDown(KeyCode.R) && buttons.Find(b => b.name == "Shield").interactable)
                shield();
        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            aspectController.index = aspectController.index == 0 ? 3 : --aspectController.index;
            aspectController.leftSwipe();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            aspectController.index = aspectController.index == 3 ? 0 : ++aspectController.index;
            aspectController.rightSwipe();
        }

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
        }
#endif
        aspectController.pivotAspect();
    }
    public IEnumerator placeFlag ()
    {
        yield return new WaitUntil(() => platformManager.progress);
        objects.Last().transform.position = new Vector3(platformManager.stage + 6, 0.5f, platformManager.stage + 6);
    }
    private IEnumerator shieldCounter ()
    {
        images.Last().gameObject.SetActive(true);

        buttons.Find(b => b.name == "Shield").interactable = false;

        playerController.shield.Play();

        Vector3 originalSize = texts[5].rectTransform.localScale;
        Color originalColor = texts[5].color;
        while (_shieldDuration > 0f)
        {
            StartCoroutine(ExtendAndFadeAnimation());
            yield return new WaitUntil(() => !isCounter);
        }

        playerController.GetComponent<OverlapBoxNonAllocPoller>().shieldIsActive = false;

        _shieldDuration = 30f;

        texts[5].text = "";

        images.Last().gameObject.SetActive(false);

        if (PlayerPrefs.GetInt("Shield") == 0)
            buttons.Find(b => b.name == "Shield").interactable = false;
        else
            buttons.Find(b => b.name == "Shield").interactable = true;

        StartCoroutine(playerController.ShieldController(false));

        playerController.shield.Stop();
    }
    public void buttonsManager(bool interactable)
    {
        buttons.ForEach(b => b.gameObject.SetActive(interactable));
    }
    public void eventsManager (bool interactable)
    {
        int index = 2;
        while (index < images.Count)
        {
            if (index == images.Count - 1 && playerController.GetComponent<OverlapBoxNonAllocPoller>().shieldIsActive)
                break;
            images[index++].gameObject.SetActive(interactable);
        }

        if (!playerController.GetComponent<OverlapBoxNonAllocPoller>().shieldIsActive && images.Last().name == "IsShieldActive")
            images.Last().gameObject.SetActive(false);
    }
    public void gameOver (int SoundIndex)
    {
        if (SoundIndex == 2)
            audioSource.PlayOneShot(audioClips[2], _volume);
        else if (SoundIndex == 3)
            audioSource.PlayOneShot(audioClips.Last(), _volume);

        StartCoroutine(scalerMenu(Vector3.zero, Vector3.one, 1f, false, images.Find(f => f.name == "GameOverMenu")));
        playerController.Render(false);
        buttons.ForEach(b => b.gameObject.SetActive(false));
    }
    public void Pause ()
    {
        StartCoroutine(scalerMenu(Vector3.zero, Vector3.one, 1f, true,images.Find(f => f.name == "PauseMenu")));
        buttons.ForEach(b => b.interactable = false);
    }
    public void Resume ()
    {
        buttons.ForEach(b => b.interactable = true);
        Time.timeScale = 1f;
        StartCoroutine(scalerMenu(Vector3.one, Vector3.zero, 1f, false, images.Find(f => f.name == "PauseMenu")));
        images.Find(f => f.name == "PauseMenu").gameObject.SetActive(true);
    }
    public void Restart ()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void menu ()
    {
        Time.timeScale = 1f;
        StartCoroutine(sceneLoader(0f, 1f, 0.5f, "MainMenu"));
    } 
    public void OnMoveForward() { if (isRotating) return; playerController.TryMove(aspectController.dirs[0]); }
    public void OnMoveRight() { if (isRotating) return; playerController.TryMove(aspectController.dirs[1]); }
    public void OnMoveBackward() { if (isRotating) return; playerController.TryMove(aspectController.dirs[2]); }
    public void OnMoveLeft() { if (isRotating) return; playerController.TryMove(aspectController.dirs[3]); }
    public void openMap ()
    {
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        gameMapController.currentIndex = 0;
        gameMapController.renderController(rawImages[0]);
        eventsManager(false);
        buttonsManager(false);
        buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(true);
        StartCoroutine(mapFade(Color.white));
    }
    public void closeMap ()
    {
        Camera.main.cullingMask = gameMapController.OriginalCulllingIndex;
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        gameMapController.currentIndex = 1;
        eventsManager(true);
        buttonsManager(true);
        buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(false);
        StartCoroutine(mapFade(new Color(1,1,1,0)));
    }
    public void shield () // Close render on the mapcamera 
    {

        audioSource.PlayOneShot(audioClips[1], _volume);
        int currentAmount = PlayerPrefs.GetInt("Shield");
        PlayerPrefs.SetInt("Shield", currentAmount - 1);
        texts.Find(t => t.name == "ShieldCount").text = ": X" + PlayerPrefs.GetInt("Shield").ToString();
        playerController.GetComponent<OverlapBoxNonAllocPoller>().shieldIsActive = true;
        StartCoroutine(playerController.ShieldController(true));
        StartCoroutine(shieldCounter());
    }
    public void clue ()
    {
        audioSource.PlayOneShot(audioClips[0], _volume);
        int currentAmount = PlayerPrefs.GetInt("Clue");
        PlayerPrefs.SetInt("Clue", currentAmount - 1);
        texts.Find(t => t.name == "ClueCount").text = ": X" + PlayerPrefs.GetInt("Clue").ToString();
        Vector2Int current = playerController.GetTileCoordAtPosition();
        int currentIndex = platformManager.solutionPath.IndexOf(current);
        Vector2Int cluePos = platformManager.solutionPath[currentIndex + 1];
        platformManager._clue.transform.localPosition = new Vector3(cluePos.x, 2.5f, cluePos.y);
        platformManager.AdjustColorOfClue(cluePos);
        platformManager._clue.Play();
        if (PlayerPrefs.GetInt("Clue") == 0)
            buttons.Find(b => b.name == "Clue").interactable = false;
        else
            buttons.Find(b => b.name == "Clue").interactable = true;
    }
    public void UpdateGps (Vector3 player)
    {
        objects[0].transform.position = player;
    }
    public void updateCoinCount (int count)
    {
        texts.Find(t => t.name == "CoinCount").text = " : X" + count.ToString();
    }
    public void updateDiamondCount (int count)
    {
        texts.Find(t => t.name == "DiamondCount").text = " : X" + count.ToString();
    }
    public IEnumerator sceneLoader(float startAlpha, float targetAlpha, float duration, string sceneName)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            loader.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        loader.GetComponent<Image>().raycastTarget = true;

        loader.GetComponent<CanvasGroup>().alpha = targetAlpha;
        PlayerPrefs.SetInt("Stage", (platformManager.stage != 12 && !sceneName.Equals("MainMenu")) ? platformManager.stage + 1 : platformManager.stage);
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(loader.LoadSceneWithPreparation(sceneName));
    }
    public IEnumerator scalerMenu (Vector3 from , Vector3 to, float time,bool menu,Image image)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            image.GetComponent<RectTransform>().localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        image.GetComponent<RectTransform>().localScale = to;
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
        
        if (gameMapController.currentIndex == 1)
            gameMapController.renderController(rawImages[0]);

    }
    public IEnumerator fadeInOut(Color startColor, Color end, float duration)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            images[1].GetComponent<Image>().color = Color.Lerp(startColor, end, time / fadeDuration);
            yield return null;
        }

        images[1].GetComponent<Image>().color = end;
    }
    public IEnumerator ExtendAndFadeAnimation ()
    {
        isCounter = true;
        
        Vector3 originalSize = texts[5].rectTransform.localScale;
        Color originalColor = texts[5].color;

        float timer = 0f;

        while(timer < 1f)
        {
            timer += Time.deltaTime;
            float shieldSize = Mathf.PingPong(Time.time * 38f, 20) + 55;
            images.Last().rectTransform.sizeDelta = new Vector2(shieldSize, shieldSize);
            if (_shieldDuration < 10f)
            {
                texts[5].rectTransform.localScale = Vector3.Lerp(originalSize, new Vector3(1.3f, 1.3f, 1f), Mathf.Clamp01(timer / 1f));
                texts[5].color = Color.Lerp(originalColor, new Color(1f, 1f, 1f, 0f), Mathf.Clamp01(timer / 1f));
            }
            yield return null;
        }

        --_shieldDuration;

        texts[5].text = ((int)_shieldDuration).ToString() + " Seconds";

        texts[5].rectTransform.localScale = originalSize;
        texts[5].color = originalColor;

        isCounter = false;
    }
}
