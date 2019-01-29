using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "ScriptableObjects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public Vector3 Velocity = new Vector3(0.12f, 0, 0);

    public int Damage = 1;
    public float KnockbackHit = 0.5f;
    public float KnockbackBlock = 0.3f;

    public int Hitstun = 0;
    public int Blockstun = 0;

    public bool KnocksDownGround = false;
    public bool KnocksDownAir = false;
    public float KnockdownLaunch = 0;
    public float KnockdownGravity = 0;
    public int KnockdownGroundFrames = 0;
}
