using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetworkCubeController : NetworkBehaviour
{

    public float rollDuration = 0.6f;

    private bool isRolling = false;
    private bool moving = true;

    public Transform _gps;

    [SerializeField]
    private Transform[] faceQuads; // The referances that belongs on face of cube

    private NetworkPlatformManager platformManager;

    private CubeSimulator CubeSimulator = new();

    private MobileTrail cubeTrail;

    [SerializeField]
    private AudioClip[] _move_Sfx;

    [SerializeField]
    private AudioSource _rolling;

    public Vector2Int target { private get; set; }
    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitUntilPlatform());
    }
    private IEnumerator WaitUntilPlatform ()
    {
        yield return new WaitUntil(() => GameObject.Find("PlatformManager") != null);

        platformManager = GameObject.Find("PlatformManager").GetComponent<NetworkPlatformManager>();

        _gps = GameObject.Find("Gps").GetComponent<Transform>();

        if (transform.GetComponent<NetworkObject>().OwnerClientId == 0)
        {
            transform.position = new Vector3(6f, 0.99f, 6f);
            transform.gameObject.name = "Host";
        }
        else
        {
            transform.position = new Vector3(platformManager._ServerManager.Stage.Value + 6, 0.99f, platformManager._ServerManager.Stage.Value + 6);
            transform.gameObject.name = "Client";
            target = new Vector2Int(6, 6);
        }

        cubeTrail = GetComponent<MobileTrail>();

        yield return new WaitUntil(() => platformManager._ServerManager.Progress);

        GetComponent<OverlapBoxNonAllocPoller>().UIController = (platformManager._ServerManager.Difficulty.Value > 0 ) ? platformManager._ServerManager._UIController : null;
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
            if (targetCoord.x < 6 || targetCoord.x > platformManager._ServerManager.Stage.Value+ 6 ||
                targetCoord.y < 6 || targetCoord.y > platformManager._ServerManager.Stage.Value + 6)
                return;
            StartCoroutine(Roll(dir));
        }
    }
    private IEnumerator Roll(Vector3 direction)
    {
        isRolling = true;

        int[] oldIndices = CubeSimulator.faceIndices;
        
        CubeSimulator.Roll(direction);

        StartCoroutine(move(direction));

        yield return new WaitUntil(() => !moving);

        Material mat = faceQuads[CubeSimulator.faceIndices[0]].GetComponent<Renderer>().sharedMaterial;

        Material tile = platformManager.GetTileMat(GetTileCoordAtPosition());

        if (!mat.name.Equals(tile.name))
        {
            moving = true;
            CubeSimulator.faceIndices = oldIndices;
            StartCoroutine(move(direction * (-1)));
            yield return new WaitUntil(() => !moving);
        }

        isRolling = false;
        cubeTrail.CancelInvoke();
        HasPlayerWon();
    }
    private IEnumerator move(Vector3 direction)
    {
        _rolling.PlayOneShot(_move_Sfx[0], NetworkUIController._Volume);

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

        _rolling.PlayOneShot(_move_Sfx[1], NetworkUIController._Volume);
        
        Vector3 finalPos = transform.position;
        finalPos.x = Mathf.Round(finalPos.x / 1f) * 1f;
        finalPos.y = 0.99f;
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
        if (target.Equals(GetTileCoordAtPosition()))
        {
            isRolling = true;
            platformManager._ServerManager.NoticationWonPlayerServerRpc(transform.gameObject.name);
            var manager = GameObject.Find("UIManager").GetComponent<NetworkUIController>();
            manager.DistributeRewards();
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

