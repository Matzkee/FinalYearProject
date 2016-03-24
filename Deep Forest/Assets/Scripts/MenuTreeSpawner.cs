using UnityEngine;
using System.Collections;

public class MenuTreeSpawner : MonoBehaviour {

    public GameObject[] trees;

	void Start () {
        // Spawn a random tree with random rotation
        int randomIndex = Random.Range(0, trees.Length);
        GameObject randomTree = Instantiate(trees[randomIndex]);
        randomTree.transform.position = transform.position;
        randomTree.transform.Rotate(Vector3.up, Random.Range(0, 360));
	}
	
}
