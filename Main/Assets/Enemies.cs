using UnityEngine;
using System.Collections;

public class Enemies : MonoBehaviour {

    public int numberOfEnemies;
    public GameObject enemy;
    public GameObject player;
    public GameObject planet;
    // Use this for initialization
    void Start () {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            GameObject e = (GameObject)Instantiate(enemy, Random.insideUnitSphere * 50, Quaternion.identity);
            e.GetComponent<EnemyAI>().planet = planet;
            e.GetComponent<EnemyAI>().player = player;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
