using UnityEngine;
using System.Collections;

public class RollingCubeController : MonoBehaviour
{

    [SerializeField]
    private PlatformManager platformManager;

    [SerializeField] 
    private Transform[] faceQuads; // The referaces that belongs on faces of cube

    [SerializeField]
    private UIController control;

    [SerializeField]
    private AudioClip[] _move_Sfx;

    [SerializeField]
    private AudioSource _rolling;

    private CubeSimulator CubeSimulator = new();
    public ParticleSystem shield { get; private set; }

    private Vector3 _shieldVelocity = Vector3.zero;

    private MobileTrail cubeTrail;

    private bool isRolling = false;
    private bool moving = true;

    public float rollDuration = 0.6f;
    private void Start()
    {
        shield = transform.GetChild(7).GetComponent<ParticleSystem>();
        cubeTrail = transform.GetComponent<MobileTrail>();
    }
    public void TryMove(Vector3 dir)
    {
        if (isRolling) return;
        if (dir != Vector3.zero)
        {
            Vector3 targetPosCandidate = transform.position + dir * 1f;
            // Calculate the grid coordinates
            Vector2Int targetCoord = new Vector2Int(
                Mathf.RoundToInt(targetPosCandidate.x / 1f),
                Mathf.RoundToInt(targetPosCandidate.z / 1f)
            );
            // Check out the grid boundaries (Depends on the stage)
            if (targetCoord.x < 6 || targetCoord.x > platformManager.stage + 6 ||
                targetCoord.y < 6 || targetCoord.y > platformManager.stage + 6)
                return; 

            // if the target position contains grid boundaries , launch rolling operation
            StartCoroutine(Roll(dir));

        }
    }
    private IEnumerator Roll(Vector3 direction)
    {
        isRolling = true;

        int[] oldIndices = CubeSimulator.faceIndices;

        CubeSimulator.Roll(direction);

        StartCoroutine(Move(direction));

        yield return new WaitUntil(() => !moving);

        Material mat = faceQuads[CubeSimulator.faceIndices[0]].GetComponent<Renderer>().sharedMaterial;

        Material tile = platformManager.GetTileMatAtPosition(GetTileCoordAtPosition());

        if (!mat.name.Equals(tile.name))
        {
            moving = true;
            CubeSimulator.faceIndices = oldIndices;
            StartCoroutine(Move(direction * (-1)));
            yield return new WaitUntil(() => !moving);
        }

        cubeTrail.CancelInvoke();
        isRolling = false;
        control.UpdateGps(transform.position);
        HasPlayerArrive();
        
    }
    private IEnumerator Move(Vector3 direction)
    {

        _rolling.PlayOneShot(_move_Sfx[0], control._volume);

        moving = true;
        Vector3 anchor = transform.position + (Vector3.down + direction) * 1f / 2f;
        Vector3 axis = Vector3.Cross(Vector3.up, direction);

        float angle = 0f;
        float speed = 90f / rollDuration;

        cubeTrail.InvokeRepeating("Trail", 0.1f, 0.1f);
        
        while (angle < 90f)
        {
            float step = speed * Time.deltaTime;
            if (angle + step > 90f) step = 90f - angle;

            transform.RotateAround(anchor, axis, step);
            angle += step;

            yield return null;
        }
        _rolling.PlayOneShot(_move_Sfx[1], control._volume);

        Vector3 finalPos = transform.position;

        finalPos.x = Mathf.Round(finalPos.x / 1f) * 1f;
        finalPos.y = 0.99f;
        finalPos.z = Mathf.Round(finalPos.z / 1f) * 1f;
        transform.position = finalPos;

        moving = false;

    }
    public Vector2Int GetTileCoordAtPosition()
    {
    	int x = Mathf.RoundToInt(transform.position.x / 1f);
    	int z = Mathf.RoundToInt(transform.position.z / 1f);
    	return new Vector2Int(x, z);
    }
    public void Render (bool render)
    {
        int index = 0;
        while (index < 6)
            transform.GetChild(index++).GetComponent<MeshRenderer>().enabled = render;
        transform.GetComponent<MeshRenderer>().enabled = render;
    }
    public PlatformManager getGridManager ()
    {
        return platformManager;
    }
    private void HasPlayerArrive ()
    {
        if (GetTileCoordAtPosition().Equals(new Vector2Int(platformManager.stage + 6, platformManager.stage + 6)))
            StartCoroutine(control.sceneLoader(0, 1, 0.15f, "Day"));
    }
    public IEnumerator ShieldController (bool isActive)
    {
        float _smoothTime = 0.3f;
        Vector3 target = new Vector3(3.3f, 3.3f, 3.3f);
        transform.GetChild(6).localScale = Vector3.zero;
        transform.GetChild(6).gameObject.SetActive(isActive);

        while (Vector3.Distance(transform.localScale , target) > 0.01f && isActive)
        {
            transform.GetChild(6).localScale = Vector3.SmoothDamp
                (transform.GetChild(6).localScale, 
                target, 
                ref _shieldVelocity , 
                _smoothTime);
            yield return null;
        }
    }

}

