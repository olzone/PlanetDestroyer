using UnityEngine;
using System.Collections;

public class SimpleKeplerOrbit : MonoBehaviour
{
    [Tooltip("How far away the planet is from the sun, 50 corresponds to one astronomical unit")]
    [Range(10.0f, 200.0f)]
    public float aphelion = 50.0f; //50 = 1AU, distance from the sun affects planet temperature and flora
    [Tooltip("This is axial tilt of the planet in degrees, Earth's axial tilt is on average 23 degrees")]
    [Range(0.0f, 45.0f)]
    public float axis_tilt = 0.0f;
    [Tooltip("This is T^2/R^3 ratio. This ratio is almost always equals 1.0. See Kepler's Third Law for more details")]
    [Range(0.8f, 1.2f)]
    public float kepler_ratio = 1.0f;
    [Tooltip("How fast planet spins around its axis, 1.0 is default")]
    [Range(0.0f, 5.0f)]
    public float axis_rotation_speed = 1.0f;
    [Tooltip("Sun that this planet orbits")]
    public GameObject orbit_target = null;

    private float orbital_period = 0.0f;
    private float orbit_perimeter = 0.0f;
    private float degree_increment = 0.0f;
    private Vector3 axis_tilt_vector;

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
        float axis_tilt_in_radians = (axis_tilt * Mathf.PI) / 180.0f;
        axis_tilt_vector = new Vector3(Mathf.Sin(axis_tilt_in_radians), Mathf.Cos(axis_tilt_in_radians), 0.0f);
        //Debug.Log(axis_tilt_vector);
        //Debug.Log(orbit_target.transform.position);
    }
	
	void Update ()
    {
        transform.Rotate(axis_tilt_vector * axis_rotation_speed * Time.deltaTime*2);
        transform.RotateAround(orbit_target.transform.position, Vector3.up, degree_increment * Time.deltaTime);
    }
}
