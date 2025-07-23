using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyProgression
{
    public static string GetKeyAttemptMainLevel(int levelID)
    {
        return $"{levelID}-keyAttemptCount_Value";
    }
}
public enum GSAnalyticProgressionEvent
{
    /// <summary>
    /// Tracks progression for main game levels.
    /// <para><b>param[0]:</b> <see cref="GAProgressionStatus"/> - Status (Start, Complete, Fail)</para>
    /// <para><b>param[1]:</b> <see cref="int"/> - Level ID (e.g., 1, 2, 3, ...)</para>
    /// <para><b>param[2]:</b> <see cref="int"/> - Attempt count (e.g., 1, 2, ...)</para>
    /// <para><b>param[3]:</b> <see cref="int"/> - Time played (seconds)</para>
    /// </summary>
    MainLevel
}
