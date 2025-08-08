using System.Collections;
using UnityEngine;

public class MobileTrail : MonoBehaviour
{
    public float duration;
    public GameObject trailPrefab;

    public void Trail ()
    {
        createTrail();
    }
    private void createTrail()
    {
        GameObject trail = Instantiate(trailPrefab, transform.position, transform.rotation);
        trail.transform.position = transform.position;
        trail.transform.rotation = transform.rotation;
        trail.transform.localScale = transform.localScale;
        StartCoroutine(fadeOut(trail));
    }
    private IEnumerator fadeOut(GameObject trail)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            trail.GetComponent<Renderer>().material.SetFloat("_Power", alpha);
            yield return null;
        }
        trail.GetComponent<Renderer>().material.SetFloat("_Power", 0);
        Destroy(trail);
    }


}
