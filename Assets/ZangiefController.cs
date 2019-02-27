using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZangiefController : CharacterSpecificController {

    public float LariatMoveSpeed = 0.03f;
    public bool CanMoveLariat = false;

    // Trigger names
    private const string TRIGGER_ZANGIEF_TOWARDS_PUNCH = "play_towards_punch";
    private const string TRIGGER_ZANGIEF_STAND_PUNCH = "play_stand_punch";
    private const string TRIGGER_ZANGIEF_STAND_KICK = "play_stand_kick";
    private const string TRIGGER_ZANGIEF_CROUCH_PUNCH = "play_crouch_punch";
    private const string TRIGGER_ZANGIEF_CROUCH_KICK = "play_crouch_kick";
    private const string TRIGGER_ZANGIEF_JUMP_PUNCH = "play_jump_punch";
    private const string TRIGGER_ZANGIEF_JUMP_KICK = "play_jump_kick";
    private const string TRIGGER_ZANGIEF_LARIAT = "play_lariat";


    public override void Start()
    {
        base.Start();

        StuckAnims = new string[] {
            "zangief_towards_punch", "zangief_stand_punch", "zangief_stand_kick",
            "zangief_throw_initial", "zangief_throw_success", "zangief_throw_whiff",
            "zangief_throw_back_success", "zangief_crouch_punch", "zangief_crouch_kick",
            "zangief_jump_punch", "zangief_jump_kick", "zangief_lariat"
        };

        AirStuckAnims = new string[] {
            "zangief_jump_punch", "zangief_jump_kick"
        };

        MovingAnims = new string[] {
        };

        IgnoreGravityAirborneAnims = new string[] {
        };
}

    public override void HandleAttacking(PlayerController player, float playerTimeScale)
    {
        // Reset triggers for buffered special moves
        if (!player.IsStuck)
        {
            player.ResetTrigger(TRIGGER_ZANGIEF_LARIAT);
        }

        // Attacking
        if (!player.IsStuck || player.InSpecialCancelWindow)
        {
            // Specials
            if (player.GetButtonDownWithBuffer("Special"))
            {
                player.FireTrigger(TRIGGER_ZANGIEF_LARIAT);
                //if (player.HoldingForward)
                //{
                //}
                //else if (player.HoldingBack)
                //{
                //}
                //else
                //{
                //}
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
                        player.FireTrigger(TRIGGER_ZANGIEF_TOWARDS_PUNCH);
                    }
                    else
                    {
                        player.FireTrigger(TRIGGER_ZANGIEF_STAND_PUNCH);
                    }
                }
                else if (player.GetButtonDownWithBuffer("Kick"))
                {
                    player.FireTrigger(TRIGGER_ZANGIEF_STAND_KICK);
                }
                else if (player.GetButtonDownWithBuffer("Throw"))
                {
                    _animator.SetBool("throw_back", player.HoldingBack);
                    player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                }
            }
            // Crouching Attacks
            else if (player.IsCrouching)
            {
                if (player.GetButtonDownWithBuffer("Special"))
                {
                    if (player.HoldingForward)
                    {
                    }
                }
                else if (player.GetButtonDownWithBuffer("Attack"))
                {
                    player.FireTrigger(TRIGGER_ZANGIEF_CROUCH_PUNCH);
                }
                else if (player.GetButtonDownWithBuffer("Kick"))
                {
                    player.FireTrigger(TRIGGER_ZANGIEF_CROUCH_KICK);
                }
                else if (player.GetButtonDownWithBuffer("Throw"))
                {
                    _animator.SetBool("throw_back", player.HoldingBack);
                    player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                }
            }
        }
        // Air attacks
        if (player.IsAirborne && !player.IsAirStuck)
        {
            if (player.GetButtonDownWithBuffer("Attack"))
            {
                player.FireTrigger(TRIGGER_ZANGIEF_JUMP_PUNCH);
            }
            else if (player.GetButtonDownWithBuffer("Kick"))
            {
                player.FireTrigger(TRIGGER_ZANGIEF_JUMP_KICK);
            }
        }

        // Lariat Movement
        if (CanMoveLariat)
        {
            if (player.HoldingForward)
            {
                player.transform.Translate(playerTimeScale * LariatMoveSpeed * (player.IsFacingRight ? 1 : -1), 0, 0);
            }
            //else if (player.HoldingBack)
            //{
            //    player.transform.Translate(playerTimeScale * -LariatMoveSpeed * (player.IsFacingRight ? 1 : -1), 0, 0);
            //}
        }
    }
}
