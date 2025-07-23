using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class MapAssemblerShuffle : MonoBehaviour
{
    [SerializeField] private List<Transform> m_ChildPieces;

    private static readonly Vector3[] s_Positions =
    {
        new Vector3(-60, 0, 0),
        new Vector3(60, 0, 0),
        new Vector3(-60, 0, 100), 
        new Vector3(60, 0, 100) 
    };

    private static readonly Vector3[] s_Eulers =
    {
        new Vector3(0, 180, 0), 
        new Vector3(0, 0, 0),
        new Vector3(0, 180, 0), 
        new Vector3(0, 0, 0)
    };

    [Button]
    public void ShuffleMap()
    {
        if (m_ChildPieces.Count != 4)
            return;

        List<Transform> shuffledPieces = new List<Transform>(m_ChildPieces);
        Shuffle(shuffledPieces);

        for (int i = 0; i < 4; i++)
        {
            Transform piece = shuffledPieces[i];
            piece.localPosition = s_Positions[i];
            piece.localEulerAngles = s_Eulers[i];
            piece.name = $"Piece_{i + 1}";
        }
    }

    /// <summary>
    /// Fisher–Yates shuffle
    /// </summary>
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = Random.Range(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
