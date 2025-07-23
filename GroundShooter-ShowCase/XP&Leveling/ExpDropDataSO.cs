using System;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.SerializedDataStructure;

[CreateAssetMenu(fileName = "ExpDropDataSO", menuName = Const.String.AssetMenuName + "ExpLevelingSystem/ExpDropDataSO")]
public class ExpDropDataSO : ScriptableObject
{
    [SerializeField, Title("EXP")]
    private SerializedDictionary<UnitSO, float> unitToExpAmountDictionary;
    
    public float GetExpAmount(UnitSO unitSO)
    {
        return unitToExpAmountDictionary.Get(unitSO);
    }
}