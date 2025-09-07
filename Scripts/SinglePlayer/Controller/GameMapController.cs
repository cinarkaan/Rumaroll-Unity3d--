using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;


public class GameMapController : MonoBehaviour
{             
    public RenderTexture rt;
    public int currentIndex = 1;

    public int OriginalCulllingIndex { get; private set; }

    [SerializeField]
    private PlatformManager platformManager;

    [SerializeField]
    private UIController UIController;

    private Vector3 originPos = new(9, 0, 9);
    private readonly float Ort = 3.5f;

    private void Start()
    {
        GetComponent<Camera>().GetUniversalAdditionalCameraData().renderPostProcessing = PlayerPrefs.GetInt("Post Processing") > 0;
        StartCoroutine(InitializeMapCamera());
        RenderController(UIController.RawImage);
    }
    public void RenderController (RawImage gameMap)
    {
        if (currentIndex == 0)
        {
            Camera.main.cullingMask = LayerMask.GetMask("UI");
            transform.GetComponent<Camera>().enabled = true;
            transform.GetComponent<Camera>().targetTexture = rt;
            transform.GetComponent<Camera>().Render();
            gameMap.texture = rt;
        }
        else
        {
            transform.GetComponent<Camera>().enabled = false;
            transform.GetComponent<Camera>().targetTexture = null;
            gameMap.texture = null;

        }
    } // Which camera must be activated 
    private IEnumerator InitializeMapCamera () // Init cam positions according to the map
    {
        yield return new WaitUntil(() => platformManager.Progress);
        float factor = (platformManager.Stage - 6) * 0.5f;
        transform.GetComponent<Camera>().orthographicSize = Ort + factor;
        transform.position = new Vector3(originPos.x + factor, 0.2f, originPos.z + factor);
        yield return new WaitUntil(() => Camera.main != null);
        OriginalCulllingIndex = Camera.main.cullingMask;
    }  


}
