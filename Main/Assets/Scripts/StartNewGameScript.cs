using UnityEngine;
using System.Collections;

public class StartNewGameScript : MonoBehaviour {

	// Update is called once per frame
	public void ChangeScene () {
		Application.LoadLevel("Proc_planet");
	}
 		
}
