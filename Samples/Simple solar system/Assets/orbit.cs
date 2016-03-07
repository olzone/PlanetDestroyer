using UnityEngine;
using System.Collections;

public class orbit : MonoBehaviour {

    public int r;
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 v = transform.position;

        transform.up = transform.position.normalized;

        GetComponent<Rigidbody>().velocity = transform.forward * r;
        

        if(Vector3.Distance(v, new Vector3(0,0,0)) > r)
        {
            v = v.normalized * (float)r;
        }

        transform.position = v;
    }
}
