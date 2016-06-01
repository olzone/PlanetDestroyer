using UnityEngine;
using System.Collections.Generic;
using VolumetricLines;

public class PlayerMovementController : MonoBehaviour {

    public MinimalPlanet target;
    public static List<MinimalPlanet> targets = null;
    public float freeRoamingRotationSpeed;
    public float freeRoamingMovementSpeed;
    public float focusedLinearSpeed;
    public float angle;
    public float switchControllDistance;
	public bool focus = false;

    public AudioClip missileAudio;
    public float missilespeed = 10f;
    public float laserspeed = 30f;
    public float fireRate = 2.0F;
    public float laserFireRate = 0.5F;
    private float nextFire = 0.0F;
    private float laserNextFire = 0.0F;
    public GameObject missile;
    public GameObject laser;

    public GameObject leftIns;
    public GameObject rightIns;

    public GameObject sun;

    public float sunDemageR = 15f;
    public float sunKillR = 6f;

    private Player player = null;
    float currentShortestDistanceSquare = float.MaxValue;
    MinimalPlanet selectedTarget = null;

    public GameObject front;

    void Start()
    {
        targets = new List<MinimalPlanet>(Resources.FindObjectsOfTypeAll<MinimalPlanet>());
        player = GetComponent<Player>();
    }

    void Update()
    {
        //Debug.Log(Vector3.Distance(transform.position, sun.transform.position));

        if (Input.GetKey(KeyCode.Q)) {
			focus = false;
			transform.parent = null;
		}

		if (Input.GetKey(KeyCode.R)) {
			focus = true;
			transform.parent = selectedTarget.transform;
//			updateClosestTarget();
			Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1);
		}
			
        if (focus)
        {
            movementFocusedOnTarget();
        }
        else {
            freeRoamingMovement();
        }


        updateClosestTarget();

        if (Vector3.Distance(front.transform.position, sun.transform.position) <= sunDemageR)
            player.TakeDamage(1);

        if (currentShortestDistanceSquare <= 20 * selectedTarget.planet_radii)
            player.TakeDamage(0);

        if (Vector3.Distance(front.transform.position, sun.transform.position) <= sunKillR || currentShortestDistanceSquare <= 5 * selectedTarget.planet_radii)
            player.Death();
    }

    private void updateClosestTarget()
    {
        //        float currentShortestDistanceSquare = float.MaxValue;
        //        MinimalPlanet selectedTarget = null;
        currentShortestDistanceSquare = float.MaxValue;
        foreach (MinimalPlanet targetCandidate in targets)
        {
            Vector3 offset = targetCandidate.transform.position - front.transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (currentShortestDistanceSquare > sqrLen)
            {
                currentShortestDistanceSquare = sqrLen;
                selectedTarget = targetCandidate;
            }
        }
		if (target != selectedTarget && focus) {
			transform.parent = selectedTarget.transform;
		}
        target = selectedTarget;
 
    }

    private bool shouldFocusOnTarget()
    {
        return true;
    }

    private void freeRoamingMovement()
    {
      if(Input.GetKey(KeyCode.Space) && Time.time > laserNextFire)
        {
            //AudioSource.PlayClipAtPoint(missileAudio, transform.position);

            laserNextFire = Time.time + laserFireRate;
            GameObject laser_tmp = (GameObject)Instantiate(laser, leftIns.transform.position, Quaternion.LookRotation(transform.forward));
            GameObject laser_tmp2 = (GameObject)Instantiate(laser, rightIns.transform.position, Quaternion.LookRotation(transform.forward));
            laser_tmp.GetComponent<VolumetricLineBehavior>().m_lineColor = Color.red;
            laser_tmp2.GetComponent<VolumetricLineBehavior>().m_lineColor = Color.red;
            laser_tmp.GetComponent<Rigidbody>().velocity = transform.forward * laserspeed;
            laser_tmp2.GetComponent<Rigidbody>().velocity = transform.forward * laserspeed;

            //GetComponent<Player>().TakeDamage(1);
        }


        if (Input.GetKey(KeyCode.S))
        {
            transform.Rotate(Vector3.right * freeRoamingRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Rotate(Vector3.left * freeRoamingRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up * freeRoamingRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.down * freeRoamingRotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.position += transform.forward * freeRoamingMovementSpeed * Time.deltaTime;
        }

		if (Input.GetKey(KeyCode.DownArrow))
		{
			transform.position -= transform.forward * freeRoamingMovementSpeed * Time.deltaTime;
		}

    }

    private void movementFocusedOnTarget()
	{
		bool rot = true;
		if (Input.GetKey(KeyCode.D)) {
			transform.RotateAround(target.transform.position,
				transform.TransformDirection(Vector3.up)
				, angle * Time.deltaTime);
			rot = false;
		}

		if (Input.GetKey(KeyCode.A)) {
			transform.RotateAround(target.transform.position,
				transform.TransformDirection(Vector3.up)
				, - angle * Time.deltaTime);
			rot = false;
		}

		if (Input.GetKey(KeyCode.W)) {
			Debug.Log(transform.forward - (target.transform.position - transform.position));
			transform.RotateAround(target.transform.position, 
				transform.TransformDirection(Vector3.right)
				, angle * Time.deltaTime);
			rot = false;
		}

		if (Input.GetKey(KeyCode.S)) {
			transform.RotateAround(target.transform.position, 
				transform.TransformDirection(Vector3.right)
				,-angle * Time.deltaTime);
			rot = false;
		}

		if (Input.GetKey(KeyCode.UpArrow)) {
			transform.position += transform.forward * focusedLinearSpeed * Time.deltaTime;
		}

		if (Input.GetKey(KeyCode.DownArrow)) {
			transform.position -= transform.forward * focusedLinearSpeed * Time.deltaTime;
		}

		if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFire)
        {
            AudioSource.PlayClipAtPoint(missileAudio, transform.position);

            nextFire = Time.time + fireRate;
			GameObject mmissile = (GameObject)Instantiate(missile, transform.position - transform.up * 2, Quaternion.LookRotation(transform.up));

            mmissile.GetComponent<Rigidbody>().velocity = transform.forward * missilespeed;
        }

		if (rot) {
			Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.2f);
		}
    }
}
