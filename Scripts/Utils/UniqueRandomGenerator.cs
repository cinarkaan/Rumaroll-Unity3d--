using System;
using System.Collections.Generic;
using UnityEngine;
public class UniqueRandomGenerator
{
    public HashSet<int> uniqueRandoms {  get; private set; }
    public int min { private get;  set; }
    public int max { private get; set; }
    public int count { private get; set; }

    public UniqueRandomGenerator ()
    {
        uniqueRandoms = new HashSet<int> ();
    }
    public UniqueRandomGenerator (int min, int max, int count)
    {
        uniqueRandoms = new HashSet<int>();
        this.min = min;
        this.max = max;
        this.count = count;
        Generate();
    }
    public void Generate()
    {
        while (uniqueRandoms.Count < count)
        {
            int randomValue = UnityEngine.Random.Range(min, max);
            uniqueRandoms.Add(randomValue);
        }
    }
    public void GenerateBySolutions (List<Vector2Int> solution, List<Vector2Int> unSolution)
    {
        for (int i = 0; i < unSolution.Count; i++)
        {
            try
            {
                if (unSolution.Contains(solution[i] + Vector2Int.up))
                    uniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.up));
                else if (unSolution.Contains(solution[i] + Vector2Int.down))
                    uniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.down));
                else if (unSolution.Contains(solution[i] + Vector2Int.right))
                    uniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.right));
                else if (unSolution.Contains(solution[i] + Vector2Int.left))
                    uniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.left));
                else
                    continue;
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }
        }

    }
}
