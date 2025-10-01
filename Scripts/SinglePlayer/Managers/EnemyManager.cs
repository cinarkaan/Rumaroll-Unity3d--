using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : ExceptionalPath
{
    [Header("Enemies")]
    [SerializeField]
    private GameObject Bulldozer, SailBoat;

    [Header("Linked Managers")]
    [SerializeField]
    private EventManager eventManager;

    [SerializeField]
    private ObstacleManager obstacleManager;

    [SerializeField]
    private UIController controller;
    
    private float ShootInterval = 0, ShootStartTime = 0f;  

    private bool IsShooting = false;

    private readonly List<Transform> Sails = new();
    private readonly List<Transform> Bullet = new();
    private readonly List<ParticleSystem> Explosive = new();

    private AudioSource CannonExplode;

    private void Start()
    {
        if (PlatformManager.Stage_ == 12) // The manager only must be worked at the stage 12 
        {
            Map = new int[13, 13];
            var _sound = Bulldozer.GetComponent<AudioSource>();
            _sound.PlayOneShot(_sound.clip, UIController._Volume);
            StartCoroutine(InitializeManager());
        }
        else if (PlatformManager.Stage_ == 7 || PlatformManager.Stage_ == 8)
        {
            PlaceSailBoat(PlatformManager.Stage_ % 2, PlatformManager.Stage_ % 2);
        }
    }
    protected override IEnumerator InitializeManager ()
    {
        yield return new WaitUntil(() => obstacleManager.Progress_);
        PathFinding();
        StartCoroutine(AdjustRouteDirsEnemy(Bulldozer));
        yield return null;
    }
    protected override void PathFinding ()
    {
        // First evulation of map without obstacles , with events
        for (int i = 0; i < eventManager.Precious.Count; i++)
        {
            Vector2Int coord = new()
            {
                x = Mathf.Abs(6 - ((int)eventManager.Precious[i].transform.position.x)),
                y = Mathf.Abs(18 - ((int)eventManager.Precious[i].transform.position.z))
            }; // Turn into local map system according to the A* algorithm

            Map[coord.x, coord.y] = 1; // Assign to obstacle symbol
        } // Initializing the first obstacles (Cutters)

        // Adding obstacles in to the map
        for (int i = 0; i < obstacleManager.Cutters.Count; i++)
        {
            Vector2Int coord = new Vector2Int()
            {
                x = Mathf.Abs(6 - ((int)obstacleManager.Cutters[i].transform.position.x)),
                y = Mathf.Abs(18 - ((int)obstacleManager.Cutters[i].transform.position.z))
            }; // Turn into local map system according to the A* algorithm

            Map[coord.x, coord.y] = 1; // Assign to obstacle symbol

        }

        //Exception case : The bulldozer might be moved to arrive point , even if there has been loot(coins or diamond) at this point. Other wise the recursive method will be throwed exception
        Map[12, 12] = 0;

        HashSet<Vector2Int> CloseList = new(); // Initialize visited nodes

        // Find a solution path by using A* 
        AStar(new Vector2Int(0, 0), new HashSet<Node>(), ref CloseList, false);

        // Fetch finded Path
        var _path = CloseList.ToArray();

        // Turn into from A* map coordinates to unity 3d Vector3 path coordinates
        for (int i = 0; i < _path.Length; i++)
        {
            Path.Add(new Vector3(StartPos.x + _path[i].x, 1f, StartPos.z - _path[i].y));
            //Debug.Log("Step " + i + " : " + Path.Last());
        }
        
        _pathprogress = true;
    }
    protected override void AStar(Vector2Int location, HashSet<Node> OpenList, ref HashSet<Vector2Int> CloseList, bool resolved)
    {
        if (location == new Vector2Int(12, 12)) // If was arrived target point , return back
        {
            CloseList.Add(location); // Add last target position to path
            //Debug.Log("Path Resolved : " + resolved);
            if (resolved) // The path has discovered new ways on the map so , it must be rearranged
            {
                var temp = ResolvePath(CloseList.ToList()); // Copy resolved path
                CloseList.Clear(); // Clear the closelist that waa added solved ways
                CloseList = temp; // Assing clear ways
            }
            return;
        }

        CloseList.Add(location); // Get the first element in the OpenList(Neighboords that is not visited)

        if (OpenList.Count > 0)
            OpenList.Remove(OpenList.FirstOrDefault(n => n.Coord == location)); // Assing to current node into closelist(Visited nodes)

        Vector2Int right_N = location + Vector2Int.up; // First neighboord 
        Vector2Int down_N = location + Vector2Int.right; // Second neighboord 

        // First check out the boundary then obstacle
        if (right_N.y <= 12 && Map[right_N.x, right_N.y] != 1)
            OpenList.Add(new Node(right_N)); // Calculate total cost and evaluate open list for first neighboord
        

        // First check out the boundary then obstacle
        if (down_N.x <= 12 && Map[down_N.x, down_N.y] != 1)
            OpenList.Add(new Node(down_N)); // Calculate total cost and evaluate open list for second neighboord}
    
        if (CloseList.Count > 1 && (CloseList.Last() - CloseList.ToArray()[CloseList.Count - 2]).magnitude > 1) // If there are available the nodes that skipped, the path have to resolve  
            resolved = true;

        // Recursive the method for the next steps
        AStar(OpenList.First().Coord, OpenList, ref CloseList, resolved);
    }
    protected override HashSet<Vector2Int> ResolvePath(List<Vector2Int> CloseList)
    {
        List<Vector2Int> _resolved = new(); // New list to adding resolution

        int last = CloseList.Count - 1; // Get last index of closelist so we have to reproccess all progress from last

        while (last > 0) // if we arrive root position or not. 
        {
            if ((CloseList[last] - CloseList[last - 1]).magnitude > 1) // Check out whether has skipped nodes in the path
            {
                if (CloseList.Contains(CloseList[last] - Vector2Int.right) && CloseList.IndexOf(CloseList[last] - Vector2Int.right) < last) // Check out the sibling node that unvisited
                {
                    //Debug.Log("1****");
                    //Debug.Log("=> Last : " + CloseList[last]);
                    _resolved.Add(CloseList[last]); // Firstly , add the node before skipped
                    //Debug.Log("=> Sibling Node In past : " + (CloseList[last] - Vector2Int.right));
                    _resolved.Add(CloseList[last] - Vector2Int.right); // Get sibling node of it
                    //Debug.Log("=> Index of Sibling Node : " + CloseList.IndexOf(CloseList[last] - Vector2Int.right));
                    last = CloseList.IndexOf(_resolved.Last()) - 1; // Set new index by sibling node 
                }
                else if (CloseList.Contains(CloseList[last] - Vector2Int.up)) // Same things with above
                {
                    //Debug.Log("2****");
                    //Debug.Log("=> Last : " + CloseList[last]);
                    _resolved.Add(CloseList[last]);
                    //Debug.Log("=> Sibling Node In past : " + (CloseList[last] - Vector2Int.down));
                    _resolved.Add(CloseList[last] - Vector2Int.up);
                    //Debug.Log("=> Index of Sibling Node : " + CloseList.IndexOf(CloseList[last] - Vector2Int.down));
                    last = CloseList.IndexOf(_resolved.Last()) - 1;

                }
            }
            else
                _resolved.Add(CloseList[last--]); // If there is no node skipped , just decrease
        }
        _resolved.Reverse(); // Reverse path that was processed from last

        return _resolved.ToHashSet(); // Return HasSet<Vector2Int>()
    }

    private void PlaceSailBoat (int SecondRegion , int ThirdRegion)
    {
        SailBoat.transform.GetChild(1).GetChild(1).gameObject.SetActive(PlayerPrefs.GetInt("Vfx") > 0);
        CannonExplode = SailBoat.GetComponent<AudioSource>();
        if (SecondRegion == 1 || SecondRegion == 0)
        {
            CannonExplode.spread = 0f;
            var _SailBoat = Instantiate(SailBoat, new Vector3(4.5f, 0.16f, 14.5f), Quaternion.Euler(0f, 225f, 0f), transform);
            Sails.Add(_SailBoat.transform);
            Bullet.Add(_SailBoat.transform.GetChild(1).GetChild(0).transform);
            Explosive.Add(_SailBoat.transform.GetChild(1).GetChild(1).GetComponent<ParticleSystem>());
            foreach (Transform transform in _SailBoat.transform)
                EnemyRenderers.Add(transform.GetComponent<Renderer>());
        }
        if (ThirdRegion == 0)
        {
            CannonExplode.spread = 360f;
            var _SailBoat = Instantiate(SailBoat, new Vector3(15.5f, 0.16f, 5f), Quaternion.Euler(0f, 45f, 0f), transform);
            Sails.Add(_SailBoat.transform);
            Bullet.Add(_SailBoat.transform.GetChild(1).GetChild(0).transform);
            Explosive.Add(_SailBoat.transform.GetChild(1).GetChild(1).GetComponent<ParticleSystem>());
            foreach (Transform transform in _SailBoat.transform)
                EnemyRenderers.Add(transform.GetComponent<Renderer>());
        }
    }
    private void Shooting()
    {
        // Get Ready to Shooting
        ShootStartTime = Time.time;
        IsShooting = true;
        for (int i = 0; i < Sails.Count; i++)
        {
            Explosive[i].Play();
            Sails[i].GetComponent<AudioSource>().PlayOneShot(CannonExplode.clip, UIController._Volume);
            Bullet[i].gameObject.SetActive(true);
        }
    }
    private void LateUpdate()
    {
        if (Sails.Count > 0)
        {
            for (int i = 0; i < EnemyRenderers.Count; i++)
                EnemyRenderers[i].enabled = GeometryUtility.TestPlanesAABB(PlatformManager.Frustum_, EnemyRenderers[i].bounds);

            // Fire rate interval
            ShootInterval += Time.deltaTime;

            float offset = Mathf.PingPong(Time.time * 2.5f, 8f) - 4f;
            for (int i = 0; i < Sails.Count; i++)
                Sails[i].transform.localEulerAngles = new Vector3(offset, Sails[i].transform.localEulerAngles.y, offset * (-1));

            // At the Each 3 seconds will be shooted
            if (ShootInterval > 3f)
            {
                Shooting(); // Shooting
                ShootInterval = 0f; // Reset fire interval
            }

            if (IsShooting)
            {
                float t = Mathf.Clamp01((Time.time - ShootStartTime) * 2f); // Calculate animation time
                Vector3 start = new(-0.11f, 0.18f, -0.09f); // From where is shooting
                Vector3 final = new(start.x * 5f * PlatformManager.Stage_, start.y, start.z); // Target point

                for (int i = 0; i < Bullet.Count; i++)
                    Bullet[i].localPosition = Vector3.Lerp(start, final, t);

                // End of the shooting animation
                if (t >= 1f)
                {
                    IsShooting = false;
                    for (int i = 0; i < Bullet.Count; i++)
                        Bullet[i].gameObject.SetActive(false);
                }
            }
        }
        else
            for (int i = 0; i < EnemyRenderers.Count; i++)
                EnemyRenderers[i].enabled = GeometryUtility.TestPlanesAABB(PlatformManager.Frustum_, EnemyRenderers[i].bounds);
    }
}

