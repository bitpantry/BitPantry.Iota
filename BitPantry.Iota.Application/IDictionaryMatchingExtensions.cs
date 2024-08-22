using System;
using BitPantry.Iota.Infrastructure;
using Microsoft.IdentityModel.Tokens;

namespace BitPantry.Iota.Application;

public static class IDictionaryMatchingExtensions
{
    public static TKey MatchValue<TKey>(this IDictionary<TKey, string> dict, string str)
    {
        TKey matchingKey = default;

        int minDistance = int.MaxValue;

        foreach (var item in dict)
        {
            int distance = str.CalculateLevenshteinDistance(item.Value);
            
            if (distance < minDistance)
            {
                minDistance = distance;
                matchingKey = item.Key;                
            }
        }

        return matchingKey;
    }


}

