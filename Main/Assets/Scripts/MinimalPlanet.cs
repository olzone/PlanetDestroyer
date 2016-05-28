/**
Author: Andrzej Czerniewski
Code implemented from scratch. Generates procedural planet with minimalistic style using subdivision surfaces of platonic solids, 
simulates kepler orbital motion and populates planet based on planet temperature
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinimalPlanet : MonoBehaviour {

    struct TemperatureGradient
    {
        public Color color;
        public float temperature;

        public TemperatureGradient(Color _color, float _temperature)
        {
            color = _color;
            temperature = _temperature;
        }
    }

    //Script parameters
    [Header("Planet terrain and topology")]
    [Tooltip("Basic seed. Change this value to randomize planet.")]
    [Range(0, 65536)]
    public int seed = 32768;

    [Tooltip("Number of subdivisions. More subdivisions means more detailed planet but this in turn introduces more chunks. Default is 7")]
    [Range(4, 8)]
    public int number_of_subdivisions = 7; //each subdivision level yields 4^(n+1) triangles

    [Tooltip("Radius of the planet. Default is 2.0f")]
    [Range(1.0f, 3.0f)]
    public float planet_radii = 2;

    [Tooltip("Height of the ocean. Default is 0.1f")]
    [Range(-1.0f, 0.1f)]
    public float ocean_height = 0.05f;

    [Tooltip("The lower the value the less mountains you will get and terrain will be smoother. Default is 4.0f")]
    [Range(1.0f, 10.0f)]
    public float terrain_noise_frequency = 4.0f;

    [Tooltip("Terrain height multiplier. Higher values generate taller and steeper mountains. Default is 0.05f")]
    [Range(0.01f, 0.1f)]
    public float terrain_height = 0.05f;


    [Header("Parameters of orbital movement")]
    [Tooltip("Sun that this planet orbits")]
    public GameObject orbit_target = null;

    [Tooltip("How far away the planet is from the sun, 50 corresponds to one astronomical unit. Default is 1.0f")]
    [Range(0.25f, 5.0f)]
    public float aphelion_AU = 1.0f; //50 = 1AU, distance from the sun affects planet temperature and flora

    [Tooltip("This is axial tilt of the planet in degrees, Earth's axial tilt is on average 23 degrees. Default is 0.0f")]
    [Range(0.0f, 45.0f)]
    public float axis_tilt = 0.0f;

    [Tooltip("This is T^2/R^3 ratio. This ratio is almost always equals 1.0. See Kepler's Third Law for more details. Default is 1.0f")]
    [Range(0.8f, 1.2f)]
    public float kepler_ratio = 1.0f;

    [Tooltip("How fast planet spins around its axis. Default is 1.0f")]
    [Range(0.0f, 5.0f)]
    public float axis_rotation_speed = 1.0f;


    [Header("Planet life parameters")]
    [Tooltip("Enables or disables flora on the planet. Default is true.")]
    public bool flora_enabled = true;

    [Tooltip("Diffuse reflectivity or reflecting power of a surface. Larger values mean colder planet. Earth albedo is 0.29. Value should be in range between 0.1 and 0.4. Default is 0.29f")]
    [Range(0.1f, 0.4f)]
    public float albedo = 0.29f;

    [Tooltip("How much radiation warms the planet. This makes planet hotter with thicker atmosphere. Default is 0.0f")]
    [Range(0.0f, 100.0f)]
    public float greenhouse_effect = 0.0f;

    [Tooltip("How much flora there is on the planet. Higher values mean more trees. Default is 0.5f")]
    [Range(0.0f, 1.0f)]
    public float forestation = 0.5f;

    [Tooltip("Number of cities. Default is 3")]
    [Range(0, 5)]
    public int cities_amount = 3;


    [Header("Misc parameters")]
    [Tooltip("Adds small randomness to the color")]
    [Range(0.01f, 0.1f)]
    public float color_randomness = .05f;

    //General constants
    private const int MAX_VERTICES = 49152; //DON'T CHANGE THAT!
    private const float SUN_LUMOSITY = 3.846e+26F;
    private const float BOLTZMANN_CONSTANT = 5.670373e-8F;
    private const float UNITY_DISTANCE_CONSTANT = 2.98e+9F;
    private const float UNITY_SOL_LUMOSITY_RATIO = 0.005f;
    private const float UNITY_FLORA_SCALE_FACTOR = 0.04f;
    private const float UNITY_TO_AU_DISTANCE_RATIO = 50.0f;
    private const float PLANET_FLORA_AMOUNT = 250.0f;
    private const float MOUNTAINS_HEIGHT = .95f;
    private const float HIGHLANDS_HEIGHT = .7f;

    //Precomputed mathematical values for faster computation
    private float negative_root_two_over_three = -Mathf.Sqrt(2) / 3.0f;
    private float negative_one_third = -1.0f / 3.0f;
    private float root_six_over_three = Mathf.Sqrt(6) / 3.0f;
    private Vector2 latitude_vector = new Vector2(1, 0);

    //Orbital parameters
    private float orbital_period = 0.0f;
    private float orbit_perimeter = 0.0f;
    private float degree_increment = 0.0f;
    private Vector3 axis_tilt_vector;

    //Flora and temperature constants
    private List<GameObject> winter_flora = new List<GameObject>();
    private List<GameObject> temperate_flora = new List<GameObject>();
    private List<GameObject> tropical_flora = new List<GameObject>();
    private List<GameObject> buildings = new List<GameObject>();
    private float planet_temperature = 0.0f;

    private const float COLD_BARRIER = 193.15f; //it is too cold for life below this point
    private const float TEMPERATE_BARRIER = 273.15f;
    private const float TROPICAL_BARRIER = 323.15f;
    private const float HOT_BARRIER = 353.15f; //it is too hot for life above this point
    private const float VARIATION_HALF_RANGE = 25.0f;

    //Polygon colors based on height
    private TemperatureGradient[] water_gradients_colors =
    {
        new TemperatureGradient(new Color(0.0f   / 255.0f, 44.0f  / 255.0f, 59.0f  / 255.0f), COLD_BARRIER - VARIATION_HALF_RANGE), //dark frozen ocean
        new TemperatureGradient(new Color(242.0f / 255.0f, 255.0f / 255.0f, 256.0f / 255.0f), TEMPERATE_BARRIER - 10.0f), //frozen ocean
        new TemperatureGradient(new Color(0.0f   / 255.0f, 191.0f / 255.0f, 255.0f / 255.0f), TEMPERATE_BARRIER), //temperate ocean
        new TemperatureGradient(new Color(23.0f  / 255.0f, 255.0f / 255.0f, 232.0f / 255.0f), HOT_BARRIER), //tropical ocean
        new TemperatureGradient(new Color(128.0f / 255.0f, 77.0f  / 255.0f, 1.0f   / 255.0f), HOT_BARRIER + 50.0f), //dry ocean
        new TemperatureGradient(new Color(179.0f / 255.0f, 0.0f   / 255.0f, 0.0f   / 255.0f), HOT_BARRIER + 200.0f) //lava ocean
    };

    private TemperatureGradient[] grass_gradients_colors =
    {
        new TemperatureGradient(new Color(81.0f  / 255.0f, 89.0f  / 255.0f, 89.0f  / 255.0f), COLD_BARRIER - VARIATION_HALF_RANGE), //dark frozen stone
        new TemperatureGradient(new Color(232.0f / 255.0f, 254.0f / 254.0f, 255.0f / 255.0f), TEMPERATE_BARRIER - 10.0f), //snow grass
        new TemperatureGradient(new Color(107.0f / 255.0f, 142.0f / 255.0f, 35.0f  / 255.0f), TEMPERATE_BARRIER), //temperate grass
        new TemperatureGradient(new Color(250.0f / 255.0f, 214.0f / 255.0f, 10.0f  / 255.0f), HOT_BARRIER), //tropical sand
        new TemperatureGradient(new Color(94.0f /  255.0f, 94.0f  / 255.0f, 86.0f  / 255.0f), HOT_BARRIER + 50.0f), //dry wasteland
        new TemperatureGradient(new Color(37.0f /  255.0f, 3.0f   / 255.0f, 69.0f  / 255.0f), HOT_BARRIER + 200.0f) //obsidian
    };

    private TemperatureGradient[] highlands_gradients_colors =
    {
        new TemperatureGradient(new Color(46.0f  / 255.0f, 51.0f  / 255.0f, 51.0f  / 255.0f), COLD_BARRIER - VARIATION_HALF_RANGE), //dark rocks
        new TemperatureGradient(new Color(58.0f  / 255.0f, 64.0f  / 254.0f, 64.0f  / 255.0f), TEMPERATE_BARRIER - 10.0f), //dark rocks
        new TemperatureGradient(new Color(205.0f / 255.0f, 133.0f / 255.0f, 63.0f / 255.0f), TEMPERATE_BARRIER), //temperate dirt
        new TemperatureGradient(new Color(44.0f  / 255.0f, 77.0f  / 255.0f, 16.0f  / 255.0f), HOT_BARRIER), //tropical dirt
        new TemperatureGradient(new Color(71.0f /  255.0f, 71.0f  / 255.0f, 65.0f  / 255.0f), HOT_BARRIER + 50.0f), //dry wasteland
        new TemperatureGradient(new Color(21.0f /  255.0f, 2.0f   / 255.0f, 38.0f  / 255.0f), HOT_BARRIER + 200.0f) //obsidian
    };

    private TemperatureGradient[] mountains_gradients_colors =
    {
        new TemperatureGradient(new Color(30.0f  / 255.0f, 33.0f  / 255.0f, 33.0f  / 255.0f), COLD_BARRIER - VARIATION_HALF_RANGE), //dark rocks
        new TemperatureGradient(new Color(181.0f / 255.0f, 241.0f / 254.0f, 255.0f / 255.0f), TEMPERATE_BARRIER - 10.0f), //ice summit
        new TemperatureGradient(new Color(255.0f / 255.0f, 250.0f / 255.0f, 255.0f / 255.0f), TEMPERATE_BARRIER), //temperate dirt
        new TemperatureGradient(new Color(71.0f  / 255.0f, 71.0f  / 255.0f, 71.0f  / 255.0f), HOT_BARRIER), //tropical dirt
        new TemperatureGradient(new Color(42.0f /  255.0f, 42.0f  / 255.0f, 42.0f  / 255.0f), HOT_BARRIER + 50.0f), //dry wasteland
        new TemperatureGradient(new Color(3.0f /  255.0f, 0.0f   / 255.0f,  3.0f  / 255.0f), HOT_BARRIER + 200.0f) //obsidian
    };


    //Misc parameters
    private bool parameters_converted = false;
    private bool flora_gameobjects_loaded = false;
    
    //Buffers for storing vertices generated using subdivision surfaces algorithm
    private List<Vector3> vertices_buffer = new List<Vector3>();
    private List<int> indices_buffer = new List<int>();

    /*
        Checks if all parameters are ok
    */
    private bool parameters_correct()
    {
        bool fault = true;

        if (parameters_converted)
        {
            Debug.Log("Parameters already converted");
            return fault;
        }

        if (orbit_target == null)
        {
            Debug.LogError("Orbit target not assigned!");
            fault = false;
        }

        parameters_converted = true;
        return fault;
    }

    /*
        Converts parameters to unity units
    */
    private void convert_parameters()
    {
        aphelion_AU *= UNITY_TO_AU_DISTANCE_RATIO;
    }

    /*
        Loads flora gameobjects
    */
    private void load_flora()
    {
        if (flora_gameobjects_loaded) return;

        //Load more if you wish
        winter_flora.Add(Resources.Load("bush_2_winter_pal") as GameObject);
        winter_flora.Add(Resources.Load("bush_5_winter_pal") as GameObject);
        winter_flora.Add(Resources.Load("Tree_01_winter_pal") as GameObject);
        winter_flora.Add(Resources.Load("Tree_05_winter_pal") as GameObject);
        winter_flora.Add(Resources.Load("Tree_20_winter_pal") as GameObject);

        temperate_flora.Add(Resources.Load("bush_2_summer_pal") as GameObject);
        temperate_flora.Add(Resources.Load("bush_5_summer_pal") as GameObject);
        temperate_flora.Add(Resources.Load("Tree_01_summer_pal") as GameObject);
        temperate_flora.Add(Resources.Load("Tree_05_summer_pal") as GameObject);
        temperate_flora.Add(Resources.Load("Tree_20_summer_pal") as GameObject);
        temperate_flora.Add(Resources.Load("Tree_28_summer_pal") as GameObject);

        tropical_flora.Add(Resources.Load("Tree_09_summer_pal") as GameObject);

        foreach (GameObject tree in winter_flora)
        {
            tree.transform.localScale = Vector3.one * UNITY_FLORA_SCALE_FACTOR;
            tree.transform.Rotate(0.0f, 0.0f, 90.0f);
        }


        foreach (GameObject tree in temperate_flora)
        {
            tree.transform.localScale = Vector3.one * UNITY_FLORA_SCALE_FACTOR;
            tree.transform.Rotate(0.0f, 0.0f, 90.0f);
        }


        foreach (GameObject tree in tropical_flora)
        {
            tree.transform.localScale = Vector3.one * UNITY_FLORA_SCALE_FACTOR;
            tree.transform.Rotate(0.0f, 0.0f, 90.0f);
        }

        flora_gameobjects_loaded = true;
    }


    /*
    Recursive subdivision call
    */
    private void subdivide(int[] triangle, int level)
    {
        if (level > 0)
        {
            //subdivide further
            vertices_buffer.Add(Vector3.Normalize((vertices_buffer[triangle[0]] + vertices_buffer[triangle[1]]) * .5f));
            vertices_buffer.Add(Vector3.Normalize((vertices_buffer[triangle[1]] + vertices_buffer[triangle[2]]) * .5f));
            vertices_buffer.Add(Vector3.Normalize((vertices_buffer[triangle[2]] + vertices_buffer[triangle[0]]) * .5f));

            int ind01 = vertices_buffer.Count - 3;
            int ind12 = vertices_buffer.Count - 2;
            int ind20 = vertices_buffer.Count - 1;

            level--;

            subdivide(new int[] { triangle[0], ind01, ind20 }, level);
            subdivide(new int[] { ind01, triangle[1], ind12 }, level);
            subdivide(new int[] { ind01, ind12, ind20 }, level);
            subdivide(new int[] { ind20, ind12, triangle[2] }, level);
        }
        else
        {
            indices_buffer.Add(triangle[0]);
            indices_buffer.Add(triangle[1]);
            indices_buffer.Add(triangle[2]);
        }
    }

    /**
     Generates planet using subdivision surfaces method for platonic solids
    */
    private void subdivision_surfaces_planet()
    {
        Vector3 tetrahedron_top = new Vector3(0, 0, 1);
        Vector3 tetrahedron_bottom1 = new Vector3(0, (float)2.0f * Mathf.Sqrt(2) / 3.0f, negative_one_third);
        Vector3 tetrahedron_bottom2 = new Vector3(-root_six_over_three, negative_root_two_over_three, negative_one_third);
        Vector3 tetrahedron_bottom3 = new Vector3(root_six_over_three, negative_root_two_over_three, negative_one_third);

        vertices_buffer.Add(tetrahedron_top);
        vertices_buffer.Add(tetrahedron_bottom1);
        vertices_buffer.Add(tetrahedron_bottom2);
        vertices_buffer.Add(tetrahedron_bottom3);

        subdivide(new int[] { 0, 1, 2 }, number_of_subdivisions);
        subdivide(new int[] { 0, 2, 3 }, number_of_subdivisions);
        subdivide(new int[] { 0, 3, 1 }, number_of_subdivisions);
        subdivide(new int[] { 1, 3, 2 }, number_of_subdivisions);
    }
    private float min_value = 1000000;
    /*
    Wrapper function for terrain generation, for now it is using only 3D simplex noise
    */
    private float get_perlin_amplitude(Vector3 geocentric_coords, float frequency = 1.0f)
    {
        //float latitude = Mathf.Atan2(geocentric_coords.x, geocentric_coords.y);
        //float longitude = Mathf.Sin(geocentric_coords.z);
        Vector3 frequency_vector = (geocentric_coords + Vector3.one * seed ) * frequency ;  
        float value = SimplexNoise.simplex3D(frequency_vector);
        if (value < min_value) min_value = value;
        return value;
    }

    /*
     Creates mesh
    */
    private void create_mesh()
    {
        if (vertices_buffer.Count <= 0) return;

        int[] indices_new = new int[indices_buffer.Count];
        Vector3[] vertices_new = new Vector3[indices_buffer.Count];
        Vector3[] normals = new Vector3[indices_buffer.Count];
        Color[] colors = new Color[indices_buffer.Count];
        Color ocean_new_color = get_colors_based_on_temperature(water_gradients_colors);
        Color grass_new_color = get_colors_based_on_temperature(grass_gradients_colors);
        Color highlands_new_color = get_colors_based_on_temperature(highlands_gradients_colors);
        Color mountains_new_color = get_colors_based_on_temperature(mountains_gradients_colors);

        for (int i = 0; i < indices_new.Length; i += 3) //for each triangle
        {
            Vector3 vert1 = vertices_buffer[indices_buffer[i]];
            Vector3 vert2 = vertices_buffer[indices_buffer[i + 1]];
            Vector3 vert3 = vertices_buffer[indices_buffer[i + 2]];

            //calculate normals
            Vector3 u = vert2 - vert1;
            Vector3 v = vert3 - vert1;
            Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));

            //get terrain noise for given point in 3D space
            float ampl1 = Mathf.Max(get_perlin_amplitude(vert1, terrain_noise_frequency), ocean_height);
            float ampl2 = Mathf.Max(get_perlin_amplitude(vert2, terrain_noise_frequency), ocean_height);
            float ampl3 = Mathf.Max(get_perlin_amplitude(vert3, terrain_noise_frequency), ocean_height);

            //assign terrain noise to vertices
            vertices_new[i] = (vert1 + vert1 * (ampl1 - .5f) * terrain_height) * planet_radii;
            vertices_new[i + 1] = (vert2 + vert2 * (ampl2 - .5f) * terrain_height) * planet_radii;
            vertices_new[i + 2] = (vert3 + vert3 * (ampl3 - .5f) * terrain_height) * planet_radii;

            //assign indices
            indices_new[i] = i;
            indices_new[i + 1] = i + 1;
            indices_new[i + 2] = i + 2;

            //assign normals
            normals[i] = normal;
            normals[i + 1] = normal;
            normals[i + 2] = normal;

            //assign color based on terrain height
            float max_height = Mathf.Max(Mathf.Max(ampl1, ampl2), ampl3); //using max function for determining triangle height based on its vertices
            Color curr_clr = new Color(0.0f, 0.0f, 0.0f);
            if (max_height <= ocean_height)
            {
                curr_clr = ocean_new_color;
            }
            else if (max_height < HIGHLANDS_HEIGHT)
            {
                curr_clr = grass_new_color;
            }
            else if (max_height < MOUNTAINS_HEIGHT)
            {
                curr_clr = highlands_new_color;
            }
            else
            {
                curr_clr = mountains_new_color;
            }

            //apply color randomness for better visual appeal
            float color_rand_factor = Random.RandomRange(-color_randomness, color_randomness);
            curr_clr += new Color(color_rand_factor, color_rand_factor, color_rand_factor);

            //assign colors
            colors[i] = curr_clr;
            colors[i + 1] = curr_clr;
            colors[i + 2] = curr_clr;
        }

        //Split vertices into seperate meshes
        int num_of_indices = indices_buffer.Count;
        int num_of_meshes = (int)Mathf.Ceil(indices_buffer.Count / (float)MAX_VERTICES);
        //Debug.Log("Num of meshes: " + num_of_meshes);
        for (int i = 0; i < num_of_meshes; i++)
        {
            int buffer_size = Mathf.Min(MAX_VERTICES, num_of_indices - MAX_VERTICES * i);
            Vector3[] vertices_chunks = new Vector3[buffer_size];
            int[] indices_chunks = new int[buffer_size];
            Vector3[] normals_chunks = new Vector3[buffer_size];
            Color[] colors_chunks = new Color[buffer_size];


            for (int j = 0; j < buffer_size; j++)
            {
                indices_chunks[j] = indices_new[j + MAX_VERTICES * i];
                if (i != 0) indices_chunks[j] = indices_chunks[j] % (MAX_VERTICES * i);
            }

            System.Array.Copy(vertices_new, MAX_VERTICES * i, vertices_chunks, 0, buffer_size);
            System.Array.Copy(normals, MAX_VERTICES * i, normals_chunks, 0, buffer_size);
            System.Array.Copy(colors, MAX_VERTICES * i, colors_chunks, 0, buffer_size);
            GameObject obj = new GameObject();
            obj.name = gameObject.name + i.ToString();
            MeshFilter _filter = obj.AddComponent<MeshFilter>();
            MeshRenderer _renderer = obj.AddComponent<MeshRenderer>();

            _renderer.material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;

            _filter.mesh.vertices = vertices_chunks;
            _filter.mesh.triangles = indices_chunks;
            _filter.mesh.normals = normals_chunks;
            _filter.mesh.colors = colors_chunks;
            obj.transform.SetParent(gameObject.transform);
			
			var rig = obj.AddComponent<Rigidbody>();
			rig.isKinematic = true;
			obj.AddComponent<Meshinator>();
			obj.AddComponent<MeshCollider>();
        }
    }

    private void initialize_orbit()
    {
        float rand_angle = Random.Range(0.0f, Mathf.PI * 2);
        Vector3 aphelion_vector = new Vector3(Mathf.Cos(rand_angle) * aphelion_AU, 0.0f, Mathf.Sin(rand_angle) * aphelion_AU);
        Vector3 object_position = orbit_target.transform.position + aphelion_vector;
        orbital_period = Mathf.Sqrt(aphelion_AU * aphelion_AU * aphelion_AU * kepler_ratio);
        orbit_perimeter = Mathf.PI * 2 * aphelion_AU;
        degree_increment = orbit_perimeter / orbital_period;
        transform.position = object_position;
        float axis_tilt_in_radians = (axis_tilt * Mathf.PI) / 180.0f;
        axis_tilt_vector = new Vector3(Mathf.Sin(axis_tilt_in_radians), Mathf.Cos(axis_tilt_in_radians), 0.0f);
    }

    /**
    Calculates planet temperature in Kelvin based on advanced equation.
    */
    public float get_planet_temperature(bool account_sol_lumosity = false)
    {
        float planet_real_distance = aphelion_AU * UNITY_DISTANCE_CONSTANT;

        float sol_current_lumosity = 0.0f;

        if(account_sol_lumosity)
        {
            Transform[] children = orbit_target.transform.GetComponentsInChildren<Transform>();
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
        }
        else
        {
            sol_current_lumosity = 1.0f / UNITY_SOL_LUMOSITY_RATIO;
        }

        float sun_scaled_lumosity = SUN_LUMOSITY * sol_current_lumosity * UNITY_SOL_LUMOSITY_RATIO;
        float temp = Mathf.Pow((sun_scaled_lumosity * (1 - albedo)) / (16 * Mathf.PI * planet_real_distance * planet_real_distance * BOLTZMANN_CONSTANT), 0.25f);
        temp += greenhouse_effect;
        return temp;
    }

    /**
        Retrieves planet colors based on gradients
    */
    private Color get_colors_based_on_temperature(TemperatureGradient[] color_gradient)
    {
        int gradient_table_size = color_gradient.Length;
        Debug.Log("Gradient table size: " + gradient_table_size);
        int gradient_index = 0;

        while (gradient_index < gradient_table_size && planet_temperature > color_gradient[gradient_index].temperature) gradient_index++;
        Debug.Log("Gradient index: " + gradient_index);
        if (gradient_index == 0)
        {
            return color_gradient[0].color;
        }
        else if (gradient_index >= gradient_table_size)
        {
            return color_gradient[gradient_table_size - 1].color;
        }
        else
        {
            float dAlpha = 1.0f - ((color_gradient[gradient_index].temperature - planet_temperature) / (color_gradient[gradient_index].temperature - color_gradient[gradient_index - 1].temperature));
            Debug.Log("dAlpha: " + dAlpha);
            float dR = color_gradient[gradient_index].color.r - color_gradient[gradient_index - 1].color.r;
            float dG = color_gradient[gradient_index].color.g - color_gradient[gradient_index - 1].color.g;
            float dB = color_gradient[gradient_index].color.b - color_gradient[gradient_index - 1].color.b;

            Vector3 gradient_vector = new Vector3(dR, dG, dB);
            gradient_vector = gradient_vector * dAlpha;
            Vector3 initial_color = new Vector3(color_gradient[gradient_index-1].color.r, color_gradient[gradient_index-1].color.g, color_gradient[gradient_index-1].color.b);
            Vector3 final_color = initial_color + gradient_vector;
            Debug.Log("Final color vector: " + final_color);
            return new Color(final_color.x, final_color.y, final_color.z);

        }
    }

    /**
    Places any object on planet on geocentric coordinates. Returs 0 if the script failed to place object and 1 if object was placed.
    */
    bool place_object_on_planet(GameObject obj, float longitude, float latitude, string name)
    {
        //Mozna by tez usunac collidery z prefabow, bedzie szybciej
        foreach (Collider c in obj.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        bool status = false;

        float frequency = terrain_noise_frequency;
        Vector3 cartesian_vector = geocentric_to_cartesian(longitude, latitude);
        float perlin_amplitude = get_perlin_amplitude(cartesian_vector, terrain_noise_frequency);
        if (perlin_amplitude > ocean_height && perlin_amplitude < MOUNTAINS_HEIGHT)
        {
            status = true;
            float height_value = (planet_radii + SimplexNoise.simplex3D(cartesian_vector * frequency) * terrain_height - obj.transform.lossyScale.y * 1.5f);
            Vector3 position = gameObject.transform.position + height_value * cartesian_vector;
            Quaternion rotation = Quaternion.LookRotation(cartesian_vector);
            rotation *= Quaternion.Euler(90, 0, 0);
            GameObject _obj = (GameObject)Instantiate(obj, position, rotation);
            _obj.name = name;
            _obj.transform.parent = gameObject.transform;
        }
        return status;
    }

    /**
        Retrieves tree based on planet's temperature
    */
    private GameObject get_random_tree()
    {
        GameObject tree = null;
        float region_temperature = Random.Range(planet_temperature - VARIATION_HALF_RANGE, planet_temperature + VARIATION_HALF_RANGE);

        if (region_temperature < COLD_BARRIER || region_temperature > HOT_BARRIER) return null;
        if(region_temperature < TEMPERATE_BARRIER)
        {
            int tree_index = Random.Range(0, winter_flora.Count - 1);
            tree = winter_flora[tree_index];
        }
        else if (region_temperature < TROPICAL_BARRIER)
        {
            int tree_index = Random.Range(0, temperate_flora.Count - 1);
            tree = temperate_flora[tree_index];
        }
        else //tropical region
        {
            int tree_index = Random.Range(0, tropical_flora.Count - 1);
            tree = tropical_flora[tree_index];
        }
        return tree;
    }

    /**
        Populates planet with flora based on temperature and forestation
    */

    private void populate_planet()
    {

        if(winter_flora.Count == 0 || temperate_flora.Count == 0 || tropical_flora.Count == 0)
        {
            Debug.Log("Not all assets are loaded. Aborting flora population!");
            return;
        }

        float surface_area_ratio = planet_radii * planet_radii;
        int number_of_trees = (int)(PLANET_FLORA_AMOUNT * surface_area_ratio * forestation);
        int counter = 0;
        bool success = false;

        while (counter < number_of_trees)
        {
            GameObject tree = get_random_tree();
            if (tree == null) //too cold or too hot on the planet
            {
                counter += 1;
            }
            else
            {
                success = false;
                while(!success)
                {
                    float rand_longitude = Random.Range(0.0f, 359.0f);
                    float rand_lattitude = Random.Range(0.0f, 179.0f);
                    success = place_object_on_planet(tree, rand_longitude, rand_lattitude, "Tree" + counter.ToString());
                    if (success) counter += 1;
                }
            }
        }
        //add cities
        /*
        counter = 0;
        while(counter < cities_amount)
        {
            success = false;
            while(!success)
            {
                float rand_longitude = Random.Range(0.0f, 359.0f);
                float rand_lattitude = Random.Range(0.0f, 179.0f);
                success = place_object_on_planet
            }
        }
        */
    }


    /**
        Converts geocentric coordinates to cartesian coordinates
    */
    private Vector3 geocentric_to_cartesian(float longitude, float lattitude)
    {

        longitude = longitude * Mathf.Deg2Rad;
        lattitude = lattitude * Mathf.Deg2Rad;

        float sin_lattitude = Mathf.Sin(lattitude);
        float x = Mathf.Cos(longitude) * sin_lattitude;
        float y = Mathf.Sin(longitude) * sin_lattitude;
        float z = Mathf.Cos(lattitude);
        return new Vector3(x, y, z);
    }

    private void addCore()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Core";
        sphere.transform.localScale = new Vector3(planet_radii, planet_radii, planet_radii);
        sphere.transform.position = transform.position;
        sphere.transform.SetParent(gameObject.transform);
    }

    void Start ()
    {
        if (!parameters_correct()) return;
        convert_parameters();
        load_flora();
        planet_temperature = get_planet_temperature(false);
        Debug.Log("planet temperature " + planet_temperature + "K");
        addCore();
        subdivision_surfaces_planet();
        create_mesh();
        initialize_orbit();
        if(flora_enabled) populate_planet();
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(axis_tilt_vector * axis_rotation_speed * Time.deltaTime * 2);
        transform.RotateAround(orbit_target.transform.position, Vector3.up, degree_increment * Time.deltaTime);
    }
}