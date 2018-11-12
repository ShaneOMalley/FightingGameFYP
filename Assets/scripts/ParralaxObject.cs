using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParralaxObject : MonoBehaviour
{
    public float FactorX = 1f;
    public float FactorY = 1f;

    private Vector3 _parentPrevPos;
    private Vector3 _parentInitialPos;
    private Vector3 _initialPosLocal;

    // Use this for initialization
    void Start ()
    {
        _parentPrevPos = transform.parent.position;
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 deltaPos = transform.parent.position - _parentPrevPos;
        transform.Translate(Vector3.Scale(deltaPos, new Vector3(-FactorX, -FactorY, 1)));
        _parentPrevPos = transform.parent.position;

    }
}
