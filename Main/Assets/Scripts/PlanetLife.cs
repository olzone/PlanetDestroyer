using UnityEngine;
using System.Collections;

public class PlanetLife : MonoBehaviour {
    [Tooltip("Sun that this planet orbits")]
    public GameObject sol = null;
    [Tooltip("Diffuse reflectivity or reflecting power of a surface. Larger values mean colder planet. Earth albedo is 0.29. Value should be in range between 0.1 and 0.4")]
    [Range(0.1f, 0.4f)]
    public float albedo = 0.29f;
    [Tooltip("How much radiation warms the planet. This makes planet hotter")]
    [Range(0.0f, 100.0f)]
    public float greenhouse_effect = 0.0f;

    private float planet_to_sun_distance = 0;
    private float planet_temperature = 10.0f; // kelvin
    private const float SUN_LUMOSITY = 3.846e+26F;
    private const float BOLTZMANN_CONSTANT = 5.670373e-8F;
    private const float UNITY_DISTANCE_CONSTANT = 2.98e+9F;
    // Use this for initialization
    void Start () {
        SimpleKeplerOrbit kepler_orbit = GetComponent<SimpleKeplerOrbit>();
        if(kepler_orbit != null)
        {
            planet_to_sun_distance = kepler_orbit.aphelion;
        }
        planet_to_sun_distance *= UNITY_DISTANCE_CONSTANT;

        Transform[] children = sol.transform.GetComponentsInChildren<Transform>();
        float sol_current_lumosity = 0.0f;
        foreach (Transform t in children)
            if (t.gameObject.name == "Lumosity")
            {
                Light light = t.GetComponent<Light>();
                if(light != null)
                {
                    sol_current_lumosity = light.range;
                }
                else
                {
                    sol_current_lumosity = 0.0f; //no sun?
                }
            }

        float sun_scaled_lumosity = SUN_LUMOSITY;
        planet_temperature = Mathf.Pow((sun_scaled_lumosity * (1 - albedo)) / (16 * Mathf.PI * planet_to_sun_distance * planet_to_sun_distance * BOLTZMANN_CONSTANT), 0.25f);
        planet_temperature += greenhouse_effect;
        Debug.Log("Temperature : " + planet_temperature);

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
