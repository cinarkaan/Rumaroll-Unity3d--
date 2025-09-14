using System.Collections;
using UnityEngine;

public class MobileTrail : MonoBehaviour
{
    public float Frequency;
    public GameObject Frame;

    public void CreateTrail()
    {
        GameObject trail = Instantiate(Frame, transform.position, transform.rotation);
        trail.transform.SetPositionAndRotation(transform.position, transform.rotation);
        trail.transform.localScale = transform.localScale;
        StartCoroutine(Ghost(trail));
    }
    private IEnumerator Ghost(GameObject trail)
    {
        float elapsed = 0f;
        while (elapsed < Frequency)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Frequency);
            float alpha = Mathf.Lerp(1f, 0f, t);
            trail.GetComponent<Renderer>().material.SetFloat("_Power", alpha);
            yield return null;
        }
        //trail.GetComponent<Renderer>().material.SetFloat("_Power", 0);
        Destroy(trail);
    }


}
