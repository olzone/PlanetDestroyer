using UnityEngine;
using System.Collections.Generic;

public class PlayerMovementController : MonoBehaviour {

    public SubdivisionSurfacesPlanet target;
    public static List<SubdivisionSurfacesPlanet> targets = null;
    public float freeRoamingRotationSpeed;
    public float freeRoamingMovementSpeed;
    public float focusedLinearSpeed;
    public float angle;
    public float switchControllDistance;


    public AudioClip missileAudio;
    public float missilespeed = 10f;
    public float fireRate = 2.0F;
    private float nextFire = 0.0F;
    public GameObject missile;

    void Start()
    {
        targets = new List<SubdivisionSurfacesPlanet>(Resources.FindObjectsOfTypeAll<SubdivisionSurfacesPlanet>());
    }

    void Update()
    {
        if (updateClosestTarget())
        {
            movementFocusedOnTarget();
        }
        else {
            freeRoamingMovement();
        }
    }

    private bool updateClosestTarget()
    {
        float currentShortestDistanceSquare = float.MaxValue;
        SubdivisionSurfacesPlanet selectedTarget = null;
        foreach (SubdivisionSurfacesPlanet targetCandidate in targets)
        {
            Vector3 offset = targetCandidate.transform.position - transform.position;
            float sqrLen = offset.sqrMagnitude;
            if (currentShortestDistanceSquare > sqrLen)
            {
                currentShortestDistanceSquare = sqrLen;
                selectedTarget = targetCandidate;
            }
        }

        if (currentShortestDistanceSquare < switchControllDistance * switchControllDistance)
        {
            target = selectedTarget;
            return true;
        }
        return false;
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

        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.forward * freeRoamingMovementSpeed * Time.deltaTime;
        }

    }

    private void movementFocusedOnTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 1);

        if (Input.GetKey(KeyCode.D))
        {
            transform.RotateAround(target.transform.position, -target.transform.up, angle * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.RotateAround(target.transform.position, target.transform.up, angle * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.RotateAround(target.transform.position, target.transform.right, angle * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.RotateAround(target.transform.position, -target.transform.right, angle * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += transform.forward * focusedLinearSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.V))
        {
            transform.position -= transform.forward * focusedLinearSpeed * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.R) && Time.time > nextFire)
        {
            AudioSource.PlayClipAtPoint(missileAudio, transform.position);

            nextFire = Time.time + fireRate;
            GameObject mmissile = (GameObject)Instantiate(missile, transform.position - transform.up * 2, Quaternion.LookRotation(transform.up));

            mmissile.GetComponent<Rigidbody>().velocity = transform.forward * missilespeed;
        }
    }
}
