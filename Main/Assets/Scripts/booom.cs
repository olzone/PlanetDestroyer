using UnityEngine;
using System.Collections;

public class booom : MonoBehaviour {

    public GameObject smoke;
    public AudioClip explosionAudio;
    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnCollisionEnter(Collision collision)
    { 
        AudioSource.PlayClipAtPoint(explosionAudio, transform.position, 1f);
        //GameObject smokec = (GameObject)Instantiate(smoke, transform.position, Quaternion.identity);
        //Destroy(smokec, 6); 
        Destroy(gameObject);
    }

}
