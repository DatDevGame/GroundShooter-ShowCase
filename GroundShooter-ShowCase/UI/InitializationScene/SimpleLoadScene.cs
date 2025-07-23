using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.GameManagement;
using UnityEngine;
public class SimpleLoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoadingScreenUI.Load(SceneManager.LoadSceneAsync(SceneName.MainScene, isPushToStack: false));
    }
}
