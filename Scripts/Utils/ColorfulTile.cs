using System.Collections;
using UnityEngine;

public class ColorfulTile : MonoBehaviour
{
    private ExceptionalPlatform ExceptionalPlatform;
    private ParticleSystem smoke;
    private MaterialPropertyBlock DummyProperty, OriginalProperty;
    private Renderer Renderer;
    public Vector2Int Position { get; private set; }

    private void Awake()
    {
        DummyProperty = new MaterialPropertyBlock();
        OriginalProperty = new MaterialPropertyBlock();
        Renderer = transform.GetComponent<Renderer>();
        Position = new Vector2Int((int)transform.position.x, (int)transform.position.z);
    }

    private void Start()
    {
        ExceptionalPlatform = SceneLoader.CurrentScene == "Day" ? GetComponentInParent<PlatformManager>() : GetComponentInParent<NetworkPlatformManager>();
    }

    public void RepeatColor (Material dummy,Material original)
    {
        DummyProperty.SetColor("_ColorBottom", dummy.GetColor("_ColorBottom").gamma);
        DummyProperty.SetColor("_ColorTop", dummy.GetColor("_ColorTop").gamma);
        OriginalProperty.SetColor("_ColorBottom", original.GetColor("_ColorBottom").gamma);
        OriginalProperty.SetColor("_ColorTop", original.GetColor("_ColorTop").gamma);
        StartCoroutine(DynamicColor(dummy, original));
    }
    private IEnumerator DynamicColor(Material dummy, Material original)
    {
        while (true)
        {
            Renderer.SetPropertyBlock(DummyProperty);
            ExceptionalPlatform.SetTileMat(dummy, Position);
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(3.5f);
            Renderer.SetPropertyBlock(OriginalProperty);
            ExceptionalPlatform.SetTileMat(original, Position);
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(3f);
        }
    }
    public void AddSmokeVfx (MaterialProperties materialProperties, ParticleSystem Smoke_Burst)
    {
        smoke = Instantiate(Smoke_Burst, new Vector3(transform.position.x, 1f, transform.position.z), Quaternion.identity, transform);
        smoke.transform.GetChild(0).GetComponent<ParticleSystem>().collision.SetPlane(0, transform.root);
        smoke.transform.GetChild(0).GetComponent<ParticleSystem>().collision.SetPlane(1, transform);
        AdjustColor(materialProperties);
    }
    private void AdjustColor (MaterialProperties materialProperties)
    {
        Gradient smokeGradient = new Gradient();
        Gradient scatteredGradient = new Gradient();
        

        var colorModuleSmoke = smoke.colorOverLifetime;
        var colorModuleScattered = smoke.transform.GetChild(0).GetComponent<ParticleSystem>().colorOverLifetime;

        GradientAlphaKey[] _alphaKeys = new GradientAlphaKey[5];

        _alphaKeys[0] = new GradientAlphaKey(0f,0f);
        _alphaKeys[1] = new GradientAlphaKey(244f/255f, 0.1f);
        _alphaKeys[2] = new GradientAlphaKey(150f / 255f, 0.35f);
        _alphaKeys[3] = new GradientAlphaKey(100f / 255f, 0.66f);
        _alphaKeys[4] = new GradientAlphaKey(50f / 255f, 1f);

        smokeGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(materialProperties._bottom,0f),
            new GradientColorKey(materialProperties._top , 1f)
        };

        smokeGradient.alphaKeys = _alphaKeys;

        scatteredGradient.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(materialProperties._bottom, 0f),
            new GradientColorKey(materialProperties._top, 1f)
        };

        scatteredGradient.alphaKeys = _alphaKeys;

        colorModuleSmoke.color = new ParticleSystem.MinMaxGradient(smokeGradient);
        colorModuleScattered.color = new ParticleSystem.MinMaxGradient(scatteredGradient);
    }
}
