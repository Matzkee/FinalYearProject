using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {

    /* 
        To do: 
            - Look if player is dead and if so end the game
            - Look if player has reached to finish object in the forest and redo the level
            - Spawning of player and the guard as game is prepared
            - level counting / scoring system
    */
    public GameObject terrain;
    //public GameObject player;
    //public GameObject guard;
    
    int level;
    TerrainGenerator terrainGen;

    void Start () {
        Instantiate(terrain);
    }
	
	void Update () {
	    
	}

    public void GameOver()
    {

    }

    public void NextLevel()
    {
        level++;
        terrainGen = GameObject.FindGameObjectWithTag("TerrainGenerator").GetComponent<TerrainGenerator>();
        terrainGen.Generate();
        Debug.Log("Current Level: " + level);
    }
}
