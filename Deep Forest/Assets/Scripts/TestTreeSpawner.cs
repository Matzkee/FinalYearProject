using UnityEngine;
using System.Collections;

public class TestTreeSpawner : MonoBehaviour {

    public GameObject prefab;

    Vector3[] terrainVerts;

	// Use this for initialization
	void Start () {
	}
	
	public void SpawnTree()
    {
        terrainVerts = gameObject.GetComponent<MeshFilter>().mesh.vertices;
        GameObject tree = (GameObject) Instantiate(
            prefab, 
            terrainVerts[Random.Range(1, terrainVerts.Length)], 
            transform.rotation);
        tree.transform.Rotate(Vector3.up * Random.Range(0,360));
    }
}
