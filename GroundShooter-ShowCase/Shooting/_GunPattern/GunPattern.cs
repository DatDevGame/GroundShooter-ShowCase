using UnityEngine;
using System.Collections;

public abstract class GunPattern : ScriptableObject
{
    /// <summary>Spawns bullets or logic from the gun</summary>
    public abstract IEnumerator Execute(Gun gun);

    /// <summary>Draws gizmos in editor or runtime</summary>
    public virtual void DrawGizmos(Gun gun) { }
}
