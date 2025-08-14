using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExceptionalUI : MonoBehaviour
{
    [SerializeField]
    protected float SwipeThreshold = 70f;
    [SerializeField]
    protected float FadeDuration = 0.5f;

    [SerializeField]
    protected AspectController Aspect;
    [SerializeField]
    protected AudioClip[] audioClips;
    [SerializeField]
    protected AudioSource audioSource;
    [SerializeField]
    protected List<Button> buttons = new List<Button>();
    [SerializeField]
    protected List<Image> images = new List<Image>();
    [SerializeField]
    protected List<RawImage> rawImages = new List<RawImage>();
    public RawImage RawImage => rawImages.Find(r => r.gameObject.name == "GameMap");

    protected bool IsRotating = false;

    protected float TimeSinceLastUpdate = 0;

    public static float _volume = 0f;

    public bool gameOver = false;

    public virtual void Forward() { }
    public virtual void Backward() { }
    public virtual void Right() { }
    public virtual void Left() { }
    public virtual void Restart() { }
    public virtual void GameOver(int SoundIndex, string name){}
    public virtual void OpenMap() { }
    public virtual void CloseMap() { }
    public virtual void Menu() { }
    public virtual void Pause() { }
    public virtual void Continue() { }
    protected virtual void InitializeUserPrefs() { }

    public void ButtonsManager(bool interactable)
    {
        buttons.ForEach(b => b.gameObject.SetActive(interactable));
    }
    protected IEnumerator MapFade(Color targetColor, Action action)
    {
        Color startColor = rawImages[0].color;
        float time = 0f;

        while (time < FadeDuration)
        {
            time += Time.deltaTime;
            rawImages[0].color = Color.Lerp(startColor, targetColor, Mathf.Clamp01(time / FadeDuration));
            yield return null;
        }

        rawImages[0].color = targetColor;
    }
    public IEnumerator scalerMenu(Vector3 from, Vector3 to, float time, Image image)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / time);
            image.GetComponent<RectTransform>().localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }

        image.GetComponent<RectTransform>().localScale = to;
    }


}

