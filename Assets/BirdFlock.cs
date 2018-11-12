using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlock : MonoBehaviour {

    public Transform Spawn;
    public Transform Despawn;

    public float AnimSpeedMin = 0.95f;
    public float AnimSpeedMax = 1.05f;

    public float SpeedForward = 0.15f;

	// Use this for initialization
	void Start ()
    {
        foreach (Bird bird in GetComponentsInChildren<Bird>())
        {
            bird.ApplySpeed(Random.Range(AnimSpeedMin, AnimSpeedMax));
        }
	}

    private void Update()
    {
        transform.Translate(SpeedForward, 0, 0, Space.Self);

        // Reset position of birds once they reach the despawn point
        if (transform.position.x >= Despawn.position.x)
        {
            transform.position = new Vector3(Spawn.position.x, Spawn.position.y, transform.position.z);
        }
    }
}
