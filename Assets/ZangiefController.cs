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


    public void Start()
    {
        StuckAnims = new string[] {
            "zangief_towards_punch", "zangief_stand_punch", "zangief_stand_kick",
            "zangief_throw_initial", "zangief_throw_success", "zangief_throw_back_success",
            "zangief_throw_whiff", "throw_tech", "thrown",
            "zangief_crouch_punch", "zangief_crouch_kick", "zangief_jump_punch",
            "zangief_jump_kick", "zangief_lariat"
        };

        AirStuckAnims = new string[] {
            "zangief_jump_punch", "zangief_jump_kick"
        };

        MovingAnims = new string[] {
        };

        IgnoreGravityAirborneAnims = new string[] {
        };
}

    public override void HandleAttacking(float playerTimeScale)
    {
        if (_player == null || _animator == null)
        {
            return;
        }

        // Reset triggers for buffered special moves
        if (!_player.IsStuck)
        {
            _player.ResetTrigger(TRIGGER_ZANGIEF_LARIAT);
        }

        // Attacking
        if (!_player.IsStuck || _player.InSpecialCancelWindow)
        {
            // Specials
            if (_player.GetButtonDownWithBuffer("Special"))
            {
                _player.FireTrigger(TRIGGER_ZANGIEF_LARIAT);
            }
        }
        if (!_player.IsStuck)
        {
            // Standing Attacks
            if (_player.IsStanding)
            {
                if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    if (_player.HoldingForward)
                    {
                        _player.FireTrigger(TRIGGER_ZANGIEF_TOWARDS_PUNCH);
                    }
                    else
                    {
                        _player.FireTrigger(TRIGGER_ZANGIEF_STAND_PUNCH);
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_ZANGIEF_STAND_KICK);
                }
                else if (_player.GetButtonDownWithBuffer("Throw"))
                {
                    _animator.SetBool("throw_back", _player.HoldingBack);
                    _player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                }
            }
            // Crouching Attacks
            else if (_player.IsCrouching)
            {
                if (_player.GetButtonDownWithBuffer("Special"))
                {
                    if (_player.HoldingForward)
                    {
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    _player.FireTrigger(TRIGGER_ZANGIEF_CROUCH_PUNCH);
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_ZANGIEF_CROUCH_KICK);
                }
                else if (_player.GetButtonDownWithBuffer("Throw"))
                {
                    _animator.SetBool("throw_back", _player.HoldingBack);
                    _player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                }
            }
        }
        // Air attacks
        if (_player.IsAirborne && !_player.IsAirStuck)
        {
            if (_player.GetButtonDownWithBuffer("Attack"))
            {
                _player.FireTrigger(TRIGGER_ZANGIEF_JUMP_PUNCH);
            }
            else if (_player.GetButtonDownWithBuffer("Kick"))
            {
                _player.FireTrigger(TRIGGER_ZANGIEF_JUMP_KICK);
            }
        }

        // Lariat Movement
        if (CanMoveLariat)
        {
            if (_player.HoldingForward)
            {
                _player.transform.Translate(playerTimeScale * LariatMoveSpeed * (_player.IsFacingRight ? 1 : -1) * Utils.TimeCorrection, 0, 0);
            }
            //else if (player.HoldingBack)
            //{
            //    player.transform.Translate(playerTimeScale * -LariatMoveSpeed * (player.IsFacingRight ? 1 : -1), 0, 0);
            //}
        }
    }
}
