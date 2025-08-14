using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] private CanvasGroup[] uiMenu;
    [SerializeField] public CanvasGroup[] UIMenu => uiMenu;
    
    [SerializeField] private RectTransform icon;
    [SerializeField] public RectTransform Icon => icon;

    [SerializeField] private Toggle[] toggles;
    [SerializeField] public Toggle[] Toggles => toggles;

    [SerializeField] private SceneLoader loader;
    [SerializeField] public SceneLoader Loader => loader;

    [SerializeField] 
    private TMP_Text[] events;
    [SerializeField] 
    private List<Image> images;
    [SerializeField]
    private AudioSource _click_Sfx;

    public ParticleSystem Vfx_Mystical_Scatter;

    [SerializeField]
    private Slider touchSensitivity;
    [SerializeField]
    private float iconDuration = 0.7f;
    [SerializeField]
    private float duration;
    [SerializeField]
    private Image frame;

    private float _sensitivity = 10f;

    private float t = 0f;

    private MultiPlayerMenu MultiPlayerMenu;

    public void Start()
    {
        Application.targetFrameRate = -1; // Remove FPS Limit

        StartCoroutine(fadeImage(images.First().color, Color.clear, duration,images.First())); // Launch fade animation only when app start

        MultiPlayerMenu = GetComponent<MultiPlayerMenu>();

        CleanUpDestory(); // Clean the network objects that remains from multiplayer

        initialize(); // Adjust save - load system as automatically
    }
    private void LateUpdate()
    {
        t += Time.deltaTime * 0.2f;

        frame.color = Color.HSVToRGB(Mathf.Repeat(t, 1f), 1f, 1f);
        frame.transform.GetChild(0).GetComponent<Image>().color = frame.color;


#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(images.Last().rectTransform.root.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out Vector2 local);
            ParticleSystem _clicked = Instantiate(
                Vfx_Mystical_Scatter,
                new Vector3(local.x, local.y, 100.5f),
                Quaternion.Euler(180f, 0f, 90f), images.Last().rectTransform.root);
            _clicked.GetComponent<RectTransform>().localPosition = new Vector3(local.x, local.y, -150);
            Destroy(_clicked.gameObject, 1.5f);
        }

