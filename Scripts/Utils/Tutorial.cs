using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

    [Header("Settings of Tutorials")]

    private readonly TypeWriter TypeWriter = new(0.01f ,0.3f);
    private UIController UIController;
    private Image TalkImage;
    private TMP_Text Talking;

    private float TextSize = 20f;
    private string FullText;
    public bool Tap;

    private void Start()
    {
        Tap = false;
        TalkImage = GameObject.Find("Canvas/Talk").GetComponent<Image>();
        Talking = TalkImage.GetComponentInChildren<TMP_Text>();
        UIController = transform.GetComponent<UIController>();
        UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = false;
        StartCoroutine(Talk());
    }
    private IEnumerator Talk()
    {
        yield return new WaitUntil(() => !Tap); 
        UIController.EventsManager(false);
        UIController.ButtonsManager(false);
        UIController.playerController.Render(false);
        FullText = "Welcome to the world of cube. The magIcal colorful cube needs to reach out evacutaIon poInt where Is located on the flag. ";
        StartCoroutine(ShowEvacutaionPoint(new Vector3(UIController.playerController.PlatformManager_.Stage + 6, UIController.playerController.transform.position.y, UIController.playerController.PlatformManager_.Stage + 6), 0));

    }
    private IEnumerator TouchToCountinue(int step)
    {
        TextSize = 19f;
        FullText = "As you can see left bottom of the screen , only, you are able to move cube at the each dIrectIon , If the bottom face color of cube mathces wIth tIle color. " +
        "The rIght bottom of the screen Is placed map, whenever you want to see where you are , you can check out both you and obstacles. Also you can demand clue , of course If you have." +
        " If everythInk Is okay , good luck ...";
        while (true)
        {
#if UNITY_STANDALONE_WIN
            if (Input.GetKeyDown(KeyCode.Space))
            {
                step++;
                StartCoroutine(UIController.FadeInOut(new Color(0f, 0f, 0f, 0.6f), Color.clear));
                StartCoroutine(UIController.ScalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, TalkImage));
                if (step < 2)
                    StartCoroutine(ShowEvacutaionPoint(new Vector3(6f, UIController.playerController.transform.position.y, 6f), step));
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
                StartCoroutine(UIController.FadeInOut(new Color(0f,0f,0f,0.6f), Color.clear));
                StartCoroutine(UIController.ScalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, _Talk));
                if (step < 2)
                    StartCoroutine(ShowEvacutaionPoint(new Vector3(6f, UIController.playerController.transform.position.y, 6f), step));
                else
                {
                    UIController.EventsManager(true);
                    UIController.ButtonsManager(true);
                    GameObject.Find("Canvas/Close").gameObject.SetActive(false);
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
    private IEnumerator ShowEvacutaionPoint(Vector3 camEnd, int step)
    {
        Vector3 velocity = Vector3.zero;
        while (Vector3.Distance(UIController.playerController.transform.position,camEnd) > 0.01f)
        {
            UIController.playerController.transform.position = Vector3.SmoothDamp(UIController.playerController.transform.position, camEnd, ref velocity, 0.3f);
            yield return null;
        }
        StartCoroutine(UIController.FadeInOut(Color.clear, new Color(0, 0, 0, 0.6f)));
        StartCoroutine(UIController.ScalerMenu(new Vector3(0f, 0f, 1f), new Vector3(1f, 1f, 1f), 0.8f, TalkImage));
        yield return StartCoroutine(TypeWriter.PlayTypeWriterFade(FullText, TextSize, Talking));
        StartCoroutine(TouchToCountinue(step));
    }
    
}

public class TypeWriter
{
    public float SpawnInterval = 0.1f;
    public float FadeDurationText = 0.3f;

    public TypeWriter ()
    {

    }
    public TypeWriter(float SpawnInterval, float FadeDurationText)
    {
        this.SpawnInterval = SpawnInterval;
        this.FadeDurationText = FadeDurationText;
    }

    public IEnumerator PlayTypeWriterFade(string Text, float TextSize, TMP_Text TMP_Text)
    {
        int n = Text.Length;
     
        // Total time : Start of the last word + fadeDuration
        float totalDuration = SpawnInterval * (n - 1) + FadeDurationText;
        float time = 0f;

        // At the each frame the text regenerate by using only one stringbuilder
        StringBuilder builder = new();

        TMP_Text.fontSize = TextSize;

        while (time < totalDuration)
        {
            builder.Length = 0;

            for (int i = 0; i < n; i++)
            {
                char c = Text[i];
                float charStart = i * SpawnInterval;
                float t = (time - charStart) / FadeDurationText;
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

            TMP_Text.text = builder.ToString();

            time += Time.deltaTime;
            yield return null;
        }

        TMP_Text.text = Text;
    }

    public IEnumerator PlayTypeWriterFade(string Text, int TextSize, Text text)
    {
        int n = Text.Length;

        // Total time : Start of the last word + fadeDuration
        float totalDuration = SpawnInterval * (n - 1) + FadeDurationText;
        float time = 0f;

        // At the each frame the text regenerate by using only one stringbuilder
        StringBuilder builder = new();

        text.fontSize = TextSize;

        while (time < totalDuration)
        {
            builder.Length = 0;

            for (int i = 0; i < n; i++)
            {
                char c = Text[i];
                float charStart = i * SpawnInterval;
                float t = (time - charStart) / FadeDurationText;
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

            text.text = builder.ToString();

            time += Time.deltaTime;
            yield return null;
        }

        text.text = Text;
    }

}

