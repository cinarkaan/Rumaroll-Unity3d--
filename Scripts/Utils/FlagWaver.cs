using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FlagWaver : MonoBehaviour
{
    [Header("Wave Parameters")]
    [Tooltip("Dalga amplitude (Height)")]
    public float amplitude = 0.5f;
    [Tooltip("Wave Lenght")]
    public float wavelength = 2f;
    [Tooltip("Wave Speed")]
    public float speed = 1f;

    private MeshFilter mf;
    private Mesh mesh;
    private Vector3[] originalVerts;
    private Vector3[] displacedVerts;

    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mesh = mf.mesh;
        // For the performance , the mesh is signed as dynamic
        mesh.MarkDynamic();

        originalVerts = mesh.vertices;
        displacedVerts = new Vector3[originalVerts.Length];
        originalVerts.CopyTo(displacedVerts, 0);
    }
    void Update()
    {
        // Wave offset by the time
        float time = Time.time * speed;

        for (int i = 0; i < displacedVerts.Length; i++)
        {
            Vector3 orig = originalVerts[i];
            // Wave function: sin( (x / λ) + t ) * A
            float wave = Mathf.Sin((orig.x / wavelength) + time) * amplitude;
            displacedVerts[i] = new Vector3(orig.x, orig.y + wave, orig.z);
        }

        mesh.vertices = displacedVerts;
        mesh.RecalculateNormals();  // To be proper the lighting so it works as seamless
    }
}
