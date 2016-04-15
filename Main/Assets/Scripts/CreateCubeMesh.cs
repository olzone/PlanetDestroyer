using UnityEngine;
using System.Collections;

public class CreateCubeMesh : MonoBehaviour {

    public float size = 1f;

	// Use this for initialization
	void Start () {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        filter.mesh = mesh;

        Vector3 right_forward_top = new Vector3(size, size, size);
        Vector3 right_forward_bottom = new Vector3(size, size, -size);
        Vector3 left_forward_bottom = new Vector3(-size, size, -size);
        Vector3 left_forward_top = new Vector3(-size, size, size);

        Vector3[] vertices = new Vector3[]
        {
            right_forward_top, right_forward_bottom, left_forward_bottom,
            left_forward_bottom, left_forward_top, right_forward_top,
        };

        int[] indices = new int[]
        {
            0,1,2,
            3,4,5
        };

        Vector3[] normals = new Vector3[]
        {
            Vector3.up, Vector3.up, Vector3.up,
            Vector3.up, Vector3.up, Vector3.up
        };

        Color color_lime = new Color(.597f, .597f, 0f);
        Color color_green = new Color(.199f, .398f, 0f);

        Color[] colors = new Color[]
        {
            color_lime, color_lime, color_lime,
            color_green, color_green, color_green
        };

        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.normals = normals;
        mesh.colors = colors;
    }
}
