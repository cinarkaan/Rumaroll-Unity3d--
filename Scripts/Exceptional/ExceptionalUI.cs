using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExceptionalUI : MonoBehaviour
{
    [SerializeField]
    protected float SwipeThreshold = 70f;
    [SerializeField]
    protected float FadeDuration = 0.5f;

    [SerializeField]
    protected AudioClip[] AudioClips;
    [SerializeField]
    protected List<Button> Buttons = new();
    [SerializeField]
    protected List<Image> Images = new();
    [SerializeField]
    protected List<Text> texts = new();

    [SerializeField]
    protected RawImage Map;
    [SerializeField]
    protected AudioSource AudioSource;
    [SerializeField]
    protected AspectController Aspect;
    [SerializeField]
    protected ExceptionalPlatform Platform;

    public RawImage RawImage => Map;

    protected bool IsRotating = false, IsClicked = false;

    protected float TimeSinceLastUpdate = 0;
    protected float MinTime, AverageTime, MaxTime;
    protected float PassedTime = 0.0f;

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
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(interactable);
        }
    }
    protected void InitializeScoreTimes()
    {
        int Stage = Platform.Stage_;

        if (Stage >= 4 && Stage <= 7)
        {
            MinTime = 4f * 60f;
            AverageTime = 5.5f * 60f;
            MaxTime = 7f * 60f;
        }
        else if (Stage >= 7 && Stage <= 9)
        {
            MinTime = 6.5f * 60f;
            AverageTime = 7.5f * 60f;
            MaxTime = 8.5f * 60f;
        }
        else if (Stage >= 10 && Stage <= 12)
        {
            MinTime = 9f * 60f;
            AverageTime = 10f * 60f;
            MaxTime = 12f * 60f;
        }
        else
            return;
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
    public IEnumerator ScalerMenu (Image menu, Vector3 Target)
    {

        float _smoothTime = 0.3f;
        Vector3 velocity = Vector3.zero;

        while (Vector3.Distance(menu.transform.localScale, Target) > 0.01f)
        {
            menu.transform.localScale = Vector3.SmoothDamp
                (menu.transform.localScale,
                Target,
                ref velocity,
                _smoothTime);
            yield return null;
        }

        menu.transform.localScale = Target;

        IsClicked = false;
    }
    protected IEnumerator Score(ParticleSystem confettie)
    {
        yield return new WaitUntil(() => !confettie.isPlaying);

        texts.Last().text = "";

        TypeWriter TypeWriter = new(0.1f, 0.3f);

        string Message;

        int Score = 0, Index = 0;

        float Point;

        if (PassedTime <= MinTime)
            Score = 3;
        else if (PassedTime <= AverageTime)
            Score = 2;
        else if (PassedTime <= MaxTime)
            Score = 1;

        Point = Mathf.CeilToInt((100.0f / 3.0f) * Score);

        Message = "TOTAL SCORE : " + Point;

        var Stars = Images.Last().transform.GetChild(1);

        
        foreach (Transform star in Stars)
            star.GetComponent<Image>().material.SetFloat("_Reveal", 0f);

        yield return StartCoroutine(ScalerMenu(Images.Last(), Vector3.one));

        while (Index < Score)
        {
            float time = 0;
            float Duration = 1f;
            while (time < Duration)
            {
                Stars.GetChild(Index).GetComponent<Image>().material.SetFloat("_Reveal", time);
                time += Time.deltaTime;
                yield return null;
            }
            Index++;
            yield return null;
        }

        StartCoroutine(TypeWriter.PlayTypeWriterFade(Message, 30, Images.Last().transform.GetChild(2).GetComponent<TMP_Text>()));
    }
    public void GetScore (ParticleSystem confettie)
    {
        ButtonsManager(false);
        AudioSource.PlayOneShot(AudioClips.Last(), _Volume);
        TypeWriter typeWriter = new(0.05f, 0.3f);
        StartCoroutine(typeWriter.PlayTypeWriterFade("CONGRATULATIONS , YOU WON !!!", 32, texts.Last()));
        StartCoroutine(Score(confettie));
    }
}

