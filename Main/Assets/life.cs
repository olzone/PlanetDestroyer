using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class life : MonoBehaviour {

    public float lifeVal = 10f;
    public bool isDead = false;
    public GameObject player;

    public GameObject beforeemplision;
    public GameObject emplision;

    public Slider slider;

    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void imTraget()
    {
        slider.value = lifeVal;
    }

    public void Dmg(float val)
    {
        lifeVal -= val;

        slider.value = lifeVal;

        // Play the hurt sound effect.
        //playerAudio.Play();

        // If the player has lost all it's health and the death flag hasn't been set yet...
        if (lifeVal <= 0 && !isDead)
        {
            // ... it should die.
            Death();
        }
    }
    public void Death(){
            isDead = true;
            player.transform.parent = null;
            player.GetComponent<PlayerMovementController>().PlanetDestoried(gameObject.GetComponent<MinimalPlanet>());
            GameObject e = (GameObject)Instantiate(emplision, transform.position, Quaternion.identity);
            Destroy(e, 2);
            Destroy(gameObject.transform.root.gameObject, 1);
    
    }
}
