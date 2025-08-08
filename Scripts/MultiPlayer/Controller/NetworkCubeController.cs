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

    public int[] faceIndices = { 0, 1, 2, 3, 4, 5 }; // Faces ; Bottom, Top, Front, Back, Left, Right

    private NetworkPlatformManager platformManager;

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
            transform.position = new Vector3(platformManager.getManager()._stage.Value + 6, 0.99f, platformManager.getManager()._stage.Value + 6);
            transform.gameObject.name = "Client";
            target = new Vector2Int(6, 6);
        }
        cubeTrail = GetComponent<MobileTrail>();
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
            if (targetCoord.x < 6 || targetCoord.x > platformManager.getManager()._stage.Value+ 6 ||
                targetCoord.y < 6 || targetCoord.y > platformManager.getManager()._stage.Value + 6)
                return;

            StartCoroutine(Roll(dir));

        }
    }
    private IEnumerator Roll(Vector3 direction)
    {
        isRolling = true;

        int[] oldIndices = (int[])faceIndices.Clone();

        RollFaceIndices(direction);

        StartCoroutine(move(direction));

        yield return new WaitUntil(() => !moving);

        Material mat = faceQuads[faceIndices[0]].GetComponent<Renderer>().sharedMaterial;

        Material tile = platformManager.FindTileMat(GetTileCoordAtPosition());

        if (!mat.name.Equals(tile.name))
        {
            moving = true;
            faceIndices = oldIndices;
            StartCoroutine(move(direction * (-1)));
            yield return new WaitUntil(() => !moving);
        }

        isRolling = false;
        cubeTrail.CancelInvoke();
        HasPlayerWon();
    }
    private void RollFaceIndices(Vector3 direction)
    {
        int[] old = (int[])faceIndices.Clone();

        // Varsayılan sıralama: 0=Bottom, 1=Top, 2=Front, 3=Back, 4=Left, 5=Right

        if (direction == Vector3.forward) // Z+
        {
            faceIndices[0] = old[2]; // Bottom = Front
            faceIndices[1] = old[3]; // Top = Back
            faceIndices[2] = old[1]; // Front = Top
            faceIndices[3] = old[0]; // Back = Bottom
                                     // Left, Right sabit
            faceIndices[4] = old[4];
            faceIndices[5] = old[5];
        }
        else if (direction == Vector3.back) // Z-
        {
            faceIndices[0] = old[3]; // Bottom = Back
            faceIndices[1] = old[2]; // Top = Front
            faceIndices[2] = old[0]; // Front = Bottom
            faceIndices[3] = old[1]; // Back = Top
                                     // Left, Right sabit
            faceIndices[4] = old[4];
            faceIndices[5] = old[5];
        }
        else if (direction == Vector3.left) // X-
        {
            faceIndices[0] = old[4]; // Bottom = Left
            faceIndices[1] = old[5]; // Top = Right
            faceIndices[4] = old[1]; // Left = Top
            faceIndices[5] = old[0]; // Right = Bottom
                                     // Front, Back sabit
            faceIndices[2] = old[2];
            faceIndices[3] = old[3];
        }
        else if (direction == Vector3.right) // X+
        {
            faceIndices[0] = old[5]; // Bottom = Right
            faceIndices[1] = old[4]; // Top = Left
            faceIndices[4] = old[0]; // Left = Bottom
            faceIndices[5] = old[1]; // Right = Top
                                     // Front, Back sabit
            faceIndices[2] = old[2];
            faceIndices[3] = old[3];
        }
    }
    private IEnumerator move(Vector3 direction)
    {
        _rolling.PlayOneShot(_move_Sfx[0], NetworkUIController._volume);

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

        _rolling.PlayOneShot(_move_Sfx[1], NetworkUIController._volume);
        
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
            platformManager.getManager().NoticationWonPlayerServerRpc(transform.gameObject.name);
            var manager = GameObject.Find("UIManager").GetComponent<NetworkUIController>();
            manager.DistributeRewards();
        }
    }
}

