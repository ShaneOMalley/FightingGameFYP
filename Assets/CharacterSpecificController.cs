using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterSpecificController : MonoBehaviour {

    public string[] StuckAnims;
    public string[] AirStuckAnims;
    public string[] MovingAnims;
    public string[] IgnoreGravityAirborneAnims;

    protected Animator _animator;

    public virtual void Start ()
    {
        _animator = GetComponent<Animator>();
    }

    public abstract void HandleAttacking(PlayerController player, float playerTimeScale);
}
