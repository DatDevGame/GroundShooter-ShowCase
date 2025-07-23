using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerBattleSkillController : MonoBehaviour
{
    public Dictionary<BattleSkillSO, int> ActiveBattleSkillDictionary => activeBattleSkillDictionary;

    private const string DebugTag = nameof(PlayerBattleSkillController);

    private PlayerUnit playerUnit;
    [ShowInInspector, ReadOnly]
    private Dictionary<BattleSkillSO, int> activeBattleSkillDictionary = new Dictionary<BattleSkillSO, int>();

    private void Start()
    {
        playerUnit = PlayerUnit.Instance;
    }

    public void AddBattleSkill(BattleSkillSO battleSkillSO)
    {
        if (activeBattleSkillDictionary.TryGetValue(battleSkillSO, out int stack))
        {
            if (stack < battleSkillSO.MaxStack)
            {
                activeBattleSkillDictionary[battleSkillSO] += 1;
                battleSkillSO.Apply(playerUnit);
                LGDebug.Log($"BattleSkill {battleSkillSO.name} stacked to {stack}", DebugTag);
            }
            else
            {
                LGDebug.Log($"BattleSkill {battleSkillSO.name} already applied and can't stack further.", DebugTag);
            }
        }
        else
        {
            battleSkillSO.Apply(playerUnit);
            activeBattleSkillDictionary.Add(battleSkillSO, 1);
        }
        LGDebug.Log($"BattleSkill {battleSkillSO.name} applied.", DebugTag);
    }

    public void RemoveBattleSkill(BattleSkillSO battleSkillSO)
    {
        if (!activeBattleSkillDictionary.TryGetValue(battleSkillSO, out int stack))
            return;

        if (stack > 1)
        {
            activeBattleSkillDictionary[battleSkillSO] -= 1;
            battleSkillSO.Remove(playerUnit);
            LGDebug.Log($"BattleSkill {battleSkillSO.name} stack reduced to {stack}", DebugTag);
        }
        else
        {
            activeBattleSkillDictionary.Remove(battleSkillSO);
            LGDebug.Log($"BattleSkill {battleSkillSO.name} removed.", DebugTag);
        }
    }

    public int GetBattleSkillStack(BattleSkillSO battleSkillSO)
    {
        return activeBattleSkillDictionary.Get(battleSkillSO);
    }
}