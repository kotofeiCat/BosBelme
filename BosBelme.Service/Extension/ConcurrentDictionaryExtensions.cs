using System;
using System.Collections.Generic;
using System.Text;

namespace BosBelme.Service.Extension
{
    public static class ConcurrentDictionaryExtensions
    {
        extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict) where TKey : class
        {
            public bool TryGetKey(TValue value, out TKey? key)
            {
                foreach (var kvp in dict)
                {
                    if (EqualityComparer<TValue>.Default.Equals(kvp.Value, value))
                    {
                        key = kvp.Key;
                        return true;
                    }
                }
                key = null;
                return false;
            }
        }
    }
}
