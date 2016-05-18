using UnityEngine;
using System.Collections;

public class Prewarm : MonoBehaviour {



    // Use this for initialization
    void Start () {
        Preheat(((ParticleEmitter) transform.GetComponent("EllipsoidParticleEmitter")));

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void Preheat(ParticleEmitter emitter, float time = 0f, float timeStep = 0.02f)
    {
        // Default preheat time is the longest particle lifetime, which is the
        // latest the particle system should have reached is max particle count
        if (time <= 0f)
        {
            time = emitter.maxEnergy*1;
        }

        float emission = 0f;
        for (float currentTime = 0f; currentTime < time; currentTime += timeStep)
        {
            // Accumulate emission as we might emit less than one particle per step
            // and wouldn't emit at all if we don't accumulate over multiple steps
            emission += UnityEngine.Random.Range(emitter.minEmission, emitter.maxEmission) * timeStep;
            emitter.Emit((int)emission);
            emission = emission % 1f;
            emitter.Simulate(timeStep);
        }
    }
}
