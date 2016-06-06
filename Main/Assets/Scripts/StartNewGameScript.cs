using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class StartNewGameScript : MonoBehaviour {

	public InputField planets;
	public InputField enemies;

	GameObject numberOfPlanetsField;
	// Update is called once per frame
	public void ChangeScene () {
		try {
			int numOfPlanets = Convert.ToInt32(planets.text);
			int numOfEnemies = Convert.ToInt32(enemies.text);
			PlayerPrefs.SetInt ("numberOfPlanets", numOfPlanets);
			PlayerPrefs.SetInt ("numberOfEnemies", numOfEnemies);
		} catch (Exception e) {
		}
		if (PlayerPrefs.GetInt ("numberOfPlanets") <= 0) {
			PlayerPrefs.SetInt ("numberOfPlanets", 1);
		}

		if (PlayerPrefs.GetInt ("numberOfEnemies") <= 0) {
			PlayerPrefs.SetInt ("numberOfEnemies", 1);
		}
		Application.LoadLevel("Proc_planet");
	}
 		
}
