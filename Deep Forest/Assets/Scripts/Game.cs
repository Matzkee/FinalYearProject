using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Game : MonoBehaviour {
    
    public Canvas looseScreen;
    public Text looseText;
    public Text debugText;
    public Text levelCounter;
    public Button exitButton;
    public KeyCode debugKey;

    public GameObject terrainPrefab;
    GameObject terrain;

    bool debugActive = false;
    int level = 0;
    TerrainGenerator terrainGen;

    // Start the level
    void Start () {
        Time.timeScale = 1;
        looseScreen = looseScreen.GetComponent<Canvas>();
        looseText = looseText.GetComponent<Text>();
        debugText = debugText.GetComponent<Text>();
        exitButton = exitButton.GetComponent<Button>();
        looseScreen.enabled = false;
        levelCounter = levelCounter.GetComponent<Text>();
        UpdateLevelCount();
        terrain = Instantiate(terrainPrefab);
        terrainGen = terrain.GetComponent<TerrainGenerator>();

        debugText.text = "PRESS " + debugKey.ToString() + " FOR DEBUG INFORMATION";
    }

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
        {
            if (terrainGen != null)
            {
                if (!debugActive)
                {
                    debugText.text = "TOTAL NUMBER OF TREES  "+ terrainGen.treeCount +
                        "\nTOTAL NUMBER OF WEEDS  " + terrainGen.weedCount + 
                        "\nTOTAL NUMBER OF WALL TILES  " + terrainGen.wallTileCount + 
                        "\nPRESS " + debugKey.ToString() + " AGAIN TO HIDE THIS";
                    debugActive = true;
                }
                else
                {
                    debugText.text = "PRESS " + debugKey.ToString() + " FOR DEBUG INFORMATION";
                    debugActive = false;
                }
            }
        }
    }

    void UpdateLevelCount()
    {
        level++;
        levelCounter.text = "LEVEL " + level;
    }

    public void GameOver()
    {
        // Pause the game, display text & canvas
        Time.timeScale = 0;
        looseText.text = "YOU LOOSE\n\n YOU FOUND THE EXIT " + (level - 1) + " TIMES";

        looseScreen.enabled = true;
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void NextLevel()
    {
        terrainGen.Generate();
        UpdateLevelCount();
    }
}
