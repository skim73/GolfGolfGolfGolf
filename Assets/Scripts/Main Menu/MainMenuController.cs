using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenuController : MonoBehaviour
{
	public GameObject main_screen;
	public GameObject host_game_screen;
	public GameObject join_game_screen;
	public GameObject settings_screen;
	public GameObject credits_screen;

	void Update()
	{
		if (Cursor.visible == false || Cursor.lockState != CursorLockMode.None)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void Awake()
	{
		DisplayMain();
	}

	public void StartGame()
	{
		SceneManager.LoadScene("");
	}

	public void DisplaySettings()
	{
		main_screen.SetActive(false);
		credits_screen.SetActive(false);
		settings_screen.SetActive(true);
		host_game_screen.SetActive(false);
		join_game_screen.SetActive(false);
	}

	public void DisplayHostGame()
	{
		main_screen.SetActive(false);
		credits_screen.SetActive(false);
		settings_screen.SetActive(false);
		host_game_screen.SetActive(true);
		join_game_screen.SetActive(false);
	}
	public void DisplayJoinGame()
	{
		main_screen.SetActive(false);
		credits_screen.SetActive(false);
		settings_screen.SetActive(false);
		host_game_screen.SetActive(false);
		join_game_screen.SetActive(true);
	}

	public void DisplayMain()
	{
		main_screen.SetActive(true);
		credits_screen.SetActive(false);
		settings_screen.SetActive(false);
		host_game_screen.SetActive(false);
		join_game_screen.SetActive(false);
	}

	public void DisplayCredits()
	{
		main_screen.SetActive(false);
		credits_screen.SetActive(true);
		settings_screen.SetActive(false);
		host_game_screen.SetActive(false);
		join_game_screen.SetActive(false);
	}

	public void Volume()
	{

	}

	public void ExitToDesktop()
	{
		Application.Quit();
	}
}