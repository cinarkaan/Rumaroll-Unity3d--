using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
#if UNITY_STANDALONE_WIN && UNITY_EDITOR
        Application.targetFrameRate = -1;
#else
        Application.targetFrameRate = 120;
#endif
        DontDestroyOnLoad(gameObject);
    }


}
