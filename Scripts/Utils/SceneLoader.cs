using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static string currentScene { get; private set; } // Current index when it run
    public TMP_Text loadingText { get; private set; } // Child text of this script
    private AsyncOperation asyncLoad; 
    private List<GameObject> roots = new List<GameObject>(); // Root objects that in the tutorial scene

    // it will be use remowe the waiting screen to be notificated by either host or client from server side on multiplayer
    public int operation = 1;

    void Start()
    {
        SceneLoader.currentScene = SceneManager.GetActiveScene().name;
        loadingText = transform.GetChild(1).GetComponent<TMP_Text>();
    }
    private IEnumerator fadeText ()
    {
        float t = 0f;
        while (true)
        {
            float alpha = Mathf.PingPong(t * 1f, 1f);
            loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
    }
    public IEnumerator LoadSceneWithPreparation(string targetSceneName)
    {
        asyncLoad = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);   
     
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            loadingText.text = "LOADING % " + (int)(asyncLoad.progress * 100);
            yield return null;
        }

        loadingText.rectTransform.localPosition = new Vector3(loadingText.rectTransform.localPosition.x + (-80f) , loadingText.rectTransform.localPosition.y , loadingText.rectTransform.localPosition.z);

        loadingText.text = "TAP TO CONTINUE";

        StartCoroutine(fadeText());

#if UNITY_STANDALONE_WIN
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
#else
        yield return new WaitUntil(() => Input.touchCount != 0);
#endif

        yield return new WaitForSeconds(0.5f);

        asyncLoad.allowSceneActivation = true;

        yield return new WaitUntil(() => !asyncLoad.isDone);
        
        Scene loaded = SceneManager.GetSceneByName(targetSceneName);

        SceneManager.SetActiveScene(loaded);

        loaded.GetRootGameObjects(roots);

        if (targetSceneName == "Tutorial")
            roots.Find(o => o.name == "UIManager").GetComponent<Tutorial>().tap = true;

        SceneManager.UnloadSceneAsync(SceneLoader.currentScene);

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

        while (asyncOperation.progress < 0.9f)
        {
            loadingText.text = "LOADING % " + (int)(asyncLoad.progress * 100);

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

        StartCoroutine(fadeText());

        if (playerType.Equals("Host"))
        {
            loadingText.text = $"WAITING FOR PLAYERS {operation}/{max}";
            yield return new WaitUntil(() => operation > 1);
        } else
        {
            loadingText.rectTransform.localPosition = new Vector3(-105f, -112f, 0f);
            loadingText.text = $"WAITING FOR HOST";
            yield return new WaitUntil(() => operation > 1);
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
