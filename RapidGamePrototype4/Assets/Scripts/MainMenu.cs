﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnPlayButtonPressed()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
