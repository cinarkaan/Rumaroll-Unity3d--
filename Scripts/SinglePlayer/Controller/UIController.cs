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
    private List<GameObject> objects = new();

    [SerializeField]
    private SceneLoader loader;
    [SerializeField]
    private GameMapController gameMapController;  

    [Header("Durations of shield's back counter")]
    [SerializeField]
    private float _shieldDuration = 60f;

    public RollingCubeController playerController;

    private Vector2 touchStart;
    private bool isCounter;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(PlaceFlag());
        InitializeEvents();
        InitializeButtons();
        InitializeUserPrefs();
        InitializeScoreTimes();
        StartCoroutine(FadeInOut(Color.black, Color.clear));
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
        texts[0].gameObject.SetActive(PlayerPrefs.GetInt("Fps") > 0);
        SwipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
    }
    public void InitializeButtons()
    {
        if (Buttons.Find(b => b != null && b.name == "Shield") is Button shield)
            shield.interactable = PlayerPrefs.GetInt("Shield") > 0;
        if (Buttons.Find(b => b != null && b.name == "Clue") is Button clue)
            clue.interactable = PlayerPrefs.GetInt("Clue") > 0;
        
        var ImageComponents = Images.Last().GetComponentsInChildren<Image>();
        ImageComponents[4].alphaHitTestMinimumThreshold = 0.1f;
        ImageComponents[5].alphaHitTestMinimumThreshold = 0.1f;
    }
    private void LateUpdate()
    {
        PassedTime += Time.deltaTime;
        TimeSinceLastUpdate += Time.unscaledDeltaTime;

        if (texts[0].enabled && TimeSinceLastUpdate >= 0.45f)
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
            if (gameMapController.currentIndex == 0)
                CloseMap();
            else
                OpenMap();
        }

        if (Input.GetKeyDown(KeyCode.C) && Buttons.Find(b => b.name == "Clue").interactable)
            Clue();
        if (SceneManager.GetActiveScene().buildIndex > 1 && Input.GetKeyDown(KeyCode.R) && Buttons.Find(b => b.name == "Shield").interactable)
                Shield();
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
                        Aspect.RightSwipe();
                    }
                    else if (deltaX < 0)
                    {
                        Aspect.index = Aspect.index == 0 ? 3 : --Aspect.index;
                        Aspect.LeftSwipe();
                    }
                    Platform.MainLight_.transform.eulerAngles = Aspect.GetAngleAccordingToPlayerAspect();
                }
                IsRotating = false;
            }
        }
