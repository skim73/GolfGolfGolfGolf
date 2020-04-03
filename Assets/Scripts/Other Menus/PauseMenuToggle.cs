using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuToggle : MonoBehaviour
{
    private CanvasGroup pauseMenu, settings;
    void Awake()
    {
        pauseMenu = GetComponentsInChildren<CanvasGroup>()[0];
        settings = GetComponentsInChildren<CanvasGroup>()[1];

        if (pauseMenu.name != "PausedMenu") {
            CanvasGroup temp = pauseMenu;
            pauseMenu = settings;
            settings = temp;
        }

        pauseMenu.interactable = false;
        pauseMenu.blocksRaycasts = false;
        pauseMenu.alpha = 0f;
        settings.interactable = false;
        settings.blocksRaycasts = false;
        settings.alpha = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (pauseMenu.blocksRaycasts) {
                if (!settings.interactable) {
                    pauseMenu.interactable = false;
                    pauseMenu.blocksRaycasts = false;
                    pauseMenu.alpha = 0f;
                } else {
                    CloseSettingsClicked();
                }
            } else {
                pauseMenu.interactable = true;
                pauseMenu.blocksRaycasts = true;
                pauseMenu.alpha = 1f;
            }
        }
    }

    public void OnSettingsClicked()
    {
        pauseMenu.interactable = false;
        settings.interactable = true;
        settings.blocksRaycasts = true;
        settings.alpha = 1f;
    }

    public void CloseSettingsClicked()
    {
        pauseMenu.interactable = true;
        settings.interactable = false;
        settings.blocksRaycasts = false;
        settings.alpha = 0f;
    }

    public void OnReturnHomeClicked()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnQuitClicked()
    {
        Debug.Log("Game quit successfully!");
        Application.Quit();
    }
}
