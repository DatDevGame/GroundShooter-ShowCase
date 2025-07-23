using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Serializable2DList<T>
{
    [SerializeField] private List<List<T>> innerList = new();

    public List<List<T>> Inner => innerList;

    public T this[int row, int col]
    {
        get => innerList[row][col];
        set => innerList[row][col] = value;
    }

    public int RowCount => innerList.Count;
    public int ColumnCount => RowCount > 0 ? innerList[0].Count : 0;

    public void Resize(int rows, int cols, Func<T> factory = null)
    {
        factory ??= () => default;

        while (innerList.Count < rows)
            innerList.Add(new List<T>());

        while (innerList.Count > rows)
            innerList.RemoveAt(innerList.Count - 1);

        for (int r = 0; r < innerList.Count; r++)
        {
            while (innerList[r].Count < cols)
                innerList[r].Add(factory());

            while (innerList[r].Count > cols)
                innerList[r].RemoveAt(innerList[r].Count - 1);
        }
    }
}
