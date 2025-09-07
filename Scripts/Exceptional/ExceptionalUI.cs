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
    protected AudioClip[] AudioClips;
    [SerializeField]
    protected AudioSource AudioSource;
    [SerializeField]
    protected List<Button> Buttons = new List<Button>();
    [SerializeField]
    protected List<Image> Images = new List<Image>();
    [SerializeField]
    protected List<RawImage> RawImages = new List<RawImage>();
    [SerializeField]
    protected List<Text> texts = new List<Text>();

    public RawImage RawImage => RawImages.Find(r => r.gameObject.name == "GameMap");

    protected bool IsRotating = false;

    protected float TimeSinceLastUpdate = 0;

    public static float _Volume = 0f;

    public bool _GameOver = false;

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
        Buttons.ForEach(b => b.gameObject.SetActive(interactable));
    }
    protected virtual IEnumerator MapFade(Color targetColor){ yield return null; }
    public IEnumerator ScalerMenu(Vector3 from, Vector3 to, float time, Image image)
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

