using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenUI : Singleton<LoadingScreenUI>
{
    public static bool IS_LOADING_COMPLETE = false;

    [SerializeField]
    private Slider m_LoadingSlider;

    private IUIVisibilityController m_VisibilityController;
    private IUIVisibilityController visibilityController
    {
        get
        {
            if (m_VisibilityController == null)
            {
                m_VisibilityController = GetComponentInChildren<IUIVisibilityController>();
            }
            return m_VisibilityController;
        }
    }

    private static IEnumerator Load_CR(IAsyncTask loadingAsyncTask, float minDuration)
    {
        ShowImmediately();
        var timeSinceStart = Time.time;
        while (!loadingAsyncTask.isCompleted)
        {
            Instance.m_LoadingSlider.value = loadingAsyncTask.percentageComplete * 0.5f;
            yield return null;
        }
        var max = timeSinceStart + minDuration;
        var min = Time.time;
        var current = Time.time;
        while (current < max)
        {
            current = Time.time;
            Instance.m_LoadingSlider.value = 0.5f + Mathf.InverseLerp(min, max, current) * 0.5f;
            yield return null;
        }
        HideImmediately();
        IS_LOADING_COMPLETE = true;
    }

    public static void Load(IAsyncTask loadingAsyncTask, float minDuration = AnimationDuration.LONG)
    {
        Instance.StartCoroutine(Load_CR(loadingAsyncTask, minDuration));
    }

    public static void Show()
    {
        Instance.visibilityController.Show();
    }

    public static void ShowImmediately()
    {
        Instance.visibilityController.ShowImmediately();
        Instance.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        Instance.visibilityController.Hide();
    }

    public static void HideImmediately()
    {
        Instance.visibilityController.HideImmediately();
        Instance.gameObject.SetActive(false);
    }
}