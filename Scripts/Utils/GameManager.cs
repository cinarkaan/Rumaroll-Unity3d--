using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{

#if UNITY_ANDROID
    private int ScreenRefreshRate = 60;
#endif

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        Application.targetFrameRate = -1;
#elif UNITY_ANDROID && !UNITY_EDITOR
        ScreenRefreshRate = (int)Screen.currentResolution.refreshRateRatio.value + 10;
        Debug.Log("The target frame rate has been setted as : " + ScreenRefreshRate);
        Application.targetFrameRate = ScreenRefreshRate;
#endif
    }

    private void Start()
    {
        StartCoroutine(PostProcessing());
    }

    public static IEnumerator PostProcessing ()
    {
        yield return new WaitUntil(() => Camera.main != null);

        Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = PlayerPrefs.GetInt("Post Processing") > 0;
    }


}



