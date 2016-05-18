using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SubdivisionSurfacesPlanet : MonoBehaviour
{
    [Tooltip("Number of subdivisions. More subdivisions means more detailed planet but this in turn introduces more chunks. Default is 7")]
    [Range(5, 8)]
    public int number_of_subdivisions = 7; //each subdivision level yields 4^(n+1) triangles
    [Tooltip("Radius of the planet. Default value is 2")]
    [Range(1.0f, 5.0f)]
    public float planet_radii = 2;
    [Tooltip("Height of the ocean, should be low. Default is 0.1f")]
    [Range(0.0f, 0.2f)]
    public float ocean_height = 0.10f;
    [Tooltip("The lower the value the less mountains you will get and terrain will be smoother")]
    [Range(1.0f, 10.0f)]
    public float terrain_noise_frequency = 4.0f;
    [Tooltip("Terrain height multiplier. Default is 0.05")]
    [Range(0.01f, 0.1f)]
    public float terrain_height = 0.05f;
    [Tooltip("Adds small randomness to the color")]
    [Range(0.01f, 0.1f)]
    public float color_randomness = .05f;

    public Color ocean_color = new Color(0.0f / 255.0f, 191.0f / 255.0f, 255.0f / 255.0f);
    public Color grass_color = new Color(107.0f / 255.0f, 142.0f / 255.0f, 35.0f / 255.0f);
    public Color highlands_color = new Color(205.0f / 255.0f, 133.0f / 255.0f, 63.0f / 255.0f);
    public Color mountains_color = new Color(255.0f / 255.0f, 250.0f / 255.0f, 255.0f / 255.0f);

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> indices = new List<int>();

    private float negative_root_two_over_three = -Mathf.Sqrt(2) / 3.0f;
    private float negative_one_third = -1.0f / 3.0f;
    private float root_six_over_three = Mathf.Sqrt(6) / 3.0f;
    private Vector2 latitude_vector = new Vector2(1, 0);

    private const int MAX_VERTICES = 65001;

    /**
        Generates planet using subdivision surfaces method for platonic solids
    */
    private void generate_base_planet()
    {

        Vector3 tetrahedron_top = new Vector3(0, 0, 1);
        Vector3 tetrahedron_bottom1 = new Vector3(0, (float)2.0f * Mathf.Sqrt(2) / 3.0f, negative_one_third);
        Vector3 tetrahedron_bottom2 = new Vector3(-root_six_over_three, negative_root_two_over_three, negative_one_third);
        Vector3 tetrahedron_bottom3 = new Vector3(root_six_over_three, negative_root_two_over_three, negative_one_third);

        vertices.Add(tetrahedron_top);
        vertices.Add(tetrahedron_bottom1);
        vertices.Add(tetrahedron_bottom2);
        vertices.Add(tetrahedron_bottom3);

        subdivide(new int[] { 0, 1, 2 }, number_of_subdivisions);
        subdivide(new int[] { 0, 2, 3 }, number_of_subdivisions);
        subdivide(new int[] { 0, 3, 1 }, number_of_subdivisions);
        subdivide(new int[] { 1, 3, 2 }, number_of_subdivisions);

    }

    /**
        Recursive subdivision call
    */
    private void subdivide(int[] triangle, int level)
    {
        if (level > 0)
        {
            //subdivide further
            vertices.Add(Vector3.Normalize((vertices[triangle[0]] + vertices[triangle[1]]) * .5f));
            vertices.Add(Vector3.Normalize((vertices[triangle[1]] + vertices[triangle[2]]) * .5f));
            vertices.Add(Vector3.Normalize((vertices[triangle[2]] + vertices[triangle[0]]) * .5f));

            int ind01 = vertices.Count - 3;
            int ind12 = vertices.Count - 2;
            int ind20 = vertices.Count - 1;

            level--;

            subdivide(new int[] { triangle[0], ind01, ind20 }, level);
            subdivide(new int[] { ind01, triangle[1], ind12 }, level);
            subdivide(new int[] { ind01, ind12, ind20 }, level);
            subdivide(new int[] { ind20, ind12, triangle[2] }, level);
        }
        else
        {
            indices.Add(triangle[0]);
            indices.Add(triangle[1]);
            indices.Add(triangle[2]);
        }
    }

    /**
        Wrapper function for terrain generation, for now it is using only 3D simplex noise
    */
    private float get_perlin_amplitude(Vector3 geocentric_coords, float frequency = 1.0f)
    {
        //float latitude = Mathf.Atan2(geocentric_coords.x, geocentric_coords.y);
        //float longitude = Mathf.Sin(geocentric_coords.z);
        Vector3 frequency_vector = geocentric_coords * frequency;

        return SimplexNoise.simplex3D(frequency_vector);
    }

    // Use this for initialization
    void Start () {

        generate_base_planet();

        //generate new triangles
        int[] indices_new = new int[indices.Count];
        Vector3[] vertices_new = new Vector3[indices.Count];
        Vector3[] normals = new Vector3[indices.Count];
        Color[] colors = new Color[indices.Count];

        Color color_lime = new Color(.597f, .597f, 0f);
        Color color_green = new Color(.199f, .398f, 0f);

        //Debug.Log("Num of triangles:" + indices.Count/3);

        for (int i = 0; i < indices_new.Length; i+=3) //for each triangle
        {
            Vector3 vert1 = vertices[indices[i]];
            Vector3 vert2 = vertices[indices[i+1]];
            Vector3 vert3 = vertices[indices[i+2]];

            //calculate normals
            Vector3 u = vert2 - vert1;
            Vector3 v = vert3 - vert1;
            Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));

            //get terrain noise for given point in 3D space
            float ampl1 = Mathf.Max(get_perlin_amplitude(vert1, terrain_noise_frequency), ocean_height);
            float ampl2 = Mathf.Max(get_perlin_amplitude(vert2, terrain_noise_frequency), ocean_height);
            float ampl3 = Mathf.Max(get_perlin_amplitude(vert3, terrain_noise_frequency), ocean_height);

            //assign terrain noise to vertices
            vertices_new[i]     = ( vert1 + vert1 * (ampl1 - .5f) * terrain_height) * planet_radii;
            vertices_new[i + 1] = ( vert2 + vert2 * (ampl2 - .5f) * terrain_height) * planet_radii;
            vertices_new[i + 2] = ( vert3 + vert3 * (ampl3 - .5f) * terrain_height) * planet_radii;

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
                curr_clr = ocean_color;
            } 
            else if(max_height < 0.7)
            {
                curr_clr = grass_color;
            }
            else if (max_height < 0.95)
            {
                curr_clr = highlands_color;
            }
            else
            {
                curr_clr = mountains_color;
            }

            //apply color randomness for better visual appeal
            float color_rand_factor = Random.RandomRange(-color_randomness, color_randomness);
            curr_clr += new Color(color_rand_factor, color_rand_factor, color_rand_factor);

            //assign colors
            colors[i] = curr_clr;
            colors[i + 1] = curr_clr;
            colors[i + 2] = curr_clr;
        }

        int num_of_indices = indices.Count;
        int num_of_meshes = (int)Mathf.Ceil(indices.Count/(float)MAX_VERTICES);
        //Debug.Log("Num of meshes: " + num_of_meshes);
        for(int i = 0; i < num_of_meshes; i++)
        {
            int buffer_size = Mathf.Min(MAX_VERTICES, num_of_indices - MAX_VERTICES * i);
            Vector3[] vertices_buffer = new Vector3[buffer_size];
            int[] indices_buffer = new int[buffer_size];
            Vector3[] normals_buffer = new Vector3[buffer_size];
            Color[] colors_buffer = new Color[buffer_size];


            for(int j = 0; j < buffer_size; j++)
            {              
                indices_buffer[j] = indices_new[j+MAX_VERTICES*i];
                if (i != 0) indices_buffer[j] = indices_buffer[j] % (MAX_VERTICES * i);
            }

            System.Array.Copy(vertices_new, MAX_VERTICES * i, vertices_buffer, 0, buffer_size);
            //System.Array.Copy(indices_new, MAX_VERTICES * i, indices_buffer, 0, buffer_size);
            System.Array.Copy(normals, MAX_VERTICES * i, normals_buffer, 0, buffer_size);
            System.Array.Copy(colors, MAX_VERTICES * i, colors_buffer, 0, buffer_size);
            //Debug.Log("buffer size: " + buffer_size);
            GameObject obj = new GameObject();
            obj.name = gameObject.name + i.ToString();
            MeshFilter _filter = obj.AddComponent<MeshFilter>();
            MeshRenderer _renderer = obj.AddComponent<MeshRenderer>();
            SphereCollider _sc = (SphereCollider)obj.AddComponent<SphereCollider>();

            _renderer.material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;

            _sc.center = transform.position;
            _sc.radius = planet_radii;

            _filter.mesh.vertices = vertices_buffer;
            _filter.mesh.triangles = indices_buffer;
            _filter.mesh.normals = normals_buffer;
            _filter.mesh.colors = colors_buffer;
            obj.transform.SetParent(gameObject.transform);

        }

        //MeshFilter filter = GetComponent<MeshFilter>();
        //Mesh mesh = new Mesh();
        //filter.mesh = mesh;

        //build mesh
        //mesh.vertices = vertices_new;
        ////mesh.triangles = indices_new;
        //mesh.normals = normals;
        //mesh.colors = colors;

        //SphereCollider _sc = (SphereCollider)gameObject.AddComponent<SphereCollider>();
        //_sc.center = transform.position;
        //_sc.radius = planet_radii;

    }

    public void ReColor()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        Vector3 planetCenter = transform.position;

        //Debug.Log(planetCenter);
        //Debug.Log(mesh.vertices[0]);
        //Debug.Log(Vector3.Distance(mesh.vertices[0], planetCenter));

        //Debug.Log(mesh.colors[0]);
        //Debug.Log(mesh.colors[1]);
        //Debug.Log(mesh.colors[2]);

        //mesh.vertices.Length

        Debug.Log(mesh.vertices.Length);
        Debug.Log(mesh.triangles.Length);
        Debug.Log(mesh.normals.Length);
        Debug.Log(mesh.colors.Length);


        for (int i = 0; i < mesh.vertices.Length/10; i += 3) //for each triangle
        {
            //Vector3 v1 = mesh.vertices[i + 0];
            //Vector3 v2 = mesh.vertices[i + 1];
            //Vector3 v3 = mesh.vertices[i + 2];

            //float d1 = Vector3.Distance(v1, planetCenter);
            //float d2 = Vector3.Distance(v2, planetCenter);
            //float d3 = Vector3.Distance(v3, planetCenter);

            ////Debug.Log(d1);
            ////Debug.Log(d2);
            ////Debug.Log(d3);

            //float max_height = Mathf.Max(d1, d2, d3);

            //Color curr_clr = new Color(0.0f, 0.0f, 0.0f);
            //if (max_height <= ocean_height)
            //{
            //    curr_clr = ocean_color;
            //}
            //else if (max_height < 0.7)
            //{
            //    curr_clr = grass_color;
            //}
            //else if (max_height < 0.95)
            //{
            //    curr_clr = highlands_color;
            //}
            //else
            //{
            //    curr_clr = mountains_color;
            //}

            ////apply color randomness for better visual appeal
            //float color_rand_factor = Random.RandomRange(-color_randomness, color_randomness);
            //curr_clr += new Color(color_rand_factor, color_rand_factor, color_rand_factor);

            //mesh.colors[i + 0] = Color.red;
            //mesh.colors[i + 1] = Color.red;
            //mesh.colors[i + 2] = Color.red;
        }
    } 
}
