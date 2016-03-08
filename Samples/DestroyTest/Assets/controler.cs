using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class controler : MonoBehaviour
{
    public GameObject planeta;
    public GameObject planeta_back;
    private Meshinator mesh;

    // Use this for initialization
    void Start()
    {
        mesh = (Meshinator)planeta.GetComponent(typeof(Meshinator));
    }

    // Update is called once per frame
    void Update()
    {
        float x = -1.44f;
        float y = 0f;
        float z = -5f;


        if (Input.GetKeyDown(KeyCode.Alpha5))
            print("test");

        if (Input.GetKeyDown(KeyCode.Alpha1))         
            mesh.Impact(new Vector3(x, y, z), 200*new Vector3(-x, -y, -z), Meshinator.ImpactShapes.SphericalImpact, Meshinator.ImpactTypes.Compression);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            mesh.Impact(new Vector3(x, y, z), 200 * new Vector3(-x, -y, -z), Meshinator.ImpactShapes.FlatImpact, Meshinator.ImpactTypes.Compression);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            mesh.Impact(new Vector3(x, y, z), 200 * new Vector3(-x, -y, -z), Meshinator.ImpactShapes.SphericalImpact, Meshinator.ImpactTypes.Fracture);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            mesh.Impact(new Vector3(x, y, z), 1000 * new Vector3(-x, -y, -z), Meshinator.ImpactShapes.FlatImpact, Meshinator.ImpactTypes.Fracture);

        if (Input.GetKeyDown(KeyCode.R))
        {
            var tmp = planeta.transform;
            Destroy(planeta);
            planeta = (GameObject) Instantiate(planeta_back, tmp.position, tmp.rotation);
            mesh = (Meshinator)planeta.GetComponent(typeof(Meshinator));
        }

    }
}
