
public enum GameFlowState
{
    None,

    /// <summary>
    /// This event is raised when Start Level
    /// <para> <typeparamref name="LevelDataSO"/>: parrams[0] is LevelDataSO </para>
    /// </summary>
    StartLevel,

    /// <summary>
    /// This event is raised when Start Level
    /// <para> <typeparamref name="LevelDataSO"/>: parrams[0] is LevelDataSO </para>
    /// <para> <typeparamref name="bool"/>: parrams[1] is Win </para>
    /// </summary>
    EndLevel,
}

public enum WaveStats
{
    /// <summary>
    /// This event is raised Spawn Wave
    /// <para> <typeparamref name="Int"/>: parrams[0] is Current Wave </para>
    /// <para> <typeparamref name="Int"/>: parrams[1] is Max Wave </para>
    /// </summary>
    Spawning,

    /// <summary>
    /// This event is raised when Kill Enemy
    /// <para> <typeparamref name="Int"/>: parrams[0] is Current Kill </para>
    /// <para> <typeparamref name="Int"/>: parrams[1] is Max Kill </para>
    /// </summary>
    OnKillEnemy
}

