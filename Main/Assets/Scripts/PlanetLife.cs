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
    [Tooltip("How much flora there is on the planet. Higher values mean more trees")]
    [Range(0.0f, 1.0f)]
    public float forestation = 0.5f;

    private float planet_to_sun_distance = 0;
    private const float SUN_LUMOSITY = 3.846e+26F;
    private const float BOLTZMANN_CONSTANT = 5.670373e-8F;
    private const float UNITY_DISTANCE_CONSTANT = 2.98e+9F;

    /**
        Calculates planet temperature based on advanced equation.
    */
    float calculate_temperature()
    {
        SimpleKeplerOrbit kepler_orbit = GetComponent<SimpleKeplerOrbit>();
        if (kepler_orbit != null)
        {
            planet_to_sun_distance = kepler_orbit.aphelion;
        }
        else
        {
            return greenhouse_effect;
        }
        planet_to_sun_distance *= UNITY_DISTANCE_CONSTANT;

        Transform[] children = sol.transform.GetComponentsInChildren<Transform>();
        float sol_current_lumosity = 0.0f;
        foreach (Transform t in children)
            if (t.gameObject.name == "Lumosity")
            {
                Light light = t.GetComponent<Light>();
                if (light != null)
                {
                    sol_current_lumosity = light.range;
                }
                else
                {
                    sol_current_lumosity = 0.0f; //no sun?
                }
            }

        float sun_scaled_lumosity = SUN_LUMOSITY;
        float planet_temperature = Mathf.Pow((sun_scaled_lumosity * (1 - albedo)) / (16 * Mathf.PI * planet_to_sun_distance * planet_to_sun_distance * BOLTZMANN_CONSTANT), 0.25f);
        planet_temperature += greenhouse_effect;
        return planet_temperature;
    }

    /**
        Converts geocentric coordinates to cartesian coordinates
    */
    Vector3 geocentric_to_cartesian(float longitude, float lattitude)
    {

        longitude = longitude * Mathf.Deg2Rad;
        lattitude = lattitude * Mathf.Deg2Rad;

        float sin_lattitude = Mathf.Sin(lattitude);
        float x = Mathf.Cos(longitude) * sin_lattitude;
        float y = Mathf.Sin(longitude) * sin_lattitude;
        float z = Mathf.Cos(lattitude);
        return new Vector3(x, y, z); 
    }

    /**
        Places any object on planet on geocentric coordinates. Returs 0 if the script failed to place object and 1 if object was placed.
    */
    int place_object_on_planet(GameObject obj, SubdivisionSurfacesPlanet planet, float longitude, float latitude, string name)
    {
        int status = 0;

        float frequency = planet.terrain_noise_frequency;
        Vector3 cartesian_vector = geocentric_to_cartesian(longitude, latitude);
        float perlin_amplitude = SimplexNoise.simplex3D(cartesian_vector * frequency);
        if (perlin_amplitude > planet.ocean_height && perlin_amplitude < planet.get_mountain_height())
        {
            status = 1;
            float height_value = (planet.planet_radii + SimplexNoise.simplex3D(cartesian_vector * frequency) * planet.terrain_height - obj.transform.lossyScale.y * 1.5f);
            Vector3 position = gameObject.transform.position + height_value * cartesian_vector;
            Quaternion rotation = Quaternion.LookRotation(cartesian_vector);
            rotation *= Quaternion.Euler(90, 0, 0);
            GameObject _obj = (GameObject)Instantiate(obj, position, rotation);
            _obj.name = name;
            _obj.transform.parent = gameObject.transform;
        }
        return status;
    }

    void Start () {

        /*
        float temperature = calculate_temperature();
        Transform[] children = GetComponentsInChildren<Transform>();
        int cnt = 0;
        foreach(Transform child in children)
        {
            Mesh mesh = child.gameObject.GetComponent<MeshFilter>().mesh;
            cnt += 1;

            Debug.Log(mesh.name + " " + mesh.colors.Length);

            if (mesh != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    Debug.Log(i);
                }
            }
        }
        Debug.Log(cnt);
        */

        GameObject tree = Resources.Load("Tree_01_autumn_pal") as GameObject;
        if(tree == null)
        {
            Debug.Log("Resource not found!");
        }
        float scale = 0.04f;
        tree.transform.localScale = Vector3.one * scale;
        tree.transform.Rotate(0.0f, 0.0f, 90.0f);
        int number_of_trees = (int) (1000 * forestation);
        int counter = 0;
        SubdivisionSurfacesPlanet planet = GetComponent<SubdivisionSurfacesPlanet>();
        while(counter < number_of_trees)
        {
            float rand_longitude = Random.Range(0.0f, 359.0f);
            float rand_lattitude = Random.Range(0.0f, 179.0f);
            
            counter += place_object_on_planet(tree, planet, rand_longitude, rand_lattitude, "Tree" + counter.ToString());
            
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
