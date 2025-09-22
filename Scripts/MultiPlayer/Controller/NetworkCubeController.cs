using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkCubeController : NetworkBehaviour
{
    public float rollDuration = 0.6f;

    private bool isRolling = false; // Whether is rolling or do not
    private bool moving = true; // Whether is moving or do not

    public Transform _gps; // The position of cube on the map

    [SerializeField]
    private Transform[] faceQuads; // The referances that belongs on face of cube

    private NetworkPlatformManager platformManager;

    private readonly CubeSimulator CubeSimulator = new();

    private ParticleSystem Match;

    [SerializeField]
    private MobileTrail cubeTrail;

    [SerializeField]
    private AudioClip[] _move_Sfx;

    [SerializeField]
    private AudioSource _rolling;
    public Vector2Int Target { private get; set; }
    
    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitUntilPlatform());
    }

    private IEnumerator WaitUntilPlatform ()
    {
        yield return new WaitUntil(() => GameObject.Find("PlatformManager") != null);

        platformManager = GameObject.Find("PlatformManager").GetComponent<NetworkPlatformManager>();

        Match = GameObject.Find("Cube_Match").GetComponent<ParticleSystem>();

        _gps = GameObject.Find("Gps").GetComponent<Transform>();

        if (transform.GetComponent<NetworkObject>().OwnerClientId == 0)
        {
            transform.position = new Vector3(6f, 0.815f, 6f);
            transform.gameObject.name = "Host";
        }
        else
        {
            transform.position = new Vector3(platformManager.ServerManager_.Stage.Value + 6, 0.815f, platformManager.ServerManager_.Stage.Value + 6);
            transform.gameObject.name = "Client";
            Target = new Vector2Int(6, 6);
        }

        yield return new WaitUntil(() => platformManager.ServerManager_.Progress);

        GetComponent<OverlapBoxNonAllocPoller>().UIController = (platformManager.ServerManager_.Difficulty.Value > 0 ) ? platformManager.ServerManager_.UIController_ : null;
    }
    public void TryMove(Vector3 dir)
    {
        if (isRolling) return;
        if (dir != Vector3.zero)
        {
            Vector3 targetPosCandidate = transform.position + dir * 1f;
            // Get coordinate of move according to the grid system
            Vector2Int targetCoord = new Vector2Int(
                Mathf.RoundToInt(targetPosCandidate.x / 1f),
                Mathf.RoundToInt(targetPosCandidate.z / 1f)
            );
            // Check out platform boundaries depends on platform manager
            if (targetCoord.x < 6 || targetCoord.x > platformManager.Stage + 6 ||
                targetCoord.y < 6 || targetCoord.y > platformManager.Stage + 6)
                return;
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

        Material tile = platformManager.GetTileMat(GetTileCoordAtPosition());

        if (!mat.name.Equals(tile.name))
        {
            moving = true;
            CubeSimulator.faceIndices = oldIndices;
            StartCoroutine(Move(direction * (-1)));
            yield return new WaitUntil(() => !moving);
        }
        else
        {
            Match.transform.SetPositionAndRotation(new Vector3(transform.position.x, 0.48f, transform.position.z), Quaternion.identity);
            Match.Play();
        }

        isRolling = false;
        cubeTrail.CancelInvoke();
        HasPlayerWon();
    }
    private IEnumerator Move(Vector3 direction)
    {
        _rolling.PlayOneShot(_move_Sfx[0], NetworkUIController._Volume);

        moving = true;
        Vector3 anchor = transform.position + (Vector3.down + direction) * 1f / 2f;
        Vector3 axis = Vector3.Cross(Vector3.up, direction);

        float angle = 0f;
        float speed = 90f / rollDuration;

        cubeTrail.InvokeRepeating("CreateTrail", 0.1f, 0.1f);

        while (angle < 90f)
        {
            float step = speed * Time.deltaTime;
            if (angle + step > 90f) step = 90f - angle;

            transform.RotateAround(anchor, axis, step);
            angle += step;

            yield return null;
        }

        _rolling.PlayOneShot(_move_Sfx[1], NetworkUIController._Volume);
        
        Vector3 finalPos = transform.position;
        finalPos.x = Mathf.Round(finalPos.x / 1f) * 1f;
        finalPos.y = 0.815f;
        finalPos.z = Mathf.Round(finalPos.z / 1f) * 1f;
        transform.position = finalPos;

        _gps.position = transform.position;

        moving = false;
    }
    private Vector2Int GetTileCoordAtPosition()
    {
        int x = Mathf.RoundToInt(transform.position.x / 1f);
        int z = Mathf.RoundToInt(transform.position.z / 1f);
        return new Vector2Int(x, z);
    }
    private void HasPlayerWon ()
    {
        if (Target.Equals(GetTileCoordAtPosition())) 
        {
            isRolling = true;
            platformManager.Confetie_.transform.position = transform.position;
            platformManager.ServerManager_.NoticationWonPlayerServerRpc(transform.gameObject.name);
            var manager = GameObject.Find("UIManager").GetComponent<NetworkUIController>();
            platformManager.Confetie_.Play();
            manager.GetScore(platformManager.Confetie_);
        }
    }
    public void Render(bool render)
    {
        int index = 0;
        while (index < 6)
            transform.GetChild(index++).GetComponent<MeshRenderer>().enabled = render;
        transform.GetComponent<MeshRenderer>().enabled = render;
    }
    public void Origin ()
    {
        GetComponent<OverlapBoxNonAllocPoller>().GameOver = false;
        transform.rotation = Quaternion.identity;
        CubeSimulator.Reset();
    }
}

