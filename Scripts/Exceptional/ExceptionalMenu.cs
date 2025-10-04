using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExceptionalMenu : MonoBehaviour
{
    [SerializeField] protected CanvasGroup[] UIMenu;
    [SerializeField] protected Toggle[] Toggle;
    [SerializeField] protected TMP_Text[] Events;
    [SerializeField] protected SceneLoader SceneLoader;
    [SerializeField] protected AudioSource ClickSfx;
    [SerializeField] protected ParticleSystem Clicked;
    [SerializeField] protected RectTransform SelectiveIcon;
    [SerializeField] protected float IconMoveSpeed = 0.5f;
    [SerializeField] protected float FadeDuration = 1f;
 
    protected TMPTool TMPTool;

    protected virtual void Start ()
    {
    }
    protected IEnumerator FadeCanvasGroup(float targetAlpha, float duration, CanvasGroup cv, string name)
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
        {
            TMPTool.SetHeader(SceneLoader.Header);
            StartCoroutine(SceneLoader.LoadSceneWithPreparation(name));
        }
    }
    protected IEnumerator MoveIcon(Vector3 start, Vector3 End)
    {
        float elapsed = 0f;
        while (elapsed < IconMoveSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / IconMoveSpeed);
            SelectiveIcon.localPosition = Vector3.Lerp(start, End, t);
            yield return null;
        }
    }
    protected void PlaySFX()
    {
        if (PlayerPrefs.GetInt("Sfx") == 1)
            ClickSfx.Play();
    }
}
