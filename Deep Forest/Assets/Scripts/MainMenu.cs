using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenu : MonoBehaviour {
    
    public Button newGameButton;
    public Button exitButton;

	void Start () {
        newGameButton = newGameButton.GetComponent<Button>();
        exitButton = exitButton.GetComponent<Button>();
	}
	
    public void StartLevel()
    {
        SceneManager.LoadScene("Game");
    }
	
    public void Exit()
    {
        Application.Quit();
    }

}
