using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour {

    private float _baseAnimSpeed;
    private const string GLOBALS_NAME = "Globals";
    private static Globals _globals;
    private Animator _animator;

    public void Start()
    {
        _baseAnimSpeed = 1;
        _animator = gameObject.GetComponent<Animator>();

        if (_globals == null)
        {
            _globals = Resources.Load<Globals>(GLOBALS_NAME);
        }
    }

    public void SetBaseAnimSpeed(float speed)
    {
        _baseAnimSpeed = speed;
    }

    public void Update()
    {
        _animator.SetFloat("RunSpeed", _baseAnimSpeed * _globals.TimeScale);
    }
}
