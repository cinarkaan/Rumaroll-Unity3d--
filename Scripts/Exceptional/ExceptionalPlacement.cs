using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExceptionalPlacement : MonoBehaviour
{
    protected int Stage = 0;

    protected int UnSolutionCount = 0;

    public ExceptionalPlacement (int Stage, int UnSolutionCount)
    {
        this.Stage = Stage;
        this.UnSolutionCount = UnSolutionCount;
    }

    protected List<Vector2> ExceptionalPlacementOfHazard(bool vertical, bool horizontal, int verticalCount, int horizontalCount)
    {
        List<Vector2> pos = new List<Vector2>();

        float middle = Stage / 2 + 6 + 0.5f;
        middle -= (Stage % 2 != 0) ? 0 : Random.Range(0, 2); // Exact the middle of grid

        if (vertical)
        {
            while (verticalCount > 0)
            {
                pos.Add(new Vector2(middle, 6));
                if (Stage % 2 == 0) // Which side it must be place
                    middle += ((Stage / 2 + 6 + 0.5f) > middle) ? 1f : -1f;
                pos.Add(new Vector2(middle, Stage + 6));
                verticalCount -= 2;
            }
        }
        else if (horizontal)
        {
            while (horizontalCount > 0)
            {
                pos.Add(new Vector2(Stage + 6, middle));
                if (Stage % 2 == 0) // Which side it must be place left or right
                    middle += ((Stage / 2 + 6 + 0.5f) > middle) ? 1f : -1f;
                pos.Add(new Vector2(6, middle));
                horizontalCount -= 2;
            }
        }

        return pos;
    }
    protected List<Vector2Int> ExceptionalPlacementOfCutter(bool final, bool start, bool diamond)
    {
        List<Vector2Int> placed = new List<Vector2Int>();

        if (final) // In finish tile
            placed.Add(new Vector2Int(Stage + 6, Stage + 6));
        if (start) // In origin tile
            placed.Add(new Vector2Int(6, 6));

        return placed;
    }
    protected List<Vector2Int> ExceptionalPlacementOfSpike(List<Vector2Int> path, List<Vector2Int> Solution, int firstRegionCount, int secondRegionCount, int thirdRegionCount, int fourthRegionCount)
    {
        Vector2Int first = Vector2Int.zero;
        Vector2Int second = Vector2Int.zero;
        Vector2Int third = Vector2Int.zero;
        Vector2Int fourth = Vector2Int.zero;

        List<Vector2Int> place = new List<Vector2Int>();

        if (firstRegionCount > 0) // Started region
        {
            int middleXY = (Stage % 2 == 0) ? Stage / 2 + 5 : Stage / 2 + 6; // Exact middle point of grid
            for (int x = 6; x <= middleXY; x++)
            {
                for (int y = 6; y <= middleXY; y++)
                {
                    if (((x == 6 && y == 7) || (x == 7 && y == 6)) && UnSolutionCount == path.Count) // Blade is not placed on next step of origin
                        continue;
                    if (x == 6 && y == 6) // Blade or spike can not placed on the origin 
                        continue;
                    if (path.Contains(new Vector2Int(x, y))) // If path includes unsolutions or solutions according to the object type(Blade or Spike)
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == UnSolutionCount ? CheckOutNeighbors(place,Solution) : place; // Warning this step only works when blades are placed , so not spike.
            first = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)]; // If it does not contains placeble path , return zero
            place.Clear(); // Clear path so that it will be able to used other regions
        }
        if (secondRegionCount > 0)
        {
            int startPointY = Stage / 2 + 7;
            int middleX = (Stage % 2 == 0) ? Stage / 2 + 5 : Stage / 2 + 6;
            for (int x = 6; x <= middleX; x++)
            {
                for (int y = startPointY; y <= Stage + 6; y++)
                {
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == UnSolutionCount ? CheckOutNeighbors(place, Solution) : place;
            second = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        if (thirdRegionCount > 0)
        {
            int middleXY = (Stage / 2) + 7;
            for (int x = middleXY; x <= Stage + 6; x++)
            {
                for (int y = middleXY; y <= Stage + 6; y++)
                {
                    if (x == Stage + 6 && y == Stage + 6)
                        continue;
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == UnSolutionCount ? CheckOutNeighbors(place, Solution) : place;
            third = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        if (fourthRegionCount > 0)
        {
            int startPointX = Stage / 2 + 7;
            int middleY = (Stage % 2 == 0) ? Stage / 2 + 5 : Stage / 2 + 6;
            for (int x = startPointX; x <= Stage + 6; x++)
            {
                for (int y = 6; y <= middleY; y++)
                {
                    if (path.Contains(new Vector2Int(x, y)))
                        place.Add(new Vector2Int(x, y));
                }
            }
            place = path.Count == UnSolutionCount ? CheckOutNeighbors(place, Solution) : place;
            fourth = (place.Count == 0) ? Vector2Int.zero : place[Random.Range(0, place.Count)];
            place.Clear();
        }
        place.Add(first);
        place.Add(second);
        place.Add(third);
        place.Add(fourth);
        return place;
    }
    protected List<Vector2Int> ExceptionalPlacementOfBlade(List<Vector2Int> path, List<Vector2Int> Solution, bool vertical, bool horizontal, int firstRegion, int secondRegion, int thirdRegion, int fourthRegion)
    {
        List<Vector2Int> placed = new List<Vector2Int>(); // To be placed hazards list
        List<Vector2Int> finded = new List<Vector2Int>(); // the coordinates that scanned in the place whether it is appropriate or not as relative(if it include solution next to tile or not) 
        if (Stage % 2 == 0)
        { // If the stage is even , just place it acoording to the vertical or horizontal at the middle of
            int middlePoint = Stage / 2 + 6;
            if (vertical)
            {
                for (int y = 6; y < middlePoint; y++)
                {
                    if (path.Contains(new Vector2Int(y, middlePoint))) // Find unsolutions that is to be placed 
                        finded.Add(new Vector2Int(y, middlePoint));
                }
                finded = CheckOutNeighbors(finded, Solution); // if the coordinates that was found includes the solution at next to or not
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero); // If the placeable hazard coordinat was founded , add it's coord
                finded.Clear(); // Clearing finded list
                for (int y = middlePoint; y <= Stage + 6; y++)
                {
                    if (path.Contains(new Vector2Int(y, middlePoint))) // Same way on the above but this time do it for second half of path
                        finded.Add(new Vector2Int(y, middlePoint));
                }
                finded = CheckOutNeighbors(finded, Solution);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
            }
            if (horizontal) // It works same way on the above , but this time horizontal
            {
                for (int x = 6; x < middlePoint; x++)
                {
                    if (path.Contains(new Vector2Int(middlePoint, x)))
                        finded.Add(new Vector2Int(middlePoint, x));
                }
                finded = CheckOutNeighbors(finded, Solution);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
                for (int x = middlePoint; x <= Stage + 6; x++)
                {
                    if (path.Contains(new Vector2Int(middlePoint, x)))
                        finded.Add(new Vector2Int(middlePoint, x));
                }

                finded = CheckOutNeighbors(finded, Solution);
                placed.Add(finded.Count != 0 ? finded[Random.Range(0, finded.Count)] : Vector2Int.zero);
                finded.Clear();
            }
            return placed;
        }
        else // If the stage is not even so odd then place all according to the region system just like spikes 
            placed = ExceptionalPlacementOfSpike(path, Solution,firstRegion, secondRegion, thirdRegion, fourthRegion);

        return placed;
    }
    protected List<Vector2Int> CheckOutNeighbors(List<Vector2Int> path, List<Vector2Int> Solutions)
    {
        if (path.Count == 0) // If there are not either solutionable or unsolotionable path , then return zero
            return path;
        foreach (var item in path.ToList()) // Check out how many solution path contains these solutions. Four way must be checked
        {
            bool horizontal = Solutions.Contains(item + Vector2Int.right) || Solutions.Contains(Vector2Int.left + item);
            bool vertical = Solutions.Contains(item + Vector2Int.up) || Solutions.Contains(Vector2Int.down + item);
            if (vertical == false && horizontal == false)
                path.Remove(item);
        }
        return path;
    }
    protected Vector3 SetDirectionOfMovedHazard(Vector3 pos, bool Vertical)
    {
        int middle = (Stage + 12) / 2;

        if (Vertical) // Exact the which way does has to move as stage
            return middle <= pos.x ? Vector3.left : Vector3.right;
        else
            return middle <= pos.z ? Vector3.back : Vector3.forward;
    }
    protected Vector3 MovedParts(Vector3 pos, Vector3 dir, float dis, float speed, bool isY)
    {
        if (isY)
            pos.y = 0.7f;
        // The move must be continious on the indicated range
        float offset = Mathf.PingPong(Time.time * speed, dis);
        Vector3 final = pos + dir * offset;
        return final;
    }

}
