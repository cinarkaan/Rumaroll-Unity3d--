using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private float FixedDeltaTime = 0.027f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
#if UNITY_STANDALONE_WIN && UNITY_EDITOR
        Application.targetFrameRate = -1;
#else
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        Time.fixedDeltaTime = FixedDeltaTime;
#endif
    }

}
