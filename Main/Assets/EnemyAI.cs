using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VolumetricLines;

public class EnemyAI : MonoBehaviour
{

    public float patrolSpeed = 2f;                          // The nav mesh agent's speed when patrolling.
    public float chaseSpeed = 5f;                           // The nav mesh agent's speed when chasing.
    public float rotationSpeed = 0.1f;
    public float chaseWaitTime = 5f;                        // The amount of time to wait when the last sighting is reached.
    public float patrolWaitTime = 1f;                       // The amount of time to wait when the patrol way point is reached.
    public Transform[] patrolWayPoints;                     // An array of transforms for the patrol route.

    public float visibilityRadius = 100f;

    private Vector3 center = Vector3.zero;

    private Vector3 targetPos;

    public GameObject player;
    public GameObject planet;
    public GameObject smoke;

    public float startingHealh = 20f;
    public float Heatlh = 0f;

    private List<Vector3> patrolPoints = new List<Vector3>();
    private List<Vector3> attackPoints = new List<Vector3>();
    private int patorTargetC = 0;
    private int attackTargetC = 0;

    bool isSmoke = false;
    int attackCount = 0;

    Vector3 vel_vec;
    Quaternion neededRotation;
    Quaternion rot180y = Quaternion.identity;
    float epislon = 5f;

    Random rnd = new Random();
  
    public float laserspeed = 30f;
    public float laserFireRate = 0.5F;
    private float laserNextFire = 0.0F;
    public GameObject laser;

    public GameObject leftIns;
    public GameObject rightIns;



    void Awake()
    {
        Heatlh = startingHealh;
        rot180y.y = 180f;

        for(int i=0;i<25;i++)
            patrolPoints.Add(Random.onUnitSphere * 50);

        for (int i = 0; i < 5; i++)
            attackPoints.Add(Random.onUnitSphere * 25);


        targetPos = patrolPoints[patorTargetC];
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.transform.root.GetComponent<VolumetricLineBehavior>().m_lineColor == Color.red)
            Heatlh = 0f;
        //Debug.Log("xxxxxxxxxxxxxxxxxxxxxx");
        //foreach (ContactPoint contact in collision.contacts)
        //{
        //   Debug.DrawRay(contact.point, contact.normal, Color.white);
        //}

    }


    void Update()
    {
        var sun_vec = center - transform.position;

        if (sun_vec.magnitude <= 10f)
            Heatlh = 0f;

        if (Heatlh <= 0f)
            Dead();
        else if (!isPlayerInChaseRange())
            Patrolling();
        else
            Chasing();
    }

    bool isPlayerInChaseRange()
    {
        return Vector3.Distance(player.transform.position, transform.position) < visibilityRadius;
    }

    void Chasing()
    {
        if(attackCount > 0)
        {
            Attack();
            return;
        }
        targetPos = attackPoints[attackTargetC];

        vel_vec = (targetPos + player.transform.position) - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;

        if (vel_vec.magnitude <= epislon)
        {
            attackCount = (int)((Random.value*9457)%15);
            attackTargetC++;
            attackTargetC %= attackPoints.Count;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * rotationSpeed);

        transform.Translate(-1 * Vector3.forward * chaseSpeed * Time.deltaTime);
    }

    void Attack()
    {
        vel_vec = (player.transform.position) - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;
        transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * rotationSpeed);
        //transform.Translate(-1 * Vector3.forward * chaseSpeed * Time.deltaTime);

        //Debug.Log((transform.rotation.eulerAngles - neededRotation.eulerAngles).magnitude);
        if ((transform.rotation.eulerAngles - neededRotation.eulerAngles).magnitude <= 2f)
        {
            if(Time.time > laserNextFire)
            { 
                laserNextFire = Time.time + laserFireRate;
                GameObject laser_tmp = (GameObject)Instantiate(laser, leftIns.transform.position, leftIns.transform.rotation);
                GameObject laser_tmp2 = (GameObject)Instantiate(laser, rightIns.transform.position, rightIns.transform.rotation);
                laser_tmp.GetComponent<Rigidbody>().velocity = -1*transform.forward * laserspeed;
                laser_tmp2.GetComponent<Rigidbody>().velocity = -1*transform.forward * laserspeed;

                attackCount--;
            }
        }

    }

    void Patrolling()
    {
        targetPos = patrolPoints[patorTargetC];

        vel_vec = (targetPos + planet.transform.position) - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;

        if(vel_vec.magnitude <= epislon)
        {
            patorTargetC++;
            patorTargetC %= patrolPoints.Count;
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

    void Dead()
    {
        if (!isSmoke)
        {
            GameObject s = (GameObject)Instantiate(smoke, transform.position + new Vector3(0f, 0f, 0f), transform.rotation);
            s.transform.parent = gameObject.transform;

            isSmoke = true;
        }


        vel_vec = center - transform.position;
        neededRotation = Quaternion.LookRotation(vel_vec) * rot180y;

        if (vel_vec.magnitude <= 5.5f)
        {
            Destroy(gameObject);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, neededRotation, Time.deltaTime * rotationSpeed);

        transform.Translate(-1 * Vector3.forward * patrolSpeed * Time.deltaTime);
    }
}
