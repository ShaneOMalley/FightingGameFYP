using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public AttackData AttackData;

    // Will the attack animation hit only once even with hitboxes on multiple frames?
    public bool AnimationHitOnce = true;
}
