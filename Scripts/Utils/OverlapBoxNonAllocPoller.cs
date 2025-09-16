using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class OverlapBoxNonAllocPoller : MonoBehaviour
{

    [Header("The managers that will conduct what will be happened after collide")]
    public ExceptionalUI UIController;
    
    [SerializeField]
    private EventManager EventManager;

    [Header("The objects during the destroy moment")]
    [SerializeField]
    private ParticleSystem Sparks;

    [Header("The object is going to play disintegrate effect with rigidbody system")]
    [SerializeField]
    private GameObject DestroyedCube;

    [Header("Box volume settings")]
    [Tooltip("Half-extents of the box. For example, (1, 1, 1) means a 2×2×2 volume in world space.")]
    public Vector3 HalfExtents = new(1f, 1f, 1f);

    [Tooltip("The center offset of the box relative to the local pivot. For example, (0, 1, 0) means look 1 unit above the pivot.")]
    public Vector3 CenterOffset = Vector3.zero;

    [Header("Range of control")]
    [Tooltip("How often should it perform overlap checks? For mobile devices, a range of 0.1–0.2 is appropriate.")]
    [Range(0.01f, 1f)]
    public float CheckInterval = 0.1f;

    [Header("Target Layers")]
    [Tooltip("Which Layers' colliders do you want to detect?")]
    public LayerMask TargetLayers;

    public bool ShieldIsActive = false;
    
    // Timer count to use in update
    private float _Timer = 0f;

    private int SoundIndex = 0;

    // OverlapBoxNonAlloc result array (it had been allocated before)
    // How many colliders detect at the same time , it increases as that as.
    private Collider[] _Results = new Collider[8];

    public bool GameOver = false;

    private void Update()
    {
        if (!GameOver)
        {
            _Timer += Time.deltaTime;
            if (_Timer < CheckInterval) return;
            _Timer = 0f;

            // 1) Calculate the volume on the world
            Vector3 worldCenter = transform.TransformPoint(CenterOffset);
            Quaternion worldRot = transform.rotation;

            // 2) Inquires with overlapBoxNonAlloc 
            //    - worldCenter: Word coordinate the center of box
            //    - halfExtents: Half volume of the box (X, Y, Z)
            //    - _results: The array that has been initialized to fill out the results
            //    - worldRot: Rotation of box (transform.rotation)
            //    - targetLayers: Get the layers where is placed at the this box
            //    - QueryTriggerInteraction.Ignore: Ignore trigger colliders
            int hitCount = Physics.OverlapBoxNonAlloc(
                worldCenter,
                HalfExtents,
                _Results,
                worldRot,
                TargetLayers,
                QueryTriggerInteraction.Ignore
            );

            // 3) the colliders detected as hitcount as, process these
            for (int i = 0; i < hitCount; i++)
            {
                GameObject other = _Results[i].gameObject;
                HandleOverlapWith(other);
            }
        }
    }

    /// <summary>
    /// It calls every objects that in the overlasps. 
    /// Write as you wish.
    /// </summary>
    /// <param name="other">The another object that in the box</param>
    void HandleOverlapWith(GameObject other)
    {
        //Debug.Log($"[OverlapBoxNonAllocPoller] '{gameObject.name}' with '{other.name}' overlap has been founded.");

        if (other.layer == 10 || other.layer == 13)
        {
            EventManager.CheckEarned(other);
            return;
        }

        if (!ShieldIsActive)
        {
            SoundIndex = (other.name == "Spkile" || other.name == "Cutter(Clone)") ? 2 : -1;

            if (other.name.Contains("Hazard"))
            {
                SoundIndex = 3;
                Instantiate(Sparks, new Vector3(other.transform.position.x, 1.2f, other.transform.position.z), Quaternion.Euler(-90f, 0f, 0f), other.transform.root);
            }

            if (other.name.Equals("Bullet"))
                Instantiate(DestroyedCube, new Vector3(transform.position.x, 1.5f, transform.position.z), Quaternion.Euler(Vector3.zero), null);

            UIController.GameOver(SoundIndex, transform.root.gameObject.name);
            GameOver = true;
        }
        // Add your own game logic:
        // • other.SetActive(false);
        // • add score
        // • play effects
        // • e.g.
    }

    /*
    private void OnDrawGizmos()
    {
        // Kutunun dünya uzayýndaki merkezini hesapla
        Vector3 worldCenter = transform.TransformPoint(CenterOffset);

        // Kutunun dönüþünü al
        Quaternion worldRot = transform.rotation;

        // Çizim için matrix ayarla
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // Kýrmýzý, yarý saydam
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(worldCenter, worldRot, Vector3.one);
        Gizmos.matrix = rotationMatrix;

        // Box çiz (Gizmos.DrawWireCube çerçeve, DrawCube dolu þekil çizer)
        Gizmos.DrawWireCube(Vector3.zero, HalfExtents * 2);
    }
    */

}
