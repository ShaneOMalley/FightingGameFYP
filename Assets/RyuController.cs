using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyuController : CharacterSpecificController {

    // Trigger names
    private const string TRIGGER_RYU_ATTACK = "play_attack";
    private const string TRIGGER_RYU_KICK = "play_kick";
    private const string TRIGGER_RYU_SWEEP = "play_sweep";
    private const string TRIGGER_RYU_CROUCH_PUNCH = "play_crouch_punch";
    private const string TRIGGER_RYU_OVERHEAD = "play_overhead";
    private const string TRIGGER_RYU_SHORYU = "play_shoryu";
    private const string TRIGGER_RYU_THROW_FIREBALL = "play_throw_fireball";
    private const string TRIGGER_RYU_JUMP_KICK = "play_jump_kick";

    public override void Start()
    {
        base.Start();

        StuckAnims = new string[]{
            "ryu_attack", "ryu_kick", "ryu_sweep", "ryu_overhead", "ryu_shoryu",
            "ryu_throw_fireball", "ryu_throw_initial", "ryu_throw_whiff",
            "ryu_throw_success", "ryu_throw_back_success", "ryu_thrown", "ryu_throw_tech",
            "ryu_crouch_punch", "ryu_knockdown_air", "ryu_knockdown_ground",
            "ryu_knockdown_getup", "ryu_victory",
        };

        AirStuckAnims = new string[]
        {
            "ryu_jump_kick", "ryu_flip_out"
        };

        MovingAnims = new string[]
        {
            "ryu_shoryu"
        };

        IgnoreGravityAirborneAnims = new string[]
        {
            "ryu_shoryu"
        };
}

    public override void HandleAttacking(PlayerController player, float playerTimeScale)
    {
        // Reset triggers for buffered special moves
        if (!player.IsStuck)
        {
            player.ResetTrigger(TRIGGER_RYU_SHORYU);
            player.ResetTrigger(TRIGGER_RYU_THROW_FIREBALL);
        }

        // Attacking
        if (!player.IsStuck || player.InSpecialCancelWindow)
        {
            // Specials
            if (player.GetButtonDownWithBuffer("Special"))
            {
                if (player.HoldingForward)
                {
                    player.FireTrigger(TRIGGER_RYU_SHORYU);
                }
                else if (player.HoldingBack)
                {

                }
                else
                {
                    player.FireTrigger(TRIGGER_RYU_THROW_FIREBALL);
                }
            }
        }
        if (!player.IsStuck)
        {
            // Standing Attacks
            if (player.IsStanding)
            {
                if (player.GetButtonDownWithBuffer("Attack"))
                {
                    if (player.HoldingForward)
                    {
                        player.FireTrigger(TRIGGER_RYU_OVERHEAD);
                    }
                    else
                    {
                        player.FireTrigger(TRIGGER_RYU_ATTACK);
                    }
                }
                else if (player.GetButtonDownWithBuffer("Kick"))
                {
                    player.FireTrigger(TRIGGER_RYU_KICK);
                }
                else if (player.GetButtonDownWithBuffer("Throw"))
                {
                    player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                    _animator.SetBool("throw_back", player.HoldingBack);
                }
            }
            // Crouching Attacks
            else if (player.IsCrouching)
            {
                if (player.GetButtonDownWithBuffer("Special"))
                {
                    if (player.HoldingForward)
                    {
                        player.FireTrigger(TRIGGER_RYU_SHORYU);
                    }
                }
                else if (player.GetButtonDownWithBuffer("Attack"))
                {
                    player.FireTrigger(TRIGGER_RYU_CROUCH_PUNCH);
                }
                else if (player.GetButtonDownWithBuffer("Kick"))
                {
                    player.FireTrigger(TRIGGER_RYU_SWEEP);
                }
                else if (player.GetButtonDownWithBuffer("Throw"))
                {
                    player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                    _animator.SetBool("throw_back", player.HoldingBack);
                }
            }
        }
        // Air attacks
        if (player.IsAirborne && !player.IsAirStuck)
        {
            if (player.GetButtonDownWithBuffer("Kick"))
            {
                player.FireTrigger(TRIGGER_RYU_JUMP_KICK);
            }
        }
    }
}
