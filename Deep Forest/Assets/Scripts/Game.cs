using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Game : MonoBehaviour {

    /* 
        To do: 
            - Spawning of player and the guard as game is prepared
    */
    public Canvas looseScreen;
    public Text looseText;
    public Button exitButton;

    public GameObject terrainPrefab;
    GameObject terrain;

    public Text levelCounter;
    
    int level = 0;
    TerrainGenerator terrainGen;

    // Start the level
    void Start () {
        looseScreen = looseScreen.GetComponent<Canvas>();
        looseText = looseText.GetComponent<Text>();
        exitButton = exitButton.GetComponent<Button>();
        looseScreen.enabled = false;
        levelCounter = levelCounter.GetComponent<Text>();
        UpdateLevelCount();
        terrain = Instantiate(terrainPrefab);
        terrainGen = terrain.GetComponent<TerrainGenerator>();
    }

    void UpdateLevelCount()
    {
        level++;
        levelCounter.text = "LEVEL " + level;
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        looseText.text = "YOU LOOSE\n\n YOU FOUND THE EXIT " + (level - 1) + " TIME(S)";

        looseScreen.enabled = true;
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NextLevel()
    {
        //terrainGen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
        terrainGen.Generate();
        //Debug.Log("Current Level: " + level);
        UpdateLevelCount();
    }
}
