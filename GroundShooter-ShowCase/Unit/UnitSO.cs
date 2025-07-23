using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitSO", menuName = Const.String.AssetMenuName + "UnitSO")]
public class UnitSO : ScriptableObject
{
    [Header("Stats")]
    [SerializeField]
    protected float expDropOnDead;
    [SerializeField]
    protected SerializedDictionary<StatType, Stat> initialStatDictionary = new SerializedDictionary<StatType, Stat>();

    public float ExpDropOnDead => expDropOnDead;
    public SerializedDictionary<StatType, Stat> InitialStatDictionary => initialStatDictionary;
}