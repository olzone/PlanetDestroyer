using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlanetGenerator : MonoBehaviour {

	public MinimalPlanet planet1;
	public MinimalPlanet planet2;
	public MinimalPlanet planet3;
	public GameObject sun;
	public GameObject player;
	public GameObject planetHp;
	public Slider planet_hp;
	public int min_distance_from_sun;
	public int min_distance_between_planets;
	public int max_distance_between_planets;
	// Use this for initialization
	void Start () {
		MinimalPlanet[] planets = new MinimalPlanet[3];
		planets [0] = planet1;
		planets [1] = planet2;
		planets [2] = planet3;
		int curr_min_dist = min_distance_from_sun;
		List<MinimalPlanet> targets = new List<MinimalPlanet> ();
		for(int i = 1; i<= PlayerPrefs.GetInt("numberOfPlanets"); i++) {	
			Vector3 initialRandomPoint = Random.onUnitSphere;
			int actualRay = Random.Range (curr_min_dist, curr_min_dist + max_distance_between_planets);
			Vector3 startingPoint = (initialRandomPoint + new Vector3(1, 0, 1))* actualRay;
			MinimalPlanet newPlanet = Instantiate (planets [Random.Range (0, 3)]);
			newPlanet.transform.position = startingPoint;
			newPlanet.orbit_target = sun;
			newPlanet.seed = Random.Range (0, 65536);
			newPlanet.planet_radii = Random.Range (1.0f, 3.0f);
			newPlanet.ocean_height = Random.Range (0.1f, 1.0f);
			newPlanet.terrain_noise_frequency = Random.Range (1.0f, 10.0f);
			newPlanet.terrain_height = Random.Range (0.01f, 0.1f);
			newPlanet.Start2 ();
			//((life)newPlanet).player = player;
			life l = newPlanet.GetComponents<life>()[0];
			l.player = player;
			l.slider = planet_hp;
			curr_min_dist = (actualRay + min_distance_between_planets)*(actualRay + min_distance_between_planets);
			targets.Add (newPlanet);
		}
		PlayerMovementController.targets = targets;
	}
}
