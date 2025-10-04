using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : ExceptionalMenu
{
    [SerializeField] private List<Image> images;

    [SerializeField] private Slider touchSensitivity;

    private MultiPlayerMenu MultiPlayerMenu;

    private float SwipeSensitivity = 10f;

    public TMPTool TMPTool_ => TMPTool;

    protected override void Start()
    {
        StartCoroutine(FadeImage(images.First().color, Color.clear, FadeDuration,images.First())); // Launch fade animation only when app start

        MultiPlayerMenu = GetComponent<MultiPlayerMenu>();

        CleanUpDestory(); // Clean the network objects that remains from multiplayer

        Initialize(); // Adjust save - load system as automatically
    }
    private void LateUpdate()
    {
#if UNITY_STANDALONE_WIN
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(images.Last().rectTransform.root.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out Vector2 local);
            ParticleSystem _clicked = Instantiate(
                Clicked,
                new Vector3(local.x, local.y, 100.5f),
                Quaternion.Euler(180f, 0f, 90f), images.Last().rectTransform.root);
            _clicked.GetComponent<RectTransform>().localPosition = new Vector3(local.x, local.y, -150);
            Destroy(_clicked.gameObject, 1.5f);
        }

#endif
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && UIMenu[2].alpha == 0f && PlayerPrefs.GetInt("Vfx") == 1)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(images.Last().rectTransform.root.GetComponent<RectTransform>(), Input.GetTouch(0).position, Camera.main, out Vector2 local);
            ParticleSystem _clicked = Instantiate(
                Clicked,
                new Vector3(local.x, local.y, 100.5f),
                Quaternion.Euler(180f, 0f, 90f), images.Last().rectTransform.root);
            _clicked.GetComponent<RectTransform>().localPosition = new Vector3(local.x, local.y, -150);
            Destroy(_clicked.gameObject, 1.5f);
        }

        float size = Mathf.PingPong(Time.time * 60f, 40f) + 100f;
        SelectiveIcon.sizeDelta = new Vector2(size, size);
        SelectiveIcon.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

        TMPTool.WaveHeader();
    }
    private void Initialize()
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
            PlayerPrefs.SetInt("Vfx", 1);
            PlayerPrefs.SetInt("Post Processing", 1);
            PlayerPrefs.SetInt("Sfx", 1);
            PlayerPrefs.Save();
        }

        Events[0].text = ": " + PlayerPrefs.GetInt("Coin").ToString();
        Events[1].text = ": " + PlayerPrefs.GetInt("Diamond").ToString();
        Events[2].text = "OWNED : " + PlayerPrefs.GetInt("Shield").ToString();
        Events[3].text = "OWNED : " + PlayerPrefs.GetInt("Clue").ToString();
        Toggle[1].isOn = PlayerPrefs.GetInt("Sfx") == 1;
        Toggle[2].isOn = PlayerPrefs.GetInt("Vfx") == 1;
        Toggle[3].isOn = PlayerPrefs.GetInt("Post Processing") == 1;
        touchSensitivity.value = PlayerPrefs.GetFloat("Touch Sensitivity");
        TMPTool = new(Events.Skip(4).Take(5).Cast<TextMeshProUGUI>().ToArray(),0,0);
        TMPTool.SetHeader(Events[9]);
    }
    public void NewGameButton ()
    {
        PlaySFX();
        Vector3 start = SelectiveIcon.localPosition;
        Vector3 end = new(-600f, 195f, 0);
        StartCoroutine(MoveIcon(start,end));
        if (PlayerPrefs.GetInt("Stage") > 4)
            StartCoroutine(ScalerMenu(images[1].rectTransform.localScale, new Vector3(1.2f, 1.2f, 1), images[1]));
        else
        {
            StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
            StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[3], "Tutorial"));
        }
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[5], ""));
    }
    public void Yes ()
    {
        images[1].rectTransform.localScale = Vector3.zero;
        PlayerPrefs.SetInt("Stage", 4);
        StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[3], "Tutorial"));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
    }
    public void No ()
    {
        StartCoroutine(FadeCanvasGroup(1f, 0f, UIMenu[5], ""));
        StartCoroutine(ScalerMenu(images[1].rectTransform.localScale, new Vector3(0, 0, 1),images[1]));
    }
    public void QuitGameButton()
    {
        Application.Quit();
    }
    public void ContinueButton ()
    {
        PlaySFX();
        Vector3 start = SelectiveIcon.localPosition;
        Vector3 end = new(-600f, 75f, 0);
        StartCoroutine(MoveIcon(start, end));
        if (PlayerPrefs.GetInt("Stage") > 4)
        {
            StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[2], ""));
            StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu.Last(), ""));
            StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
        }
    }
    public void BuyShield ()
    {
        int current = PlayerPrefs.GetInt("Shield");
        PlayerPrefs.SetInt("Shield", current + 1);
        Events[2].text = "OWNED : " + (current + 1).ToString();
    }
    public void BuyClue ()
    {
        int current = PlayerPrefs.GetInt("Clue");
        PlayerPrefs.SetInt("Clue", current + 1);
        Events[3].text = "OWNED : " + (current + 1).ToString();
    }
    public void BackMenu ()
    {
        PlaySFX();
        StartCoroutine(FadeCanvasGroup(0, 1, UIMenu[2],""));
        StartCoroutine(FadeCanvasGroup(1f , 1f, UIMenu[5], ""));
    }
    public void StartGame ()
    {
        PlaySFX();
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[5], ""));
        StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[3],"Day"));
    } 
    public void Settings ()
    {
        PlaySFX();
        Vector3 start = SelectiveIcon.localPosition;
        Vector3 end = new(-600f, -155f, 0f);
        StartCoroutine(MoveIcon(start, end));
        StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[1], ""));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());
        StartCoroutine(FadeCanvasGroup(0, 1, UIMenu[0], ""));
        StartCoroutine(FadeCanvasGroup(0, 1, UIMenu[5], ""));
    }
    public void Multiplayer ()
    {
        PlaySFX();
        if (!WiFi())
        {
            StartCoroutine(ScalerMenu(images.Last().rectTransform.localScale, new Vector3(1f, 1f, 1f), images.Last()));
            return;
        }
        MultiPlayerMenu.enabled = true;
        Vector3 start = SelectiveIcon.localPosition;
        Vector3 end = new(-600f, -45f, 0f);
        StartCoroutine(MoveIcon(start, end));
        TMPTool.SetHeader(Events[10]);
        StartCoroutine(FadeCanvasGroup(1, 1, UIMenu[4], ""));
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[0], ""));
        StartCoroutine(TMPTool.AnimateTextsOutAllMenu());

    }
    public void SaveSettings ()
    {
        PlaySFX();
        PlayerPrefs.SetFloat("Touch Sensitivity", SwipeSensitivity);
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[0], ""));
        StartCoroutine(FadeCanvasGroup(0f, 1f, UIMenu[1], ""));
        StartCoroutine(FadeCanvasGroup(1f, 1f, UIMenu[5], ""));
    }
    public void CheckSfx (bool Sfx)
    {
        Sfx = Toggle[1].isOn;
        PlayerPrefs.SetInt("Sfx", Sfx ? 1 : 0);
    }
    public void CheckVFX (bool Vfx)
    {
        Vfx = Toggle[2].isOn;
        PlayerPrefs.SetInt("Vfx", Vfx ? 1 : 0);
    }
    public void CheckFPS(bool Fps)
    {
        Fps = Toggle[0].isOn;
        PlayerPrefs.SetInt("Fps", Fps ? 1 : 0);
    }
    public void SliderChanger ()
    {
        SwipeSensitivity = touchSensitivity.value;
    }
    public void PostProcessingChecker(bool PostProcessing)
    {
        PostProcessing = Toggle[3].isOn;
        PlayerPrefs.SetInt("Post Processing", PostProcessing ? 1 : 0);
    }
    private IEnumerator FadeImage(Color start, Color end, float time,Image image)
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
    private IEnumerator ScalerMenu (Vector3 start , Vector3 end,Image image)
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
        StartCoroutine(ScalerMenu(images.Last().rectTransform.localScale, new Vector3(0, 0, 1), images.Last()));
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
