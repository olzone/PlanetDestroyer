using UnityEngine;
using System.Collections;

public class mov : MonoBehaviour {

    public GameObject missile;
    public float missilespeed = 10f;
    public float fireRate = 2.0F;
    private float nextFire = 0.0F;

    public AudioClip missileAudio;

    public int speed;
    private Meshinator mesh;
    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        
        

		if(Input.GetKey(KeyCode.D))
		{
			transform.Translate(new Vector3(speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.A))
		{
			transform.Translate(new Vector3(-speed * Time.deltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.S))
		{
			transform.Translate(new Vector3(0,-speed * Time.deltaTime,0));
		}
		if(Input.GetKey(KeyCode.W))
		{
			transform.Translate(new Vector3(0,speed * Time.deltaTime,0));
		}
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(new Vector3(0, 0, -speed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(new Vector3(0, 0, speed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.Z))
        {
            transform.Rotate(0, -speed/5, 0);
        }
        if (Input.GetKey(KeyCode.C))
        {
            transform.Rotate(0, speed/5, 0);
        }

        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            Rigidbody mmissile = (Rigidbody)Instantiate(missile, transform.position, transform.rotation);
            mmissile.velocity = transform.forward * missilespeed;
        }

        int forceMult = 2000;
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFire)
        {
            AudioSource.PlayClipAtPoint(missileAudio, transform.position, 0.5f);

            nextFire = Time.time + fireRate;
            GameObject mmissile = (GameObject)Instantiate(missile, transform.position - transform.up * 2, transform.rotation * Quaternion.LookRotation(transform.up));

            mmissile.GetComponent<Rigidbody>().velocity = transform.forward * missilespeed;
        }
    }
}