#endif

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && uiMenu[2].alpha == 0f && PlayerPrefs.GetInt("Vfx") == 1)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(images.Last().rectTransform.root.GetComponent<RectTransform>(), Input.GetTouch(0).position, Camera.main, out Vector2 local);
            ParticleSystem _clicked = Instantiate(
                Vfx_Mystical_Scatter,
                new Vector3(local.x, local.y, 100.5f),
                Quaternion.Euler(180f, 0f, 90f), images.Last().rectTransform.root);
            _clicked.GetComponent<RectTransform>().localPosition = new Vector3(local.x, local.y, -150);
            Destroy(_clicked.gameObject, 1.5f);
        }

        float size = Mathf.PingPong(Time.time * 60f, 40f) + 100f;
        icon.sizeDelta = new Vector2(size, size);
        icon.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }
    private void initialize()
    {
        if (!PlayerPrefs.HasKey("Stage"))
        {
            PlayerPrefs.SetInt("Stage", 4);
            PlayerPrefs.SetFloat("Touch Sensitivity", 30f);
            PlayerPrefs.SetInt("Coin", 0);
            PlayerPrefs.SetInt("Diamond", 0);
            PlayerPrefs.SetInt("Clue", 0);
            PlayerPrefs.SetInt("Shield", 0);
            PlayerPrefs.SetInt("Fps", 1);
            PlayerPrefs.SetInt("Vfx", 0);
            PlayerPrefs.SetInt("Post Processing", 0);
            PlayerPrefs.SetInt("Sfx", 0);
            PlayerPrefs.SetInt("Music", 0);
            PlayerPrefs.Save();
        }

        events[0].text = ": " + PlayerPrefs.GetInt("Coin").ToString();
        events[1].text = ": " + PlayerPrefs.GetInt("Diamond").ToString();
        events[2].text = ": " + PlayerPrefs.GetInt("Shield").ToString();
        events[3].text = ": " + PlayerPrefs.GetInt("Clue").ToString();
        toggles[2].isOn = PlayerPrefs.GetInt("Sfx") == 1;
        toggles[1].isOn = PlayerPrefs.GetInt("Music") == 1;
        toggles[3].isOn = PlayerPrefs.GetInt("Vfx") == 1;
        toggles[4].isOn = PlayerPrefs.GetInt("Post Processing") == 1;
        touchSensitivity.value = PlayerPrefs.GetFloat("Touch Sensitivity");
    }
    public void newGameButton ()
    {
        PlaySFX();
        Vector3 start = icon.localPosition;
        Vector3 end = new Vector3(-600f, 47f, 0);
        StartCoroutine(selectedIcon(start,end));
        if (PlayerPrefs.GetInt("Stage") > 4)
        {
            StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu.Last(), ""));
            StartCoroutine(scalerMenu(images[1].rectTransform.localScale, new Vector3(1.5f, 1.5f, 1), images[1]));
        }
        else
        {
            StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu.Last(), ""));
            StartCoroutine(fadeCanvasGroup(1f, 1f, uiMenu[3], "Tutorial"));
        }

    }
    public void yes ()
    {
        images[1].rectTransform.localScale = Vector3.zero;
        PlayerPrefs.SetInt("Stage", 4);
        StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu.Last(), ""));
        StartCoroutine(fadeCanvasGroup(1, 1, uiMenu[3], "Tutorial"));
    }
    public void no ()
    {
        StartCoroutine(scalerMenu(images[1].rectTransform.localScale, new Vector3(0, 0, 1),images[1]));
    }
    public void quitGameButton()
    {
        PlaySFX();
        Application.Quit();
    }
    public void continueButton ()
    {
        PlaySFX();
        Vector3 start = icon.localPosition;
        Vector3 end = new Vector3(-600f, -85f, 0);
        StartCoroutine(selectedIcon(start, end));
        if (PlayerPrefs.GetInt("Stage") > 4)
        {
            StartCoroutine(fadeCanvasGroup(1, 1, uiMenu[2], ""));
            StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu.Last(), ""));
        }
    }
    public void buyShield ()
    {
        int current = PlayerPrefs.GetInt("Shield");
        PlayerPrefs.SetInt("Shield", current + 1);
        events[2].text = ": " + (current + 1).ToString();
    }
    public void buyClue ()
    {
        int current = PlayerPrefs.GetInt("Clue");
        PlayerPrefs.SetInt("Clue", current + 1);
        events[3].text = ": " + (current + 1).ToString();
    }
    public void backMenu ()
    {
        PlaySFX();
        StartCoroutine(fadeCanvasGroup(0, 1, uiMenu[2],""));
        StartCoroutine(fadeCanvasGroup(1f , 1f, uiMenu.Last(), ""));
    }
    public void startGame ()
    {
        PlaySFX();
        StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu.Last(), ""));
        StartCoroutine(fadeCanvasGroup(1, 1, uiMenu[3],"Day"));
    } 
    public void settings ()
    {
        PlaySFX();
        Vector3 start = icon.localPosition;
        Vector3 end = new Vector3(-600f, -310f, 0f);
        StartCoroutine(selectedIcon(start, end));
        StartCoroutine(fadeCanvasGroup(1, 1, uiMenu[1], ""));
        StartCoroutine(fadeCanvasGroup(0, 1, uiMenu.First(), ""));
        StartCoroutine(fadeCanvasGroup(0, 1, uiMenu.Last(), ""));
    }
    public void multiplayer ()
    {
        MultiPlayerMenu.enabled = true;
        PlaySFX();
        if (!WiFi())
        {
            StartCoroutine(scalerMenu(images.Last().rectTransform.localScale, new Vector3(1f, 1f, 1f), images.Last()));
            return;
        }
        Vector3 start = icon.localPosition;
        Vector3 end = new Vector3(-600f, -205f, 0f);
        StartCoroutine(selectedIcon(start, end));
        StartCoroutine(fadeCanvasGroup(1, 1, uiMenu[4], ""));
        StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu[0], ""));
    }
    public void saveSettings ()
    {
        PlaySFX();
        PlayerPrefs.SetFloat("Touch Sensitivity", _sensitivity);
        StartCoroutine(fadeCanvasGroup(1f, 1f, uiMenu.First(), ""));
        StartCoroutine(fadeCanvasGroup(0f, 1f, uiMenu[1], ""));
        StartCoroutine(fadeCanvasGroup(1f, 1f, uiMenu.Last(), ""));
    }
    public void checkMusic (bool music)
    {
        music = toggles[1].isOn;
        PlayerPrefs.SetInt("Music", music ? 1 : 0);
    }
    public void CheckSfx (bool Sfx)
    {
        Sfx = toggles[2].isOn;
        PlayerPrefs.SetInt("Sfx", Sfx ? 1 : 0);
    }
    public void CheckVFX (bool vfx)
    {
        vfx = toggles[3].isOn;
        PlayerPrefs.SetInt("Vfx", vfx ? 1 : 0);
    }
    public void checkFPS(bool fps)
    {
        fps = toggles[0].isOn;
        PlayerPrefs.SetInt("Fps", fps ? 1 : 0);
    }
    public void sliderChanger ()
    {
        _sensitivity = touchSensitivity.value;
    }
    public void postProcessingChecker(bool postProcessing)
    {
        postProcessing = toggles[4].isOn;
        PlayerPrefs.SetInt("Post Processing", postProcessing ? 1 : 0);
    }
    private IEnumerator fadeImage(Color start, Color end, float time,Image image)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            image.color = Color.Lerp(start, end, t);
            yield return null;
        }
        image.color = end;
    }
    public IEnumerator selectedIcon (Vector3 start , Vector3 End)
    {
        float elapsed = 0f;
        while(elapsed < iconDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / iconDuration);
            icon.localPosition = Vector3.Lerp(start, End, t);
            yield return null;
        }
    }
    private IEnumerator scalerMenu (Vector3 start , Vector3 end,Image image)
    {
        float elapsed = 0f;
        while(elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / 0.5f);
            image.rectTransform.localScale = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }
    public IEnumerator fadeCanvasGroup(float targetAlpha, float duration, CanvasGroup cv, string name)
    {
        float startAlpha = cv.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cv.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        cv.alpha = targetAlpha;
        cv.interactable = (targetAlpha > 0f);
        cv.blocksRaycasts = (targetAlpha > 0f);

        if (cv.name == "LoadingScene")
            StartCoroutine(loader.LoadSceneWithPreparation(name));
    }
    private bool WiFi ()
    {
# if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            try
            {
                using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                {
                    return wifiManager.Call<bool>("isWifiEnabled");
                }
            }
            catch (Exception e)
            {

            }
        }
    #endif
        return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
    }
    public void CloseWarning ()
    {
        StartCoroutine(scalerMenu(images.Last().rectTransform.localScale, new Vector3(0, 0, 1), images.Last()));
    }
    public void PlaySFX ()
    {
        if (PlayerPrefs.GetInt("Sfx") == 1)
            _click_Sfx.Play();
    }
    public void CleanUpDestory ()
    {
        var access = new GameObject("Access");
        DontDestroyOnLoad(access);
        var objs = access.scene.GetRootGameObjects();
        if (objs.Length > 1)
            for (int i = 1; i < objs.Length; i++)
                Destroy(objs[i]);
    }
}
