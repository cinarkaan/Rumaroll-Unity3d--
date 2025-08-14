using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIController : ExceptionalUI
{
    
    [Header("Screen control")]
    [SerializeField]
    private int counterDurationMin = 10;
    [SerializeField]
    private float fadeDurationSpiwe = 0.5f;

    [SerializeField]
    private List<GameObject> objects = new List<GameObject>();

    [SerializeField]
    private List<Text> texts = new List<Text>();

    [SerializeField]
    private SceneLoader loader;
    [SerializeField]
    private GameMapController gameMapController;  

    [Header("Durations of shield's back counter")]
    [SerializeField]
    private float _shieldDuration = 60f;

    [Header("Requirements")]
    [SerializeField]
    private PlatformManager platformManager;

    public RollingCubeController playerController;

    private Vector2 touchStart;

    private bool isCounter;

    void Start()
    {        
        StartCoroutine(PlaceFlag());
        InitializeEvents();
        InitializeButtons();
        InitializeUserPrefs();
        StartCoroutine(FadeInOut(Color.black, Color.clear, 1f));
    }
    public void InitializeEvents ()
    {
        texts.Find(t => t.name == "CoinCount").text = ": X" + PlayerPrefs.GetInt("Coin").ToString();
        if (texts.Find(t => t.name == "DiamondCount") != null)
            texts.Find(t => t.name == "DiamondCount").text = ": X" + PlayerPrefs.GetInt("Diamond").ToString();
        if (texts.Find(t => t.name == "ShieldCount") != null)
            texts.Find(t => t.name == "ShieldCount").text = ": X" + PlayerPrefs.GetInt("Shield").ToString();
        if (texts.Find(t => t.name == "ClueCount") != null)
            texts.Find(t => t.name == "ClueCount").text = ": X" + PlayerPrefs.GetInt("Clue").ToString();
        texts[0].gameObject.SetActive(PlayerPrefs.GetInt("Fps") > 0 ? true : false);
        SwipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
    }
    public void InitializeButtons()
    {
        buttons.Find(b => b.name == "Close").gameObject.SetActive(false);
        if (buttons.Find(b => b != null &&  b.name == "Shield") is Button shield)
         shield.interactable = PlayerPrefs.GetInt("Shield") > 0;
        if (buttons.Find(b => b != null &&  b.name == "Clue") is Button clue)
            clue.interactable = PlayerPrefs.GetInt("Clue") > 0;
    }
    protected override void InitializeUserPrefs ()
    {
        texts[0].enabled = (PlayerPrefs.GetInt("Fps") == 1);
        SwipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
        UIController._volume = PlayerPrefs.GetInt("Sfx");
    }
    private void LateUpdate()
    {
        TimeSinceLastUpdate += Time.unscaledDeltaTime;

        if (texts[0].enabled && TimeSinceLastUpdate >= 1f)
        {
            texts[0].text = "FPS : " + ((int)(1f / Time.unscaledDeltaTime)).ToString();
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
            if (gameMapController.currentIndex == 0)
                CloseMap();
            else
                OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.C) && buttons.Find(b => b.name == "Clue").interactable)
            Clue();
        if (SceneManager.GetActiveScene().buildIndex > 1 && Input.GetKeyDown(KeyCode.R) && buttons.Find(b => b.name == "Shield").interactable)
                Shield();
        if (Input.GetKeyDown(KeyCode.Escape))
            Pause();

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Aspect.index = Aspect.index == 0 ? 3 : --Aspect.index;
            Aspect.leftSwipe();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Aspect.index = Aspect.index == 3 ? 0 : ++Aspect.index;
            Aspect.rightSwipe();
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
        Aspect.pivotAspect();
    }
    public IEnumerator PlaceFlag ()
    {
        yield return new WaitUntil(() => platformManager.progress);
        objects.Last().transform.position = new Vector3(platformManager.stage + 6, 0.5f, platformManager.stage + 6);
    }
    private IEnumerator ShieldCounter ()
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
    public void EventsManager (bool interactable)
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
    public override void GameOver(int SoundIndex, string name)
    {
        if (SoundIndex == 2)
            audioSource.PlayOneShot(audioClips[2], _volume);
        else if (SoundIndex == 3)
            audioSource.PlayOneShot(audioClips.Last(), _volume);

        StartCoroutine(scalerMenu(Vector3.zero, Vector3.one, 1f, images.Find(f => f.name == "GameOverMenu")));
        playerController.Render(false);
        buttons.ForEach(b => b.gameObject.SetActive(false));
    }
    public override void Pause ()
    {
        StartCoroutine(scalerMenu(Vector3.zero, Vector3.one, 1f,images.Find(f => f.name == "PauseMenu")));
        buttons.ForEach(b => b.interactable = false);
    }
    public override void Continue()
    {
        buttons.ForEach(b => b.interactable = true);
        Time.timeScale = 1f;
        StartCoroutine(scalerMenu(Vector3.one, Vector3.zero, 1f, images.Find(f => f.name == "PauseMenu")));
        images.Find(f => f.name == "PauseMenu").gameObject.SetActive(true);
    }
    public override void Restart ()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public override void Menu ()
    {
        Time.timeScale = 1f;
        StartCoroutine(SceneLoader(0f, 1f, 0.5f, "MainMenu"));
    } 
    public override void Forward() { if (IsRotating) return; playerController.TryMove(Aspect.dirs[0]); }
    public override void Right() { if (IsRotating) return; playerController.TryMove(Aspect.dirs[1]); }
    public override void Backward() { if (IsRotating) return; playerController.TryMove(Aspect.dirs[2]); }
    public override void Left() { if (IsRotating) return; playerController.TryMove(Aspect.dirs[3]); }
    public override void OpenMap ()
    {
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        gameMapController.currentIndex = 0;
        gameMapController.renderController(rawImages[0]);
        EventsManager(false);
        ButtonsManager(false);
        buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(true);
        StartCoroutine(MapFade(Color.white, null));
    }
    public override void CloseMap ()
    {
        Camera.main.cullingMask = gameMapController.OriginalCulllingIndex;
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        gameMapController.currentIndex = 1;
        EventsManager(true);
        ButtonsManager(true);
        buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(false);
        StartCoroutine(MapFade(new Color(1,1,1,0), () =>
        {
            //if (gameMapController.currentIndex == 1)
                gameMapController.renderController(rawImages[0]);
        }));
    }
    public void Shield () 
    {

        audioSource.PlayOneShot(audioClips[1], _volume);
        int currentAmount = PlayerPrefs.GetInt("Shield");
        PlayerPrefs.SetInt("Shield", currentAmount - 1);
        texts.Find(t => t.name == "ShieldCount").text = ": X" + PlayerPrefs.GetInt("Shield").ToString();
        playerController.GetComponent<OverlapBoxNonAllocPoller>().shieldIsActive = true;
        StartCoroutine(playerController.ShieldController(true));
        StartCoroutine(ShieldCounter());
    }
    public void Clue ()
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
    public void UpdateCoinCount (int count)
    {
        texts.Find(t => t.name == "CoinCount").text = " : X" + count.ToString();
    }
    public void UpdateDiamondCount (int count)
    {
        texts.Find(t => t.name == "DiamondCount").text = " : X" + count.ToString();
    }
    public IEnumerator SceneLoader(float startAlpha, float targetAlpha, float duration, string sceneName)
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
    public IEnumerator FadeInOut(Color startColor, Color end, float duration)
    {
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            images[1].GetComponent<Image>().color = Color.Lerp(startColor, end, time / FadeDuration);
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
