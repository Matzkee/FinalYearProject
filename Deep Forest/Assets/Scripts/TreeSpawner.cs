using UnityEngine;
using System.Collections;

public class TreeSpawner : MonoBehaviour {

    public GameObject seed = null;

    public float speed = 10.0f;
    float fireRate = 0.5f;
    float lastSpawn = 0.0f;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0))
        {
            Spawn();
        }
	}

    void Spawn()
    {
        if (Time.time > fireRate + lastSpawn)
        {
            GameObject seedClone = (GameObject)Instantiate(seed,
                    transform.position + (transform.forward * 2), transform.rotation);
            seedClone.GetComponent<Rigidbody>().AddForce(transform.forward * speed);
            lastSpawn = Time.time;
        }
    }
}
