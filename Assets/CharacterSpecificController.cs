using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSpecificController : MonoBehaviour {

    public string[] StuckAnims;
    public string[] AirStuckAnims;
    public string[] MovingAnims;
    public string[] IgnoreGravityAirborneAnims;

    protected PlayerController _player;
    protected Animator _animator;

    public virtual void FixedUpdate ()
    {
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        if (_player == null)
        {
            _player = GetComponent<PlayerController>();
        }
    }

    public abstract void HandleAttacking(float playerTimeScale);
}
