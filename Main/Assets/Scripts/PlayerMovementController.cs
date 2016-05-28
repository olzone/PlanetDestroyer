using UnityEngine;
using System.Collections.Generic;

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
    public float fireRate = 2.0F;
    private float nextFire = 0.0F;
    public GameObject missile;

    void Start()
    {
        targets = new List<MinimalPlanet>(Resources.FindObjectsOfTypeAll<MinimalPlanet>());
    }

    void Update()
    {
		if (Input.GetKey(KeyCode.Q)) {
			focus = false;
		}

		if (Input.GetKey(KeyCode.R)) {
			focus = true;
			updateClosestTarget();
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
    }

    private void updateClosestTarget()
    {
        float currentShortestDistanceSquare = float.MaxValue;
        MinimalPlanet selectedTarget = null;
        foreach (MinimalPlanet targetCandidate in targets)
        {
            Vector3 offset = targetCandidate.transform.position - transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (currentShortestDistanceSquare > sqrLen)
            {
                currentShortestDistanceSquare = sqrLen;
                selectedTarget = targetCandidate;
            }
        }
        target = selectedTarget;
 
    }

    private bool shouldFocusOnTarget()
    {
        return true;
    }

    private void freeRoamingMovement()
    {
		
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
