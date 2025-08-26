using System.Collections;
using UnityEngine;

public class MobileTrail : MonoBehaviour
{
    public float Duration;
    public GameObject TrailPrefab;

    public void Trail ()
    {
        CreateTrail();
    }
    private void CreateTrail()
    {
        GameObject trail = Instantiate(TrailPrefab, transform.position, transform.rotation);
        trail.transform.position = transform.position;
        trail.transform.rotation = transform.rotation;
        trail.transform.localScale = transform.localScale;
        StartCoroutine(FadeOut(trail));
    }
    private IEnumerator FadeOut(GameObject trail)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Duration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            trail.GetComponent<Renderer>().material.SetFloat("_Power", alpha);
            yield return null;
        }
        trail.GetComponent<Renderer>().material.SetFloat("_Power", 0);
        Destroy(trail);
    }


}
