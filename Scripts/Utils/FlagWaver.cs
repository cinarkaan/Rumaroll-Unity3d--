using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class FlagWaver : MonoBehaviour
{
    [Header("Wave Parameters")]

    [Tooltip("Dalga amplitude (Height)")]
    [SerializeField]
    private float Amplitude = 0.5f;
    [Tooltip("Wave Lenght")]
    [SerializeField]
    private float Wavelength = 2f;
    [Tooltip("Wave Speed")]
    [SerializeField]
    private float Speed = 1f;
    
    private MeshFilter MeshFilter;
    private Mesh Mesh;
    private Vector3[] OriginalVerts, DisplacedVerts;
    private float MixX, MaxX;
    
    private void Start()
    {
        MeshFilter = GetComponent<MeshFilter>();
        Mesh = MeshFilter.mesh;

        // For the performance , the mesh is signed as dynamic
        Mesh.MarkDynamic();

        OriginalVerts = Mesh.vertices;
        DisplacedVerts = new Vector3[OriginalVerts.Length];
        OriginalVerts.CopyTo(DisplacedVerts, 0);

        MixX = OriginalVerts.Min(v => v.x);
        MaxX = OriginalVerts.Max(v => v.x);
    }
    
    private void Update()
    {
        float time = Time.time * Speed;

        Parallel.For(0, DisplacedVerts.Length, index =>
        {
            Vector3 orig = OriginalVerts[index];

            // Normal wave
            float wave = Mathf.Sin((orig.x / Wavelength) + time) * Amplitude;

            // Decrease the wave effect according to the x coordinate.
            // The wave effect must be nearly zero at the nearest of the pole.
            float factor = Mathf.InverseLerp(MixX, MaxX, orig.x);

            // Update the vertexes
            DisplacedVerts[index] = new Vector3(orig.x, orig.y + wave * factor, orig.z);
        });

        Mesh.SetVertices(DisplacedVerts);
        Mesh.RecalculateNormals();
    }

}
