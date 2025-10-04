using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TMPTool : MonoBehaviour
{
    private readonly float Duration = 1f; 
    
    private TextMeshProUGUI[] TMP_TEXTS;

    private TMP_Text Header;

    private Mesh Mesh;

    private float SpawnInterval = 0.1f;
    private float FadeDurationText = 0.3f;

    private Vector3[][] OriginalVertices;
    private void CacheOriginalVertices()
    {
        OriginalVertices = new Vector3[TMP_TEXTS.Length][];
        for (int i = 0; i < TMP_TEXTS.Length; i++)
        {
            TMP_TEXTS[i].ForceMeshUpdate();
            OriginalVertices[i] = TMP_TEXTS[i].mesh.vertices.Clone() as Vector3[];
        }
    }
    public TMPTool (TextMeshProUGUI[] TMP_TEXTS, float IntervalChars, float FadeDurationChars)
    {
        this.TMP_TEXTS = TMP_TEXTS;
        SpawnInterval = IntervalChars;
        FadeDurationText = FadeDurationChars;
    }
    public TMPTool()
    {

    }
    public IEnumerator AnimateTextsOutAllMenu()
    {
        CacheOriginalVertices();
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            float t = elapsed / Duration;
            float scale = Mathf.Lerp(1f, 0f, t); // Shrink
            float angle = Mathf.Lerp(0f, -360f, t); // Rotate
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            for (int i = 0; i < TMP_TEXTS.Length; i++)
            {
                TMP_TEXTS[i].ForceMeshUpdate();
                Mesh = TMP_TEXTS[i].mesh;
                Vector3[] vertices = Mesh.vertices;
                var Info = TMP_TEXTS[i].textInfo;
                for (int j = 0; j < Info.characterCount; j++)
                {

                    var charInfo = Info.characterInfo[j];
                    if (!charInfo.isVisible) continue; // Pass the space charachters

                    int vi = charInfo.vertexIndex;

                    // Pivot (Middle Of The Charachter)
                    Vector3 pivot = (vertices[vi] + vertices[vi + 1] +
                                     vertices[vi + 2] + vertices[vi + 3]) / 4f;

                    // Write seperate by seperate the progress for the each vertex
                    vertices[vi] = pivot + rot * ((vertices[vi] - pivot) * scale);
                    vertices[vi + 1] = pivot + rot * ((vertices[vi + 1] - pivot) * scale);
                    vertices[vi + 2] = pivot + rot * ((vertices[vi + 2] - pivot) * scale);
                    vertices[vi + 3] = pivot + rot * ((vertices[vi + 3] - pivot) * scale);
                }
                Mesh.vertices = vertices;
                TMP_TEXTS[i].canvasRenderer.SetMesh(Mesh);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        ResetAllCharacters();
    }
    public void WaveHeader ()
    {
        if (Header == null) return;
        var textInfo = Header.textInfo;

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            Vector3[] vertices = textInfo.meshInfo[i].vertices;

            for (int c = 0; c < textInfo.characterCount; c++)
            {
                if (!textInfo.characterInfo[c].isVisible) continue;
                if (textInfo.characterInfo[c].materialReferenceIndex != i) continue;

                int vertexIndex = textInfo.characterInfo[c].vertexIndex;

                // it waves by both time and charachter index 
                float wave = Mathf.Sin(Time.time * 2f + c * 0.2f) * 0.2f;

                Vector3 offset = new(0, wave, 0);

                vertices[vertexIndex + 0] += offset;
                vertices[vertexIndex + 1] += offset;
                vertices[vertexIndex + 2] += offset;
                vertices[vertexIndex + 3] += offset;
            }

            textInfo.meshInfo[i].mesh.vertices = vertices;
            Header.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
        }
    }
    private void ResetAllCharacters()
    {
        for (int i = 0; i < TMP_TEXTS.Length; i++)
        {
            TMP_TEXTS[i].ForceMeshUpdate();
            Mesh = TMP_TEXTS[i].mesh;
            Mesh.vertices = OriginalVertices[i];
            TMP_TEXTS[i].canvasRenderer.SetMesh(Mesh);
        }
    }
    public void SetHeader (TMP_Text Header)
    {
        this.Header = Header;
    }
    public IEnumerator PlayTypeWriterFade(string Text, float TextSize, TMP_Text TMP_Text)
    {
        int n = Text.Length;

        // Total time : Start of the last word + fadeDuration
        float totalDuration = SpawnInterval * (n - 1) + FadeDurationText;
        float time = 0f;

        // At the each frame the text regenerate by using only one stringbuilder
        StringBuilder builder = new();

        TMP_Text.fontSize = TextSize;

        while (time < totalDuration)
        {
            builder.Length = 0;

            for (int i = 0; i < n; i++)
            {
                char c = Text[i];
                float charStart = i * SpawnInterval;
                float t = (time - charStart) / FadeDurationText;
                t = Mathf.Clamp01(t);

                // Add either space or new line
                if (c == ' ' || c == '\n')
                {
                    builder.Append(c);
                    continue;
                }

                if (t <= 0f)
                {
                    // Did it start ? alfa = 00 (Full Transparent)
                    builder.Append("<color=#00000000>");
                    builder.Append(c);
                    builder.Append("</color>");
                }
                else if (t >= 1f)
                {
                    // Fade was complated ? Full black (without tag, opaque)
                    builder.Append(c);
                }
                else
                {
                    // 0 < t < 1 ? alfa = between 0?255 
                    byte alphaByte = (byte)Mathf.RoundToInt(Mathf.Lerp(0, 255, t));
                    string hex = alphaByte.ToString("X2");  // "00" ... "FF"
                    builder.Append("<color=#000000");
                    builder.Append(hex);
                    builder.Append(">");
                    builder.Append(c);
                    builder.Append("</color>");
                }
            }

            TMP_Text.text = builder.ToString();

            time += Time.deltaTime;
            yield return null;
        }

        TMP_Text.text = Text;
    }
    public IEnumerator PlayTypeWriterFade(string Text, int TextSize, Text text)
    {
        int n = Text.Length;

        // Total time : Start of the last word + fadeDuration
        float totalDuration = SpawnInterval * (n - 1) + FadeDurationText;
        float time = 0f;

        // At the each frame the text regenerate by using only one stringbuilder
        StringBuilder builder = new();

        text.fontSize = TextSize;

        while (time < totalDuration)
        {
            builder.Length = 0;

            for (int i = 0; i < n; i++)
            {
                char c = Text[i];
                float charStart = i * SpawnInterval;
                float t = (time - charStart) / FadeDurationText;
                t = Mathf.Clamp01(t);

                // Add either space or new line
                if (c == ' ' || c == '\n')
                {
                    builder.Append(c);
                    continue;
                }

                if (t <= 0f)
                {
                    // Did it start ? alfa = 00 (Full Transparent)
                    builder.Append("<color=#00000000>");
                    builder.Append(c);
                    builder.Append("</color>");
                }
                else if (t >= 1f)
                {
                    // Fade was complated ? Full black (without tag, opaque)
                    builder.Append(c);
                }
                else
                {
                    // 0 < t < 1 ? alfa = between 0?255 
                    byte alphaByte = (byte)Mathf.RoundToInt(Mathf.Lerp(0, 255, t));
                    string hex = alphaByte.ToString("X2");  // "00" ... "FF"
                    builder.Append("<color=#000000");
                    builder.Append(hex);
                    builder.Append(">");
                    builder.Append(c);
                    builder.Append("</color>");
                }
            }

            text.text = builder.ToString();

            time += Time.deltaTime;
            yield return null;
        }

        text.text = Text;
    }
    public IEnumerator FadeText(TMP_Text Text)
    {
        float t = 0f;
        while (true)
        {
            float alpha = Mathf.PingPong(t * 1f, 1f);
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }
    }

}
