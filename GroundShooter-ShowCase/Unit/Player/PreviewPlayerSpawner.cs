using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PreviewPlayerSpawner : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RigBuilder m_RigBuilder;
    [SerializeField, BoxGroup("Ref")] private Transform m_LoadoutHolder;

    private void Start()
    {
        LoadoutPreview();
    }

    public void LoadoutPreview()
    {
        WeaponManager.Instance.Clear();
        WeaponManager.Instance.InitPreviewWeapons();

        m_RigBuilder.enabled = false;

        //MainGun
        var mainGun = WeaponManager.Instance.GetWeapon(WeaponType.MainGun);
        mainGun.transform.SetParent(m_LoadoutHolder);
        mainGun.transform.localEulerAngles = Vector3.zero;
        mainGun.transform.localPosition = Vector3.zero;

        m_RigBuilder.enabled = true;
    }
}
