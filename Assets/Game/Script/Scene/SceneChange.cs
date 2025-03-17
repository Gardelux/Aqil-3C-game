using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
    [Header("Script")]
    [SerializeField] InputManager input;

    [Header("Component")]
    [SerializeField] Canvas pauseUI;

    // Start is called before the first frame update
    void Start()
    {
        if(input != null)
        {
            input.OnPause += PauseSimulator;
        }
    }

    private void OnDestroy()
    {
        if(input != null)
        {
            input.OnPause -= PauseSimulator;
        }
    }

    public void PauseSimulator()
    {
        if(pauseUI != null)
        {
            if (pauseUI.isActiveAndEnabled)
            {
                pauseUI.gameObject.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            else
            {
                pauseUI.gameObject.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GamePlay");
        if(Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
