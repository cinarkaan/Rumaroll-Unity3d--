using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExceptionalPath : MonoBehaviour
{
    protected Vector3 StartPos = new Vector3(6f, 1f, 18f);

    protected Quaternion StartRotation;

    protected List<Vector3> Path = new();

    protected List<float> RotDirs = new();

    protected bool IsRotate = false, IsMoving = false, _pathprogress = false;

    protected float WheelRotationSpeed = 360f;

    protected IEnumerator AdjustRouteDirsEnemy(GameObject Bulldozer)
    {
        yield return new WaitUntil(() => _pathprogress);
        Path = CalculateDirectionOfRotate();
        Path = CompressDirections(Path);
        AdjustEnemy(Bulldozer);
    }
    protected List<Vector3> CompressDirections(List<Vector3> input)
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
    protected List<Vector3> CalculateDirectionOfRotate()
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
    protected void AdjustEnemy(GameObject _Bulldozer)
    {
        GameObject _bulldozer = Instantiate(_Bulldozer, StartPos, StartRotation, transform); // Spawing the enemy object (Bulldozer)
        StartCoroutine(Move(_bulldozer)); // Launch the periodic moves

    }
    protected IEnumerator Move(GameObject _bulldozer)
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
    protected IEnumerator Moving(GameObject _bulldozer, Vector3 Target, bool reverse, int _indexOfWay)
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
    protected IEnumerator Rotation(GameObject _bulldozer)
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
    protected IEnumerator WheelsRotationSmooth(GameObject _bulldozer, float _TargetDirection, float offset)
    {
        IsRotate = false; // Turn off the rotation control

        float _velocityOfWheels = 0.0f, _velocityOfBulldozer = 0.0f, _velocityOfSelf = 0.0f; // Velocities of the objects that will be implemented rotate
        float _StartDirectionOfWheels = 0.0f, _StartDirectionOfBulldozer = 0f, _StartDirectionOfSelf = 0.0f; // Start rotation values of objects
        while (Mathf.Abs(_TargetDirection) - Mathf.Abs(_StartDirectionOfBulldozer) > 0.01f) // Whether rotating of bulldozer completed. Check out the distance by the lengest duration
        {
            _StartDirectionOfWheels = Mathf.SmoothDampAngle(_StartDirectionOfWheels, _TargetDirection / 2, ref _velocityOfWheels, 0.5f); // Either left or right rotation of wheels +/-45f
            _StartDirectionOfBulldozer = Mathf.SmoothDampAngle(_StartDirectionOfBulldozer, _TargetDirection, ref _velocityOfBulldozer, 0.65f); // Rotation of bulldozer either +/- 90f
            _StartDirectionOfSelf = Mathf.SmoothDampAngle(_StartDirectionOfSelf, 359f, ref _velocityOfSelf, 0.85f); // The wheels must be rotated itself to seems realistic as if it was moving.
            _bulldozer.transform.GetChild(2).localEulerAngles = new Vector3(_StartDirectionOfSelf * -360f, _StartDirectionOfWheels, 0f); // implement rotations to wheel objects
            _bulldozer.transform.GetChild(3).localEulerAngles = new Vector3(_StartDirectionOfSelf * -360f, _StartDirectionOfWheels, 0f); // implement rotations to wheel objects
            _bulldozer.transform.localEulerAngles = new Vector3(0f, _StartDirectionOfBulldozer + offset, 0f); // implement rotations to bulldozer
            yield return null;
        }

        _bulldozer.transform.GetChild(2).localEulerAngles = new Vector3(_bulldozer.transform.GetChild(2).rotation.x, 0f, 0f); // Reset eulers of wheels after rotate
        _bulldozer.transform.GetChild(3).localEulerAngles = new Vector3(_bulldozer.transform.GetChild(3).rotation.x, 0f, 0f); // Reset eulers of wheels after rotate

        IsRotate = true; // Turn on the rotation control
        IsMoving = false; // End of the moving
    }


    protected virtual IEnumerator InitializeManager() { yield return null; }
    protected virtual void PathFinding() { }
    protected virtual void AStar(Vector2Int location, HashSet<Node> OpenList, ref HashSet<Vector2Int> CloseList, bool resolved) { }
    protected virtual HashSet<Vector2Int> ResolvePath(List<Vector2Int> CloseList) { return null; }
}
