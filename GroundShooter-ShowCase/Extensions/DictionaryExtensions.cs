using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class DictionaryExtensions
{
    public static bool TryGetRandom<TKey, TValue>(this Dictionary<TKey, TValue> dict, out TValue value)
    {
        value = default;
        if (dict == null || dict.Count == 0) return false;
        int index = Random.Range(0, dict.Count);
        value = dict.ElementAt(index).Value;
        return true;
    }
}