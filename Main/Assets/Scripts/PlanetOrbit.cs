using UnityEngine;
using System.Collections;

public class PlanetOrbit : MonoBehaviour {

    public float rotate_speed = -25.0f;
    public float orbit_speed = 10.0f;


    // Use this for initialization
    void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(transform.up * rotate_speed * Time.deltaTime);
        transform.RotateAround(Vector3.zero, Vector3.up, orbit_speed * Time.deltaTime);
    }
}
