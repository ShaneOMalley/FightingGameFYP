using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public enum HitLevel { HIGH, LOW, MID };
    public HitLevel Level = HitLevel.HIGH;

    public int Damage = 1;
    public float KnockbackHit = 0.5f;
    public float KnockbackBlock = 0.3f;
    public float PushbackHit = 0.2f;
    public float PushbackBlock = 0.2f;

    public int Hitstun = 0;
    public int Blockstun = 0;

    public bool KnocksDownGround = false;
    public bool KnocksDownAir = false;
    public float KnockdownLaunch = 0;
    public float KnockdownGravity = 0;
    public int KnockdownGroundFrames = 0;
}
