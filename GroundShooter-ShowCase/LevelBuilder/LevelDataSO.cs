using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "Level_", menuName = "Game/Level Data")]
public class LevelDataSO : ItemSO
{
    // General Info Tab (light blue)
    [TabGroup("🏠 General"), GUIColor(0.9f, 0.95f, 1f)]
    [InfoBox("Display name of the level, e.g., Level 1, Boss Stage...")]
    public string LevelName = "Level 1";

    [TabGroup("🏠 General"), GUIColor(0.9f, 0.95f, 1f), LabelText("Reward")]
    [InfoBox("Reward given when this level is completed.")]
    public WeaponSO Reward;

    [TabGroup("🏠 General"), GUIColor(0.9f, 0.95f, 1f), Range(0f, 1f), LabelText("HP PowerUp Drop Chance")]
    [InfoBox("Chance for HP PowerUp to drop when an enemy is defeated.")]
    public float HpPowerUpDropChance = 0.25f;

    // Map & Turret Tab (light green)
    [TabGroup("🗺️ Map & Turret"), GUIColor(0.95f, 1f, 0.9f), LabelText("Environment")]
    [InfoBox("Map configuration for this level.")]
    public MapAssemblerShuffle Map;

    [TabGroup("🗺️ Map & Turret"), GUIColor(0.95f, 1f, 0.9f), LabelText("Turret Config")]
    [InfoBox("Turret configuration for this level.")]
    public TurretData TurretData;

    // Waves Tab (light orange)
    [TabGroup("🌊 Waves"), GUIColor(1f, 0.95f, 0.9f), LabelText("Waves")]
    [ListDrawerSettings(
        ShowFoldout = true,
        ShowIndexLabels = false,
        DraggableItems = true,
        NumberOfItemsPerPage = 0,
        CustomAddFunction = nameof(CreateNewWave),
        CustomRemoveElementFunction = nameof(RemoveWave),
        OnTitleBarGUI = nameof(DrawWaveButtons)
    )]
    [InfoBox("List of enemy waves in this level.")]
    public List<WaveData> Waves = new();

    // Battle Skills Tab (light purple)
    [TabGroup("✨ Battle Skills"), GUIColor(1f, 0.9f, 1f), LabelText("Scripted Battle Skills")]
    [InfoBox("Special scripted skills for this level.")]
    public List<BattleSkillSystem.ScriptedBattleSkill> scriptedBattleSkills = new List<BattleSkillSystem.ScriptedBattleSkill>();

    // Add New Wave Button
    [Button("Add New Wave")]
    private void AddWave()
    {
        Waves.Add(new WaveData());
    }

    // Clear All Waves Button
    [Button("Clear All Waves")]
    private void Clear()
    {
#if UNITY_EDITOR
        if (!EditorUtility.DisplayDialog("Confirm Deletion",
        $"Are you sure you want to delete all?",
        "Yes", "Cancel"))
        {
            return;
        }
#endif
        Waves.Clear();
    }

    // Custom add/remove for Waves list
    private WaveData CreateNewWave() => new WaveData();
    private void RemoveWave(WaveData wave) => Waves.Remove(wave);

    // Draw custom buttons on the Waves list title bar (for future extension)
#if UNITY_EDITOR
    private void DrawWaveButtons()
    {
        // Example: Add custom buttons here if needed
    }
#endif

    // Custom label for each wave (optional, for future use)
    // private string GetWaveLabel(int index) => $"🌊 Wave {index + 1}";
}
