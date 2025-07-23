using UnityEngine;

[System.Serializable]
public class SkillStatData
{
    public SkillStatData(TypeBattleCard type, int id, string name, Sprite icon)
    {
        m_type = type;
        m_Id = id;
        m_DisplayName = name;
        m_Icon = icon;
    }

    private TypeBattleCard m_type;
    private int m_Id;
    private string m_DisplayName;
    private Sprite m_Icon;

    public TypeBattleCard TypeBattleCard => m_type;
    public int Id => m_Id;
    public string DisplayName => m_DisplayName;
    public Sprite Icon => m_Icon;
}