#endif
        Aspect.PivotAspect();
        TMPTool.WaveHeader();
    }
    protected override void InitializeUserPrefs()
    {
        texts[0].enabled = (PlayerPrefs.GetInt("Fps") == 1);
        SwipeThreshold = PlayerPrefs.GetFloat("Touch Sensitivity");
        UIController._Volume = PlayerPrefs.GetInt("Sfx");
    }
    public override void GameOver(int SoundIndex, string name)
    {
        IsClicked = false;
        if (SoundIndex == 2)
            AudioSource.PlayOneShot(AudioClips[2], _Volume);
        else if (SoundIndex == 3)
            AudioSource.PlayOneShot(AudioClips[3], _Volume);
        
        if (gameMapController.currentIndex == 0)
            CloseMap();

        Images.Find(f => f.name == "PauseMenu").transform.localScale = Vector3.zero;
        StartCoroutine(ScalerMenu(Images.Find(f => f.name == "GameOverMenu"), Vector3.one));
        playerController.Render(false);
        Buttons.ForEach(b => b.gameObject.SetActive(false));
    }
    public override void Pause ()
    {
        if (IsClicked) return;
        IsClicked = true;
        StartCoroutine(ScalerMenu(Images.Find(f => f.name == "PauseMenu"), Vector3.one));
        Buttons.ForEach(b => b.gameObject.SetActive(false));
    }
    public override void Continue()
    {
        if (IsClicked) return;
        IsClicked = true;
        Buttons.ForEach(b => b.gameObject.SetActive(true));
        Buttons.Find(close => close.name == "Close").gameObject.SetActive(false);
        StartCoroutine(ScalerMenu(Images.Find(f => f.name == "PauseMenu"), Vector3.zero));
    }
    public override void Restart ()
    {
        if (IsClicked) return;
        IsClicked = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public override void Menu ()
    {
        if (IsClicked) return;
        IsClicked = true;
        Images.Last().transform.localScale = Vector3.zero;
        StartCoroutine(SceneLoader(0f, 1f, 0.5f, "MainMenu"));
    } 
    public override void Forward() { if (IsRotating) return; playerController.TryMove(Aspect.Dirs[0]); }
    public override void Right() { if (IsRotating) return; playerController.TryMove(Aspect.Dirs[1]); }
    public override void Backward() { if (IsRotating) return; playerController.TryMove(Aspect.Dirs[2]); }
    public override void Left() { if (IsRotating) return; playerController.TryMove(Aspect.Dirs[3]); }
    public override void OpenMap ()
    {
        if (IsClicked) return;
        IsClicked = true;
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        gameMapController.currentIndex = 0;
        gameMapController.RenderController(RawImage);
        EventsManager(false);
        ButtonsManager(false);
        Buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(true);
        StartCoroutine(MapFade(Color.white));
    }
    public override void CloseMap ()
    {
        if (IsClicked) return;
        IsClicked = true;
        Camera.main.cullingMask = gameMapController.OriginalCulllingIndex;
        objects.First().transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        gameMapController.currentIndex = 1;
        EventsManager(true);
        ButtonsManager(true);
        Buttons.Find(b => b.gameObject.name == "Close").gameObject.SetActive(false);
        StartCoroutine(MapFade(new Color(1,1,1,0)));
    }
    public void Next ()
    {
        if (IsClicked) return;
        IsClicked = true;
        Images.Last().transform.localScale = Vector3.zero;
        StartCoroutine(SceneLoader(0f, 1f, 0.5f, "Day"));
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

        if (gameMapController.currentIndex == 1)
            gameMapController.RenderController(RawImage);

        IsClicked = false;
    }
    public void EventsManager(bool interactable)
    {
        int index = 2;
        Image Shield = null;
        while (index < Images.Count)
        {
            Images[index].gameObject.SetActive(interactable);
            if (Images[index].gameObject.name == "IsShieldActive")
                Shield = Images[index];
            index++;
        }
        
        if (!playerController.GetComponent<OverlapBoxNonAllocPoller>().ShieldIsActive && Shield != null)
            Images[10].gameObject.SetActive(false);
        
    }
    public void Shield () 
    {
        AudioSource.PlayOneShot(AudioClips[1], _Volume);
        int currentAmount = PlayerPrefs.GetInt("Shield");
        PlayerPrefs.SetInt("Shield", currentAmount - 1);
        texts.Find(t => t.name == "ShieldCount").text = ": X" + PlayerPrefs.GetInt("Shield").ToString();
        playerController.GetComponent<OverlapBoxNonAllocPoller>().ShieldIsActive = true;
        StartCoroutine(playerController.ShieldController(true));
        StartCoroutine(ShieldCounter());
    }
    public void Clue ()
    {
        AudioSource.PlayOneShot(AudioClips[0], _Volume);
        int currentAmount = PlayerPrefs.GetInt("Clue");
        PlayerPrefs.SetInt("Clue", currentAmount - 1);
        texts.Find(t => t.name == "ClueCount").text = ": X" + PlayerPrefs.GetInt("Clue").ToString();
        Vector2Int current = playerController.GetTileCoordAtPosition();
        int currentIndex = Platform.SolutionPath.IndexOf(current);
        Vector2Int cluePos = Platform.SolutionPath[currentIndex + 1];        
        ((PlatformManager)Platform).Clue_.transform.localPosition = new Vector3(cluePos.x, 2.5f, cluePos.y);
        ((PlatformManager)Platform).AdjustColorOfClue(cluePos);
        ((PlatformManager)Platform).Clue_.Play();
        if (PlayerPrefs.GetInt("Clue") == 0)
            Buttons.Find(b => b.name == "Clue").interactable = false;
        else
            Buttons.Find(b => b.name == "Clue").interactable = true;
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
        int Stage = ((PlatformManager)Platform).Stage;

        while (elapsed < duration) // Scene loader will be activated
        {
            elapsed += Time.deltaTime;
            loader.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        loader.GetComponent<Image>().raycastTarget = true; // Obstructing to user clicks 

        loader.GetComponent<CanvasGroup>().alpha = targetAlpha; // Assign new alpha value
        PlayerPrefs.SetInt("Stage", (Stage != 12 && !sceneName.Equals("MainMenu")) ? Stage + 1 : Stage); // Make clear which scene is to be loaded
        yield return new WaitForSeconds(0.5f); // Wait for half second
        TMPTool.SetHeader(loader.Header);
        StartCoroutine(loader.LoadSceneWithPreparation(sceneName)); // // Prepare to loading new scene 
    }
    public IEnumerator FadeInOut(Color startColor, Color end)
    {
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            Images[1].GetComponent<Image>().color = Color.Lerp(startColor, end, time / FadeDuration);
            yield return null;
        }

        Images[1].GetComponent<Image>().color = end;
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
            Images[10].rectTransform.sizeDelta = new Vector2(shieldSize, shieldSize);
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
    public IEnumerator PlaceFlag()
    {
        int Stage = ((PlatformManager)Platform).Stage;
        yield return new WaitUntil(() => Platform.Progress);
        objects.Last().transform.position = new Vector3(Stage + 6, 0.5f, Stage + 6);
    }
    private IEnumerator ShieldCounter()
    {
        Images[10].gameObject.SetActive(true);

        Buttons.Find(b => b.name == "Shield").interactable = false;

        playerController.shield.Play();

        Vector3 originalSize = texts[5].rectTransform.localScale;
        Color originalColor = texts[5].color;

        while (_shieldDuration > 0f)
        {
            StartCoroutine(ExtendAndFadeAnimation());
            yield return new WaitUntil(() => !isCounter);
        }

        playerController.GetComponent<OverlapBoxNonAllocPoller>().ShieldIsActive = false;

        _shieldDuration = 30f;

        texts[5].text = "";

        Images[10].gameObject.SetActive(false);

        if (PlayerPrefs.GetInt("Shield") == 0)
            Buttons.Find(b => b.name == "Shield").interactable = false;
        else
            Buttons.Find(b => b.name == "Shield").interactable = true;

        StartCoroutine(playerController.ShieldController(false));

        playerController.shield.Stop();
    }
}
