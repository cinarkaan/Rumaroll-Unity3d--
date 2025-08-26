using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FlagWaver : MonoBehaviour
{
    [Header("Wave Parameters")]
    [Tooltip("Dalga amplitude (Height)")]
    public float Amplitude = 0.5f;
    [Tooltip("Wave Lenght")]
    public float Wavelength = 2f;
    [Tooltip("Wave Speed")]
    public float Speed = 1f;

    private MeshFilter MeshFilter;
    private Mesh Mesh;
    private Vector3[] OriginalVerts;
    private Vector3[] DisplacedVerts;

    void Start()
    {
        MeshFilter = GetComponent<MeshFilter>();
        Mesh = MeshFilter.mesh;
        // For the performance , the mesh is signed as dynamic
        Mesh.MarkDynamic();

        OriginalVerts = Mesh.vertices;
        DisplacedVerts = new Vector3[OriginalVerts.Length];
        OriginalVerts.CopyTo(DisplacedVerts, 0);
    }
    void Update()
    {
        // Wave offset by the time
        float time = Time.time * Speed;

        for (int i = 0; i < DisplacedVerts.Length; i++)
        {
            Vector3 orig = OriginalVerts[i];
            // Wave function: sin( (x / λ) + t ) * A
            float wave = Mathf.Sin((orig.x / Wavelength) + time) * Amplitude;
            DisplacedVerts[i] = new Vector3(orig.x, orig.y + wave, orig.z);
        }

        Mesh.vertices = DisplacedVerts;
        Mesh.RecalculateNormals();  // To be proper the lighting so it works as seamless
    }
}
