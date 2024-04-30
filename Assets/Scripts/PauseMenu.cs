using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public static bool isPaused;  // This variable tracks if the game is paused.

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        isPaused = false;  // Ensure that isPaused is properly initialized.
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Escape key pressed.");  // Debug statement to check if the key press is detected
            if (isPaused)
            {
                Debug.Log("Game is currently paused. Resuming game...");  // Debug output before resuming game
                ResumeGame();
            }
            else
            {
                Debug.Log("Game is not paused. Pausing game...");  // Debug output before pausing game
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;  // Stops all time-based operations in the game.
        isPaused = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Game paused. Time scale set to 0. Cursor unlocked.");  // Confirm game pause settings
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;  // Resumes all time-based operations at normal speed.
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Game resumed. Time scale set to 1. Cursor locked.");  // Confirm game resume settings
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;  // Ensure that the game time is normalized before switching scenes.
        SceneManager.LoadScene("MainMenu");  // Ensure this is the correct name of your main menu scene.
        Debug.Log("Loading main menu.");  // Debugging main menu loading
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game.");  // Debug before quitting game
        Application.Quit();  // Exits the game. Note that this will only work in a built version, not in the Unity editor.
    }
}