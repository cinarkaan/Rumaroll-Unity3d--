using System.Collections;
using UnityEngine;
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

    private Vector3 originPos = new Vector3(9, 0, 9);
    private float ort = 3.5f;

    private void Start()
    {
        StartCoroutine(initializeMapCamera());
        renderController(UIController.RawImage);
    }
    public void renderController (RawImage gameMap)
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
    private IEnumerator initializeMapCamera () // Init cam positions according to the map
    {
        yield return new WaitUntil(() => platformManager.progress);
        float factor = (platformManager.stage - 6) * 0.5f;
        transform.GetComponent<Camera>().orthographicSize = ort + factor;
        transform.position = new Vector3(originPos.x + factor, 0.2f, originPos.z + factor);
        yield return new WaitUntil(() => Camera.main != null);
        OriginalCulllingIndex = Camera.main.cullingMask;
    }  


}
