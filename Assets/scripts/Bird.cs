using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    public void ApplySpeed(float speed)
    {
        Animator animator = gameObject.GetComponent<Animator>();
        animator.SetFloat("RunSpeed", speed);
    }
}
