using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour
{

    public float patrolSpeed = 2f;                          // The nav mesh agent's speed when patrolling.
    public float chaseSpeed = 5f;                           // The nav mesh agent's speed when chasing.
    public float rotationSpeed = 0.1f;
    public float chaseWaitTime = 5f;                        // The amount of time to wait when the last sighting is reached.
    public float patrolWaitTime = 1f;                       // The amount of time to wait when the patrol way point is reached.
    public Transform[] patrolWayPoints;                     // An array of transforms for the patrol route.

    private Vector3 center = Vector3.zero;

    private Vector3 targetPos;

    public GameObject player;
    public GameObject planet;

    private List<Vector3> patrolPoints = new List<Vector3>();
    private int patorTargetC = 0;

    Vector3 vel_vec;
    Quaternion neededRotation;
    Quaternion rot180y = Quaternion.identity;
    float epislon = 0.1f;

    Random rnd = new Random();



    void Awake()
    {
        rot180y.y = 180f;

        for(int i=0;i<25;i++)
            patrolPoints.Add(Random.onUnitSphere * 10);

        targetPos = patrolPoints[patorTargetC];
    }


    void Update()
    {
        if (!isPlayerInChaseRange())
            Patrolling();
        else
            Chasing();
    }

    bool isPlayerInChaseRange()
    {
        return false;
    }

    void Chasing()
    {
        vel_vec = player.transform.position - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;

        transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * rotationSpeed);

        transform.Translate(-1 * Vector3.forward * chaseSpeed * Time.deltaTime);
    }

    void Patrolling()
    {
        vel_vec = (targetPos + planet.transform.position) - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;

        if(vel_vec.magnitude <= epislon)
        {
            patorTargetC++;
            patorTargetC %= patrolPoints.Count;
            targetPos = patrolPoints[patorTargetC];
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * rotationSpeed);

        //transform.LookAt(targetPos); // enemy looks to you
        transform.Translate(-1*Vector3.forward * patrolSpeed * Time.deltaTime); // enemy walks to you
        //if (vel_vec.magnitude <= distance_byte) //if is near
        //{
        //    animation.Play("byte", PlayMode.StopAll);
        //    vel_vec = Vector3.zero;
        //}
        //else if (vel_vec.magnitude > distance_byte + 5) // if is far
        //    animation.Play("walk", PlayMode.StopAll);
    }
}
