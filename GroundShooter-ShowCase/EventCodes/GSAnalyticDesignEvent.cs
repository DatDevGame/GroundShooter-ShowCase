
public enum BotTargetingType
{
    Bombot,
    Turret,
    ShooterBot
}
public enum GSAnalyticDesignEvent
{
    /// <summary>
    /// Triggered when the player fails a level.
    /// Logs a main level failure event with the following parameters:
    /// <para><b>param[0]:</b> <see cref="int"/> - Level ID (e.g., 1, 2, 3, ...)</para>
    /// <para><b>param[1]:</b> <see cref="int"/> - Attempt count (e.g., 1, 2, ...)</para>
    /// <para><b>param[2]:</b> <see cref="string"/> - Bot type (e.g., BombBot, Turret, ...)</para>
    /// <para><b>param[3]:</b> <see cref="int"/> - Level progression = (current wave / total wave) * 100</para>
    /// </summary>
    MainLevel_FailReason,

    /// <summary>
    /// Triggered when the player starts a level to track all equipped items.
    /// Logs an equipped item event with the following parameters:
    /// <para><b>param[0]:</b> <see cref="int"/> - Level ID (e.g., 1, 2, 3, ...)</para>
    /// <para><b>param[1]:</b> <see cref="string"/> - Item type (e.g., MainGun, Drone, ...)</para>
    /// <para><b>param[2]:</b> <see cref="string"/> - Item name</para>
    /// </summary>
    InUsedItem,

    /// <summary>
    /// Triggered when the player upgrades a battle skill in a level.
    /// Logs a skill upgrade event with the following parameters:
    /// <para><b>param[0]:</b> <see cref="int"/> - Current wave</para>
    /// <para><b>param[1]:</b> <see cref="string"/> - Skill type (e.g., EnhancementSkill, EquipmentSkill, ...)</para>
    /// <para><b>param[2]:</b> <see cref="string"/> - Skill name (the name of the upgraded skill)</para>
    /// <para><b>param[3]:</b> <see cref="int"/> - Star ID (the star level of the upgraded skill, starts at 1, red star at level 5)</para>
    /// </summary>
    UpgradedBattleSkill,
}