public struct Node : IEquatable<Node>, IComparable<Node>
{
    public Vector2Int Coord;
    public int Cost; // F(n) = G(n) + H(n)

    public Node(Vector2Int Coord)
    {
        this.Coord = Coord;
        Cost = 0;
        CalculateCost();
    }
    private void CalculateCost ()
    {
        int heuristic = Mathf.Abs(Coord.x - 12) + Mathf.Abs(Coord.y - 12); // Heuristic Method(Manhatten Distance)
        int real = Coord.x + Coord.y; // G(n) : Real Cost
        Cost = real + heuristic; // F(n) : Total Cost
    }

    // Equality: Check it only according to the coord value
    public bool Equals(Node other)
    {
        return this.Coord == other.Coord;
    }
    public override bool Equals(object obj)
    {
        return obj is Node other && Equals(other);
    }
    public override int GetHashCode()
    {

        return Coord.GetHashCode(); // Unless the coord is odd , it is sufficient
    }
    public int CompareTo(Node other)
    {
        int costCompare = this.Cost.CompareTo(other.Cost);
        if (costCompare == 0)
            return this.Coord.GetHashCode().CompareTo(other.Coord.GetHashCode()); // Eþitse farklýlaþtýr

        //return costCompare;

        // Secondary comparison to avoid duplicate cost issues
        int xCompare = Coord.x.CompareTo(other.Coord.x);
        return xCompare != 0 ? xCompare : Coord.y.CompareTo(other.Coord.y);

    }
    public override string ToString()
    {
        return $"Node({Coord.x}, {Coord.y}) Cost: {Cost}";
    }
}

