using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{

    [Header("Settings of Tutorials")]

    private readonly TMPTool TMPTool = new(null,0.01f ,0.3f);
    private UIController UIController;
    private Image TalkImage;
    private TMP_Text Talking;

    private float TextSize = 20f;
    private string FullText;
    
    private void Start()
    {
        TalkImage = GameObject.Find("Canvas/Talk").GetComponent<Image>();
        Talking = TalkImage.GetComponentInChildren<TMP_Text>();
        UIController = transform.GetComponent<UIController>();
        UIController.playerController.GetComponent<OverlapBoxNonAllocPoller>().enabled = false;
        Talk();
    }
    private void Talk()
    {
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
                StartCoroutine(UIController.ScalerMenu(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f), 0.8f, TalkImage));
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
        yield return StartCoroutine(TMPTool.PlayTypeWriterFade(FullText, TextSize, Talking));
        StartCoroutine(TouchToCountinue(step));
    }
    
}

