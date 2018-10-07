using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int Damage = 1;
    public float KnocbackHit = 0.5f;
    public float KnockbackBlock = 0.3f;
    public float PushbackHit = 0.2f;
    public float PushbackBlock = 0.2f;

    public int Hitstun = 0;
    public int Blockstun = 0;
}
