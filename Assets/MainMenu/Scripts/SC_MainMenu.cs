﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SC_MainMenu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject CreditsMenu;



    // Start is called before the first frame update
    void Start()
    {
        MainMenuButton();
        Cursor.lockState = CursorLockMode.None;  
        Cursor.visible = true;
    }

    public void PlayNowButton()
    {
        // Play Now Button has been pressed, here you can initialize your game (For example Load a Scene called GameLevel etc.)
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLevel");
    }

    public void CreditsButton()
    {
        // Show Credits Menu
        MainMenu.SetActive(false);
        CreditsMenu.SetActive(true);
    }

    public void MainMenuButton()
    {
        // Show Main Menu
        MainMenu.SetActive(true);
        CreditsMenu.SetActive(false);
    }

    public void QuitButton()
    {
#if UNITY_EDITOR
        // Stop playing the scene in Unity editor
        EditorApplication.isPlaying = false;
#else
        // Quit the application
        Application.Quit();
#endif
    }
}