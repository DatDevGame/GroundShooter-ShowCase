using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Gun/Sequence Pattern")]
public class SequencePattern : GunPattern
{
    public GunPattern[] sequence;
    public float delayBetween = 0.2f;

    public override IEnumerator Execute(Gun gun)
    {
        foreach (var pattern in sequence)
        {
            if (pattern != null)
                yield return pattern.Execute(gun);

            yield return new WaitForSeconds(delayBetween);
        }
    }

    public override void DrawGizmos(Gun gun)
    {
        foreach (var pattern in sequence)
        {
            pattern?.DrawGizmos(gun);
        }
    }
}






