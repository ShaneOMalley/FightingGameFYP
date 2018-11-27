using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlock : MonoBehaviour {

    public Transform Spawn;
    public Transform Despawn;

    public float AnimSpeedMin = 0.95f;
    public float AnimSpeedMax = 1.05f;

    public float SpeedForward = 0.15f;

    private const string GLOBALS_NAME = "Globals";
    private static Globals _globals;

	// Use this for initialization
	void Start ()
    {
        if (_globals == null)
        {
            _globals = Resources.Load<Globals>(GLOBALS_NAME);
        }

        foreach (Bird bird in GetComponentsInChildren<Bird>())
        {
            bird.SetBaseAnimSpeed(Random.Range(AnimSpeedMin, AnimSpeedMax));
        }
	}

    private void Update()
    {
        transform.Translate(SpeedForward * _globals.TimeScale, 0, 0, Space.Self);
        
        if (transform.position.x >= Despawn.position.x)
        {
            transform.position = new Vector3(Spawn.position.x, Spawn.position.y, transform.position.z);
        }
    }
}
