using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

    [Header("Settings of Animations")]
    [Tooltip("Her bir karakterin fade?in süresi (saniye)")]
    [Range(0.01f, 1f)]
    public float fadeDurationText = 0.3f;

    [Tooltip("Bir karakter tamamen fade?in olduktan sonra sonraki karaktere geçme süresi (saniye)")]
    [Range(0f, 0.2f)]
    public float intervalBetweenChars = 0.05f;

    [Tooltip("Harfler arasý baþlangýç gecikmesi (saniye)\nfadeDuration’dan küçük seçilirse fade süreçleri üst üste binecek.")]
    [Range(0.01f, 0.5f)]
    public float spawnInterval = 0.1f;

    public bool tap;

    private Image _talk;
    private string fullText;

    private UIController UIController;

    void Start()
    {
        tap = false;
        _talk = GameObject.Find("Canvas/Talk").GetComponent<Image>();
        UIController = transform.GetComponentInChildren<UIController>();
        UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = false;
        StartCoroutine(talk());
    }
  
    private IEnumerator talk()
    {
        yield return new WaitUntil(() => !tap); 
        UIController.EventsManager(false);
        UIController.ButtonsManager(false);
        UIController.playerController.Render(false);
        fullText = "Welcome to the world of cube. The magical colorful cube needs to reach out evacutaion point where is located on the flag. ";
        StartCoroutine(showEvacutaionPoint(new Vector3(UIController.playerController.getGridManager().stage + 6, UIController.playerController.transform.position.y, UIController.playerController.getGridManager().stage + 6), 0));
    }
    private IEnumerator touchToCountinue(int step)
    {
        fullText = "As you can see left bottom of the screen , only, you are able to move cube at the each direction , if the bottom face color of cube mathces with tile color. " +
        "The right bottom of the screen is placed map, whenever you want to see where you are , you can check out both you and obstacles. Also you can demand clue , of course if you have" +
        " If everythink is okay , good luck ...";
        while (true)
        {
#if UNITY_STANDALONE_WIN
            if (Input.GetKeyDown(KeyCode.Space))
            {
                step++;
                StartCoroutine(UIController.FadeInOut(new Color(0f, 0f, 0f, 0.6f), Color.clear, 1f));
                StartCoroutine(UIController.scalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, _talk));
                if (step < 2)
                    StartCoroutine(showEvacutaionPoint(new Vector3(6f, UIController.playerController.transform.position.y, 6f), step));
                else
                {
                    UIController.EventsManager(true);
                    UIController.ButtonsManager(true);
                    GameObject.Find("Canvas/Close").SetActive(false);
                    UIController.playerController.Render(true);
                    UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = true;
                }
                break;
            }
#else
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                step++;
                StartCoroutine(UIController.fadeInOut(new Color(0f,0f,0f,0.6f), Color.clear, 1f));
                StartCoroutine(UIController.scalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, false, _talk));
                if (step < 2)
                    StartCoroutine(showEvacutaionPoint(new Vector3(6f, UIController.playerController.transform.position.y, 6f), step));
                else
                {
                    UIController.eventsManager(true);
                    UIController.buttonsManager(true);
                    GameObject.Find("Canvas/Close").SetActive(false);
                    UIController.playerController.Render(true);
                    UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = true;
                }
                break;
            }
#endif
            /*if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                step++;
                StartCoroutine(UIController.fadeInOut(new Color(0f,0f,0f,0.6f), Color.clear, 1f));
                StartCoroutine(UIController.scalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, false, _talk));
                if (step < 2)
                    StartCoroutine(showEvacutaionPoint(new Vector3(6f, UIController.playerController.transform.position.y, 6f), step));
                else
                {
                    UIController.eventsManager(true);
                    UIController.buttonsManager(true);
                    GameObject.Find("Canvas/Close").SetActive(false);
                    UIController.playerController.Render(true);
                    UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = true;
                }
                break;
            }*/
            yield return null;
        }

    }
    private IEnumerator showEvacutaionPoint(Vector3 camEnd, int step)
    {
        Vector3 velocity = Vector3.zero;
        while (Vector3.Distance(UIController.playerController.transform.position,camEnd) > 0.01f)
        {
            UIController.playerController.transform.position = Vector3.SmoothDamp(UIController.playerController.transform.position, camEnd, ref velocity, 0.3f);
            yield return null;
        }
        StartCoroutine(UIController.FadeInOut(Color.clear, new Color(0, 0, 0, 0.6f), 1f));
        StartCoroutine(UIController.scalerMenu(new Vector3(0f, 0f, 1f), new Vector3(1f, 1f, 1f), 0.8f, _talk));
        StartCoroutine(PlayTypewriterFade(step));
    }
    private IEnumerator PlayTypewriterFade(int step)
    {
        int n = fullText.Length;
        // Total time : Start of the last word + fadeDuration
        float totalDuration = spawnInterval * (n - 1) + fadeDurationText;
        float time = 0f;

        // At the each frame the text regenerate by using only one stringbuilder
        StringBuilder builder = new StringBuilder();

        while (time < totalDuration)
        {
            builder.Length = 0;

            for (int i = 0; i < n; i++)
            {
                char c = fullText[i];
                float charStart = i * spawnInterval;
                float t = (time - charStart) / fadeDurationText;
                t = Mathf.Clamp01(t);

                // Add either space or new line
                if (c == ' ' || c == '\n')
                {
                    builder.Append(c);
                    continue;
                }

                if (t <= 0f)
                {
                    // Did it start ? alfa = 00 (Full Transparent)
                    builder.Append("<color=#00000000>");
                    builder.Append(c);
                    builder.Append("</color>");
                }
                else if (t >= 1f)
                {
                    // Fade was complated ? Full black (without tag, opaque)
                    builder.Append(c);
                }
                else
                {
                    // 0 < t < 1 ? alfa = between 0?255 
                    byte alphaByte = (byte)Mathf.RoundToInt(Mathf.Lerp(0, 255, t));
                    string hex = alphaByte.ToString("X2");  // "00" ... "FF"
                    builder.Append("<color=#000000");
                    builder.Append(hex);
                    builder.Append(">");
                    builder.Append(c);
                    builder.Append("</color>");
                }
            }

            _talk.GetComponentInChildren<Text>().text = builder.ToString();

            time += Time.deltaTime;
            yield return null;
        }

        //The text will be complately black and without tag as soon as complate the animation
        _talk.GetComponentInChildren<Text>().text = fullText;
        StartCoroutine(touchToCountinue(step));
    }

}
