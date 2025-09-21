using System.Collections.Generic;
using UnityEngine;

public static class NumberSettings
{
    public static int GetBiasedRandom(int min, int max)
    {
        // Build weights
        List<int> numbers = new List<int>();
        List<float> weights = new List<float>();

        for (int i = min; i <= max; i++)
        {
            numbers.Add(i);
            weights.Add(1f / i); // smaller i = higher weight
        }

        // Weighted pick
        float total = 0f;
        foreach (var w in weights) total += w;

        float r = Random.value * total;
        for (int i = 0; i < numbers.Count; i++)
        {
            if (r < weights[i])
                return numbers[i];
            r -= weights[i];
        }

        return numbers[numbers.Count - 1];
    }

}
