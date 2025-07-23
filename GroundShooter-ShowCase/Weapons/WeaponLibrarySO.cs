using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "WeaponLibrarySO", menuName = "GroundShooter/Weapons/WeaponLibrarySO")]
public class WeaponLibrarySO : SerializedScriptableObject
{
    public List<WeaponManagerSO> WeaponManagerSOs;
}
