using System;
using System.Collections.Generic;
using UnityEngine;
public class UniqueRandomGenerator
{
    public HashSet<int> UniqueRandoms {  get; private set; }
    public int Min { private get;  set; }
    public int Max { private get; set; }
    public int Count { private get; set; }

    public UniqueRandomGenerator ()
    {
        UniqueRandoms = new HashSet<int> ();
    }
    public UniqueRandomGenerator (int min, int max, int count)
    {
        UniqueRandoms = new HashSet<int>();
        this.Min = min;
        this.Max = max;
        this.Count = count;
        Generate();
    }
    public void Generate()
    {
        while (UniqueRandoms.Count < Count)
        {
            int randomValue = UnityEngine.Random.Range(Min, Max);
            UniqueRandoms.Add(randomValue);
        }
    }
    public void GenerateBySolutions (List<Vector2Int> solution, List<Vector2Int> unSolution)
    {
        for (int i = 0; i < unSolution.Count; i++)
        {
            try
            {
                if (unSolution.Contains(solution[i] + Vector2Int.up))
                    UniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.up));
                else if (unSolution.Contains(solution[i] + Vector2Int.down))
                    UniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.down));
                else if (unSolution.Contains(solution[i] + Vector2Int.right))
                    UniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.right));
                else if (unSolution.Contains(solution[i] + Vector2Int.left))
                    UniqueRandoms.Add(unSolution.IndexOf(solution[i] + Vector2Int.left));
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
