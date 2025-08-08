using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private GameObject bulldozer;

    [SerializeField]
    private PlatformManager platformManager;

    [SerializeField]
    private EventManager eventManager;

    [SerializeField]
    private ObstacleManager obstacleManager;

    [SerializeField]
    private float WheelRotationSpeed = 360f;

    [SerializeField]
    private UIController controller;

    private Vector3 StartPos = new Vector3(6f, 1f, 18f);

    private int[,] _Map = new int[13, 13];

    private List<Vector3> Path = new();

    private List<float> RotDirs = new();

    private Quaternion StartRotation;

    private bool IsRotate = false, IsMoving = false, _pathprogress = false;
    private void Start()
    {
        var _sound = bulldozer.GetComponent<AudioSource>();
        _sound.PlayOneShot(_sound.clip, controller._volume);
        StartCoroutine(InitializeManager());
    }
    private IEnumerator InitializeManager ()
    {
        yield return new WaitUntil(() => ObstacleManager.progress);

        if (platformManager.stage == 12) // The manager only must be worked at the stage 12 
        {
            PathFinding();
            StartCoroutine(AdjustRouteDirsEnemy());
        }

        yield return null;
    }
    private IEnumerator AdjustRouteDirsEnemy ()
    {
        yield return new WaitUntil(() => _pathprogress);
        Path = CalculateDirectionOfRotate();
        Path = CompressDirections(Path);
        AdjustEnemy();
    }
    private List<Vector3> CompressDirections(List<Vector3> input)
    {
        List<Vector3> Compressed = new List<Vector3>(); // Primitive Vector3 values will be compressed as smaller path 

        // If compressed is empty
        if (input == null || input.Count == 0)
            return Compressed;

        Vector3 currentDir = input[0];
        int count = 1;

        // Calculate same ways and then combine them
        for (int i = 1; i < input.Count; i++)
        {
            if (input[i] == currentDir)
            {
                count++;
            }
            else
            {
                Compressed.Add(currentDir * count);
                currentDir = input[i];
                count = 1;
            }
        }

        // Adding last way
        Compressed.Add(currentDir * count);

        return Compressed;
    }
    private List<Vector3> CalculateDirectionOfRotate ()
    {
        List<Vector3> dirs = new List<Vector3>(); // The list that is recorded rotation angle values

        for (int i = 0; i < Path.Count - 1; i++) // Turn into primitive Vector3 values
        {
            Vector3 dir = (Path[i + 1] - Path[i]).normalized; // Calculate the direction as primitive

            if (Vector3.right == dir)
                dirs.Add(Vector3.right);
            if (Vector3.left == dir)
                dirs.Add(Vector3.left);
            if (Vector3.forward == dir)
                dirs.Add(Vector3.forward);
            if (Vector3.back == dir)
                dirs.Add(Vector3.back);


            if (i >= 1) // Calculate the rotation angles after first index
            {
                if (dirs[i - 1].Equals(Vector3.right))
                {
                    if (dirs[i].Equals(Vector3.forward))
                    {
                        RotDirs.Add(-90f);
                    }
                    else if (dirs[i].Equals(Vector3.back))
                    {
                        RotDirs.Add(90f);
                    }
                    else
                        continue;
                }
                else if (dirs[i - 1].Equals(Vector3.left))
                {
                    if (dirs[i].Equals(Vector3.forward))
                    {
                        RotDirs.Add(90f);
                    }
                    else if (dirs[i].Equals(Vector3.back))
                    {
                        RotDirs.Add(-90f);
                    }
                    else
                        continue;
                }
                else if (dirs[i - 1].Equals(Vector3.forward))
                {
                    if (dirs[i].Equals(Vector3.right))
                    {
                        RotDirs.Add(90f);
                    }
                    else if (dirs[i].Equals(Vector3.left))
                    {
                        RotDirs.Add(-90f);
                    }
                    else
                        continue;
                }
                else if (dirs[i - 1].Equals(Vector3.back))
                {
                    if (dirs[i].Equals(Vector3.right))
                    {
                        RotDirs.Add(-90f);
                    }
                    else if (dirs[i].Equals(Vector3.left))
                    {
                        RotDirs.Add(90f);
                    }
                    else
                        continue;
                }
            }

        }
        // Enemy spawn rotations when it spawn
        if (Vector3.right == dirs.First())
            StartRotation.eulerAngles = new Vector3(0f, 90f, 0f);
        else if (Vector3.back == dirs.First())
            StartRotation.eulerAngles = new Vector3(0f, 180f, 0f);
        else
            StartRotation = Quaternion.identity;

            return dirs;
    }
    private void PathFinding ()
    {
        // First evulation of map without obstacles , with events
        for (int i = 0; i < eventManager.precious.Count; i++)
        {
            Vector2Int coord = new Vector2Int()
            {
                x = Mathf.Abs(6 - ((int)eventManager.precious[i].transform.position.x)),
                y = Mathf.Abs(18 - ((int)eventManager.precious[i].transform.position.z))
            }; // Turn into local map system according to the A* algorithm

            _Map[coord.x, coord.y] = 1; // Assign to obstacle symbol
        } // Initializing the first obstacles (Cutters)

        // Adding obstacles in to the map
        for (int i = 0; i < obstacleManager.cutters.Count; i++)
        {
            Vector2Int coord = new Vector2Int()
            {
                x = Mathf.Abs(6 - ((int)obstacleManager.cutters[i].transform.position.x)),
                y = Mathf.Abs(18 - ((int)obstacleManager.cutters[i].transform.position.z))
            }; // Turn into local map system according to the A* algorithm

            _Map[coord.x, coord.y] = 1; // Assign to obstacle symbol

        }

        //Exception case : The bulldozer might be moved to arrive point , even if there has been loot(coins or diamond) at this point. Other wise the recursive method will be throwed exception
        _Map[12, 12] = 0;

        HashSet<Vector2Int> CloseList = new HashSet<Vector2Int>(); // Initialize visited nodes

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
    private void AStar(Vector2Int location, HashSet<Node> OpenList, ref HashSet<Vector2Int> CloseList, bool resolved)
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
        if (right_N.y <= 12 && _Map[right_N.x, right_N.y] != 1)
            OpenList.Add(new Node(right_N)); // Calculate total cost and evaluate open list for first neighboord
        

        // First check out the boundary then obstacle
        if (down_N.x <= 12 && _Map[down_N.x, down_N.y] != 1)
            OpenList.Add(new Node(down_N)); // Calculate total cost and evaluate open list for second neighboord}
    
        if (CloseList.Count > 1 && (CloseList.Last() - CloseList.ToArray()[CloseList.Count - 2]).magnitude > 1) // If there are available the nodes that skipped, the path have to resolve  
            resolved = true;

        // Recursive the method for the next steps
        AStar(OpenList.First().Coord, OpenList, ref CloseList, resolved);
    }
    private HashSet<Vector2Int> ResolvePath(List<Vector2Int> CloseList)
    {
        List<Vector2Int> _resolved = new List<Vector2Int>(); // New list to adding resolution

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
    private void AdjustEnemy ()
    {
        GameObject _bulldozer = Instantiate(bulldozer, StartPos, StartRotation, transform); // Spawing the enemy object (Bulldozer)
        StartCoroutine(Move(_bulldozer)); // Launch the periodic moves
    }
    private IEnumerator Move (GameObject _bulldozer)
    {
        IsRotate = true;
        StartCoroutine(Rotation(_bulldozer)); // Launch the rotate of the wheels
        while (_pathprogress)
        {
            int _indexOfWay = 0; // First road on the path
            while (_indexOfWay < Path.Count) // Have arrived to the finish point
            {
                yield return new WaitUntil(() => !IsMoving); // Wait until end of the current move
                StartCoroutine(Moving(_bulldozer, Path[_indexOfWay], false, _indexOfWay)); // Start another moving
                _indexOfWay++; // Increament road 
            }

            yield return new WaitUntil(() => !IsMoving); // Wait until end of the current move

            Path.Reverse(); // The bulldozer will be moved from reverse of the path
            RotDirs.Reverse(); // We have to take reverse of rotation values

            _bulldozer.transform.localEulerAngles = new Vector3(0f, _bulldozer.transform.localEulerAngles.y - 180f, 0f); // Get symmetry of start position by subtracting of start position

            _indexOfWay = 0; // Reset index of the path

            while (_indexOfWay < Path.Count) // Check out whether complated the path
            {
                yield return new WaitUntil(() => !IsMoving); // Wait until the current move
                StartCoroutine(Moving(_bulldozer, (-1 * Path[_indexOfWay]), true, _indexOfWay)); // Launh another move
                _indexOfWay++; // Increament road 
            }

            yield return new WaitUntil(() => !IsMoving); // Wait until the current move

            Path.Reverse(); // We have to reverse path again
            RotDirs.Reverse(); // We have to rotations path again

            _bulldozer.transform.localEulerAngles = StartRotation.eulerAngles; // Get symmetry of start position by subtracting of start position

            _indexOfWay = 0; // Reset road index
        }
    }
    private IEnumerator Moving (GameObject _bulldozer, Vector3 Target, bool reverse, int _indexOfWay)
    {
        IsMoving = true; // The move has been started
        Vector3 _velocityOfBulldozer = Vector3.zero; // Reset velocity
        float _RotDir = (_indexOfWay < RotDirs.Count) ? RotDirs[_indexOfWay] : 0f; // Get the angle value that is belong way that will be rotated
        Vector3 _ArrivePoint = _bulldozer.transform.position + Target; // Calculate the arrive point
        while (Vector3.Distance(_bulldozer.transform.position, _ArrivePoint) > 0.15f) // Whether arrived or not to the point
        {
            _bulldozer.transform.position = Vector3.SmoothDamp(_bulldozer.transform.position, _ArrivePoint, ref _velocityOfBulldozer, Path[_indexOfWay].magnitude / 2); // Implement interpolation
            yield return null;
        }

        _bulldozer.transform.position = _ArrivePoint; // Close small distance so it must be reached out complately

        _RotDir = reverse ? (-1f * _RotDir) : _RotDir; // If the path is reversed , each value will be multiply with -1  

        StartCoroutine(WheelsRotationSmooth(_bulldozer, _RotDir, _bulldozer.transform.localEulerAngles.y)); // Rotationing

        IsMoving = (_RotDir == 0) ? false : true; // End of the moving by the rotation 
    }
    private IEnumerator Rotation (GameObject _bulldozer)
    {
        while (_pathprogress)
        {
            if (IsRotate) // Continuous rotating wheels on the terrain
            {
                _bulldozer.transform.GetChild(0).Rotate(100f * Time.deltaTime * Vector3.right, Space.Self); // Wheel 
                _bulldozer.transform.GetChild(1).Rotate(100f * Time.deltaTime * Vector3.right, Space.Self); // Wheel
                _bulldozer.transform.GetChild(2).Rotate(100f * Time.deltaTime * Vector3.right, Space.Self); // Wheel
                _bulldozer.transform.GetChild(3).Rotate(100f * Time.deltaTime * Vector3.right, Space.Self); // Wheel
            }
                yield return null;
        }
    }
    private IEnumerator WheelsRotationSmooth(GameObject _bulldozer, float _TargetDirection, float offset)
    {
        IsRotate = false; // Turn off the rotation control

        float _velocityOfWheels = 0.0f, _velocityOfBulldozer = 0.0f , _velocityOfSelf= 0.0f; // Velocities of the objects that will be implemented rotate
        float _StartDirectionOfWheels = 0.0f, _StartDirectionOfBulldozer = 0f, _StartDirectionOfSelf = 0.0f; // Start rotation values of objects
        while (Mathf.Abs(_TargetDirection) - Mathf.Abs(_StartDirectionOfBulldozer) > 0.01f) // Whether rotating of bulldozer completed. Check out the distance by the lengest duration
        {
            _StartDirectionOfWheels = Mathf.SmoothDampAngle(_StartDirectionOfWheels, _TargetDirection / 2, ref _velocityOfWheels, 0.5f); // Either left or right rotation of wheels +/-45f
            _StartDirectionOfBulldozer = Mathf.SmoothDampAngle(_StartDirectionOfBulldozer, _TargetDirection, ref _velocityOfBulldozer, 0.65f); // Rotation of bulldozer either +/- 90f
            _StartDirectionOfSelf = Mathf.SmoothDampAngle(_StartDirectionOfSelf, 359f, ref _velocityOfSelf, 0.85f); // The wheels must be rotated itself to seems realistic as if it was moving.
            _bulldozer.transform.GetChild(2).localEulerAngles = new Vector3(_StartDirectionOfSelf * -360f, _StartDirectionOfWheels, 0f); // implement rotations to wheel objects
            _bulldozer.transform.GetChild(3).localEulerAngles = new Vector3(_StartDirectionOfSelf * -360f, _StartDirectionOfWheels,0f); // implement rotations to wheel objects
            _bulldozer.transform.localEulerAngles = new Vector3(0f, _StartDirectionOfBulldozer + offset, 0f); // implement rotations to bulldozer
            yield return null;
        }

        _bulldozer.transform.GetChild(2).localEulerAngles = new Vector3(_bulldozer.transform.GetChild(2).rotation.x, 0f, 0f); // Reset eulers of wheels after rotate
        _bulldozer.transform.GetChild(3).localEulerAngles = new Vector3(_bulldozer.transform.GetChild(3).rotation.x, 0f, 0f); // Reset eulers of wheels after rotate

        IsRotate = true; // Turn on the rotation control
        IsMoving = false; // End of the moving
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

