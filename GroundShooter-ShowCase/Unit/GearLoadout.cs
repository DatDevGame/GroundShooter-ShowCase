using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearLoadout : MonoBehaviour
{
    public static GearLoadout Instance { get; private set; }

    public PPrefItemSOVariable ArmorCurrentInUse => m_ArmorCurrentInUse;
    public PPrefItemSOVariable DroneCurrentInUse => m_DroneCurrentInUse;
    public PPrefItemSOVariable MainGunCurrentInUse => m_MainGunCurrentInUse;
    public PPrefItemSOVariable GadgetCurrentInUse => m_GadgetCurrentInUse;

    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_ArmorCurrentInUse;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_DroneCurrentInUse;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_MainGunCurrentInUse;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_GadgetCurrentInUse;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple instances of GearSaver found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
