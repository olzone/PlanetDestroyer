using UnityEngine;
using System.Collections;

public class SimpleKeplerOrbit : MonoBehaviour
{

    public float aphelion = 25.0f;
    public float kepler_ratio = 1.0f;
    public float axis_rotation_speed = 1.0f;
    public GameObject orbit_target = null;

    private float orbital_period = 0.0f;
    private float orbit_perimeter = 0.0f;
    private float degree_increment = 0.0f;

	void Start ()
    {
        float rand_angle = Random.Range(0.0f, Mathf.PI * 2);
        Vector3 aphelion_vector = new Vector3(Mathf.Cos(rand_angle)*aphelion, 0.0f, Mathf.Sin(rand_angle) * aphelion);
        //Vector3 aphelion_vector = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 object_position = orbit_target.transform.position + aphelion_vector;
        orbital_period = Mathf.Sqrt(aphelion * aphelion * aphelion * kepler_ratio);
        orbit_perimeter = Mathf.PI * 2 * aphelion;
        degree_increment = orbit_perimeter / orbital_period;
        transform.position = object_position;
        //Debug.Log(orbit_target.transform.position);
    }
	
	void Update ()
    {
        transform.Rotate(transform.up * axis_rotation_speed * Time.deltaTime);
        transform.RotateAround(orbit_target.transform.position, Vector3.up, degree_increment * Time.deltaTime);
    }
}
