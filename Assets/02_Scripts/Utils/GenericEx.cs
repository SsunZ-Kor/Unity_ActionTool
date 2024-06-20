using System.Collections;
using System.Collections.Generic;

public static class GenericEx
{
    public static bool AddOrRefresh<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) where TValue : class
    {
        if (dict != null && !dict.TryAdd(key, value))
        {
            dict[key] = value;
            return true;
        }

        return false;
    }
    
    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : new()
    {
        TValue val;

        if (!dict.TryGetValue(key, out val))
        {
            val = new TValue();
            dict.Add(key, val);
        }

        return val;
    }

    public static TValue GetOrNull<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key) where TValue : class
    {
        if (dict == null)
            return null;

        dict.TryGetValue(key, out var val);
        return val;
    }
    
    public static void ForEach<T>(this T[] array, System.Action<T> action)
    {
        if (array == null || action == null)
            return;

        foreach (var t in array)
            action.Invoke(t);
    }

    public static void ForEach<T>(this LinkedList<T> llist, System.Action<LinkedListNode<T>> action)
    {
        var node = llist?.First;
        if (node == null)
            return;

        while (node != null)
        {
            var currNode = node;
            node = node.Next;

            action.Invoke(currNode);
        }
    }
    
    public static void TrimNullIndex<T>(this List<T> list) where T : class
    {
        if (list == null)
            return;

        for (int i = list.Count - 1; i >= 0; --i)
        {
            if (list[i] == null)
                list.RemoveAt(i);
        }

        list.TrimExcess();
    }
    
    public static void RemoveSelf<T>(this LinkedListNode<T> node) => node?.List?.Remove(node);

    public static bool CheckIndex<T>(this List<T> list, int index) => list != null && 0 <= index && index < list.Count;

    public static bool CheckIndex(this System.Array array, int index) => array != null && 0 <= index && index < array.Length;
    
    public static T[] SubArray<T>(this T[] data, int index, int length)
    {
        T[] result = new T[length];
        System.Array.Copy(data, index, result, 0, length);
        return result;
    }
}
