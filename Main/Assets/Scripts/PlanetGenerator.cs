using UnityEngine;
using System.Collections;

public class PlanetGenerator : MonoBehaviour {

	public MinimalPlanet planet1;
	public MinimalPlanet planet2;
	public MinimalPlanet planet3;
	public GameObject sun;
	public GameObject player;
	public GameObject planetHp;

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
		for(int i = 1; i<=Random.Range(3,7); i++) {
			Vector3 initialRandomPoint = Random.onUnitSphere;
			int actualRay = Random.Range (curr_min_dist, curr_min_dist + max_distance_between_planets);
			Vector3 startingPoint = (initialRandomPoint + new Vector3(1, 0, 1))* actualRay;
			MinimalPlanet newPlanet = Instantiate (planets [Random.Range (0, 3)]);
			newPlanet.transform.position = startingPoint;
			newPlanet.orbit_target = sun;
			newPlanet.Start2 ();
			curr_min_dist = (actualRay + min_distance_between_planets)*(actualRay + min_distance_between_planets);
		}
	}
}
