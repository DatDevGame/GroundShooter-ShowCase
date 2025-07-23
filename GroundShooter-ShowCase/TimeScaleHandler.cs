using UnityEngine;

public class TimeScaleHandler : MonoBehaviour
{
    public static TimeScaleHandler Instance { get; private set; }

    public bool IsPaused => Mathf.Approximately(Time.timeScale, 0f);
    public float CurrentTimeScale { get; private set; } = 1f;

    [SerializeField] private bool m_StartPaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetTimeScale(m_StartPaused ? 0f : 1f);
    }

    public void Pause()
    {
        SetTimeScale(0f);
    }

    public void Resume()
    {
        SetTimeScale(CurrentTimeScale == 0f ? 1f : CurrentTimeScale);
    }

    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void SetTimeScale(float value)
    {
        value = Mathf.Clamp(value, 0f, 10f);
        CurrentTimeScale = value;
        Time.timeScale = value;
        Time.fixedDeltaTime = 0.02f * value;
    }

    public void SetTimeScaleNormalized(float normalized)
    {
        SetTimeScale(Mathf.Lerp(0f, 1f, normalized));
    }
}
