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
    private const string TRIGGER_RYU_JUMP_PUNCH = "play_jump_punch";

    public void Start()
    {
        StuckAnims = new string[]{
            "ryu_attack", "ryu_kick", "ryu_sweep", "ryu_overhead", "ryu_shoryu",
            "ryu_throw_fireball", "ryu_throw_fireball_cancelled", "ryu_throw_initial",
            "ryu_throw_whiff", "ryu_throw_success", "ryu_throw_back_success", "thrown",
            "throw_tech", "ryu_crouch_punch", "ryu_knockdown_air", "ryu_knockdown_ground",
            "ryu_knockdown_getup", "ryu_victory",
        };

        AirStuckAnims = new string[]
        {
            "ryu_jump_kick", "ryu_jump_punch", "ryu_flip_out"
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

    public override void HandleAttacking(float playerTimeScale)
    {
        if (_player == null || _animator == null)
        {
            return;
        }

        // Reset triggers for buffered special moves
        if (!_player.IsStuck)
        {
            _player.ResetTrigger(TRIGGER_RYU_SHORYU);
            _player.ResetTrigger(TRIGGER_RYU_THROW_FIREBALL);
        }

        // Attacking
        if (_player.IsStuck && _player.InSpecialCancelWindow)
        {
            // Special Cancels
            if (_player.GetButtonDownWithBuffer("Special") && !_player.FireballExists())
            {
                _player.FireTrigger(TRIGGER_RYU_THROW_FIREBALL);
            }
        }
        if (!_player.IsStuck)
        {
            // Standing Attacks
            if (_player.IsStanding)
            {
                if (_player.GetButtonDownWithBuffer("Special"))
                {
                    if (_player.HoldingForward)
                    {
                        _player.FireTrigger(TRIGGER_RYU_SHORYU);
                    }
                    else if (!_player.FireballExists())
                    {
                        _player.FireTrigger(TRIGGER_RYU_THROW_FIREBALL);
                    }
                }
                if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    if (_player.HoldingForward)
                    {
                        _player.FireTrigger(TRIGGER_RYU_OVERHEAD);
                    }
                    else
                    {
                        _player.FireTrigger(TRIGGER_RYU_ATTACK);
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_RYU_KICK);
                }
                else if (_player.GetButtonDownWithBuffer("Throw"))
                {
                    _player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                    _animator.SetBool("throw_back", _player.HoldingBack);
                }
            }
            // Crouching Attacks
            else if (_player.IsCrouching)
            {
                if (_player.GetButtonDownWithBuffer("Special"))
                {
                    if (_player.HoldingForward)
                    {
                        _player.FireTrigger(TRIGGER_RYU_SHORYU);
                    }
                    else if (!_player.FireballExists())
                    {
                        _player.FireTrigger(TRIGGER_RYU_THROW_FIREBALL);
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    _player.FireTrigger(TRIGGER_RYU_CROUCH_PUNCH);
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_RYU_SWEEP);
                }
                else if (_player.GetButtonDownWithBuffer("Throw"))
                {
                    _player.FireTrigger(PlayerController.TRIGGER_PLAYER_THROW_INITIAL);
                    _animator.SetBool("throw_back", _player.HoldingBack);
                }
            }
        }
        // Air attacks
        if (_player.IsAirborne && !_player.IsAirStuck)
        {
            if (_player.GetButtonDownWithBuffer("Attack"))
            {
                _player.FireTrigger(TRIGGER_RYU_JUMP_PUNCH);
            }
            else if (_player.GetButtonDownWithBuffer("Kick"))
            {
                _player.FireTrigger(TRIGGER_RYU_JUMP_KICK);
            }
        }
    }
}
