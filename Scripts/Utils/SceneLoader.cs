using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string CurrentScene { get; private set; } // Current index when it run
    
    public TMP_Text LoadingText { get; private set; } // Child text of this script
    private AsyncOperation AsyncLoad; 
    private readonly List<GameObject> Root = new(); // Root objects that in the tutorial scene

    // it will be use remowe the waiting screen to be notificated by either host or client from server side on multiplayer
    public int Operation = 1;

    private void Start()
    {
        CurrentScene = SceneManager.GetActiveScene().name;
        LoadingText = transform.GetChild(1).GetComponent<TMP_Text>();
    }
    private IEnumerator FadeText ()
    {
        float t = 0f;
        while (true)
        {
            float alpha = Mathf.PingPong(t * 1f, 1f);
            LoadingText.color = new Color(LoadingText.color.r, LoadingText.color.g, LoadingText.color.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator LoadSceneWithPreparation(string targetSceneName)
    {  
        AsyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);   
     
        AsyncLoad.allowSceneActivation = false;

        while (AsyncLoad.progress < 0.9f)
        {
            LoadingText.text = "LOADING % " + (int)(AsyncLoad.progress * 100);
            yield return null;
        }

        LoadingText.rectTransform.localPosition = new Vector3(LoadingText.rectTransform.localPosition.x + (-80f) , LoadingText.rectTransform.localPosition.y , LoadingText.rectTransform.localPosition.z);

        LoadingText.text = "TAP TO CONTINUE";

        StartCoroutine(FadeText());

#if UNITY_STANDALONE_WIN
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
#else
        yield return new WaitUntil(() => Input.touchCount != 0);
#endif
        yield return new WaitForSeconds(0.5f);

        AsyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => !AsyncLoad.isDone);

        Scene loaded = SceneManager.GetSceneByName(targetSceneName);

        SceneManager.SetActiveScene(loaded);

        loaded.GetRootGameObjects(Root);

        SceneManager.UnloadSceneAsync(CurrentScene);

        CurrentScene = targetSceneName;

    }
    public IEnumerator LoadSceneMultiplayer(AsyncOperation asyncOperation)
    {

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 1f);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = (1f > 0f);
        canvasGroup.blocksRaycasts = (1f> 0f);

        CurrentScene = "Multiplayer";

        while (asyncOperation.progress < 0.9f)
        {
            LoadingText.text = "LOADING % " + (int)(AsyncLoad.progress * 100);

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        asyncOperation.allowSceneActivation = true;

        yield return new WaitUntil(() => !asyncOperation.isDone);

    }
    public IEnumerator RemoveWaiting (int max, string playerType)
    {
        CanvasGroup load = GetComponent<CanvasGroup>();
        float elapsed = 0f;

        StartCoroutine(FadeText());

        if (playerType.Equals("Host"))
        {
            LoadingText.text = $"WAITING FOR PLAYERS {Operation}/{max}";
            yield return new WaitUntil(() => Operation > 1);
        } else
        {
            LoadingText.rectTransform.localPosition = new Vector3(-105f, -112f, 0f);
            LoadingText.text = $"WAITING FOR HOST";
            yield return new WaitUntil(() => Operation > 1);
        }

        yield return new WaitForSeconds(1f);

        while (elapsed < 0.5f)
        {
            elapsed += Time.deltaTime;
            load.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.5f);
            yield return null;
        }

        load.alpha = 0f;
        load.interactable = false;
        load.blocksRaycasts = false;
    }

}
