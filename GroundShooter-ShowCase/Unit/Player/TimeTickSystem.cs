using HyrphusQ.Events;
using UnityEngine;

[EventCode]
public enum TimeTickSystemEventCode
{
    OnHalfSecondTick,
    OnOneSecondTick,
}
public class TimeTickSystem : MonoBehaviour
{
    float tickTimer;
    bool hasTickHalf;
    void Update()
    {
        tickTimer += Time.deltaTime;
        if (!hasTickHalf && tickTimer >= 0.5f)
        {
            GameEventHandler.Invoke(TimeTickSystemEventCode.OnHalfSecondTick);
            hasTickHalf = true;
        }
        if (tickTimer >= 1)
        {
            GameEventHandler.Invoke(TimeTickSystemEventCode.OnOneSecondTick);
            GameEventHandler.Invoke(TimeTickSystemEventCode.OnHalfSecondTick);
            tickTimer = 0;
            hasTickHalf = false;
        }
    }
}