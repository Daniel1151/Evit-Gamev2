using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public bool isPaused;  // This variable tracks if the game is paused.

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
        //isPaused = false;  // Ensure that isPaused is properly initialized.
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                //Debug.Log("Pause menu active state"
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        //Cursor.lockState = CursorLockMode.None;
        //Cursor.visible = true;
    }

    public void ResumeGame()
    {
        //Debug.Log("Disabling pause menu.");
        pauseMenu.SetActive(false);
        //Debug.Log("Pause menu active state after disabling: " + pauseMenu.activeSelf);  // Verify it is deactivated
        Time.timeScale = 1f;
        isPaused = false;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;  // Ensure that the game time is normalized before switching scenes.
        SceneManager.LoadScene("GameLevel");  // Ensure this is the correct name of your main menu scene.
        //Debug.Log("Loading main menu.");  // Debugging main menu loading
    }

    public void QuitGame()
    {
        //Debug.Log("Quitting game.");  // Debug before quitting game
        Application.Quit();  // Exits the game. Note that this will only work in a built version, not in the Unity editor.
    }
}