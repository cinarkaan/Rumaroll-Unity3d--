using System.Collections;
using UnityEngine;

public class ColorfulTile : MonoBehaviour
{
    private ParticleSystem smoke;
    private MaterialPropertyBlock propertyBlock;
    private Renderer Renderer; // It might be null beacuse of the frustum culling operation
    public Vector2Int Position { get; private set; }
    private Coroutine dynamics;

    private void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        Renderer = transform.GetComponent<Renderer>();
        Position = new Vector2Int((int)transform.position.x, (int)transform.position.z);
    }
    public void RepeatColor (Material dummy,Material original)
    {
        dynamics = StartCoroutine(DynamicColor(dummy, original));
    }
    private IEnumerator DynamicColor(Material dummy, Material original)
    {
        while (true)
        {
            propertyBlock.SetColor("_ColorBottom", dummy.GetColor("_ColorBottom").gamma);
            propertyBlock.SetColor("_ColorTop", dummy.GetColor("_ColorTop").gamma);
            Renderer.SetPropertyBlock(propertyBlock);
            ExchangeMaterials(dummy);
            if (smoke != null) smoke.Play();
            yield return new WaitForSeconds(3.5f);
            propertyBlock.SetColor("_ColorBottom", original.GetColor("_ColorBottom").gamma);
            propertyBlock.SetColor("_ColorTop", original.GetColor("_ColorTop").gamma);
            Renderer.SetPropertyBlock(propertyBlock);
            ExchangeMaterials(original);
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
    private void ExchangeMaterials (Material mat)
    {
        // The var attribute where is on the following must be defined as global which is used with polymorphism to avoid gc allocation. The metod takes same args both side.
        if (SceneLoader.CurrentScene == "Day")
        {
            var obj = GetComponentInParent<PlatformManager>();
            obj.SetTileMat(mat, Position);
        } else if (SceneLoader.CurrentScene == "Multiplayer")
        {
            var obj = GetComponentInParent<NetworkPlatformManager>();
            obj.SetTileMat(mat, Position);
        }
        else
            StopCoroutine(dynamics);
    }

}
