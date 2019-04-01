using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharlieController : CharacterSpecificController {

    public float JumpFierceVelYThreshold = 0f;
    public float StandKickMoveSpeed = 0.05f;
    public bool CanMoveStandKick = false;

    public Projectile SlowFireballPrefab;
    public Projectile MediumFireballPrefab;
    public Projectile FastFireballPrefab;

    // Trigger names
    private const string TRIGGER_CHARLIE_STAND_PUNCH = "play_stand_punch";
    private const string TRIGGER_CHARLIE_STAND_KICK = "play_stand_kick";
    private const string TRIGGER_CHARLIE_CROUCH_PUNCH = "play_crouch_punch";
    private const string TRIGGER_CHARLIE_CROUCH_KICK = "play_crouch_kick";
    private const string TRIGGER_CHARLIE_JUMP_FIERCE = "play_jump_fierce";
    private const string TRIGGER_CHARLIE_JUMP_STRONG = "play_jump_strong";
    private const string TRIGGER_CHARLIE_JUMP_KICK = "play_jump_kick";
    private const string TRIGGER_CHARLIE_THROW_FIREBALL = "play_throw_fireball";
    private const string TRIGGER_CHARLIE_THROW_FIREBALL_TURN = "play_throw_fireball_turn";
    private const string TRIGGER_CHARLIE_THROW_FIREBALL_UPPER = "play_throw_fireball_upper";


    public void Start()
    {
        StuckAnims = new string[] {
            "charlie_throw_initial", "charlie_throw_whiff", "charlie_throw_success",
            "charlie_throw_back_success", "throw_tech", "thrown",
            "charlie_stand_punch", "charlie_stand_kick", "charlie_crouch_kick",
            "charlie_crouch_punch", "charlie_jump_strong", "charlie_jump_fierce",
            "charlie_throw_fireball", "charlie_throw_fireball_turn",
            "charlie_throw_fireball_upper", "charlie_throw_fireball_cancelled",
        };

        AirStuckAnims = new string[] {
            "charlie_jump_strong", "charlie_jump_fierce", "charlie_jump_kick",
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

        // Buffering special moves for special-cancellable normals
        if (_player.IsStuck && _player.InSpecialCancelWindow)
        {
            // Specials
            if (_player.GetButtonDownWithBuffer("Special") && !_player.FireballExists())
            {
                if (_player.HoldingBack)
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_UPPER);
                }
                else
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL);
                }
            }
        }
        // Attacking
        else if (!_player.IsStuck)
        {
            // Reset triggers for buffered special moves
            _player.ResetTrigger(TRIGGER_CHARLIE_THROW_FIREBALL);
            _player.ResetTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_TURN);
            _player.ResetTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_UPPER);

            // Standing Attacks
            if (_player.IsStanding)
            {
                if (_player.GetButtonDownWithBuffer("Special") && !_player.FireballExists())
                {
                    if (_player.HoldingForward && false)
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_TURN);
                    }
                    else if (_player.HoldingBack)
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_UPPER);
                    }
                    else
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL);
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_STAND_PUNCH);
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_STAND_KICK);
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
                if (_player.GetButtonDownWithBuffer("Special") && !_player.FireballExists())
                {
                    if (_player.HoldingForward && false)
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_TURN);
                    }
                    else if (_player.HoldingBack)
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL_UPPER);
                    }
                    else
                    {
                        _player.FireTrigger(TRIGGER_CHARLIE_THROW_FIREBALL);
                    }
                }
                else if (_player.GetButtonDownWithBuffer("Attack"))
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_CROUCH_PUNCH);
                }
                else if (_player.GetButtonDownWithBuffer("Kick"))
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_CROUCH_KICK);
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
                if (_player.VelY < JumpFierceVelYThreshold)
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_JUMP_FIERCE);
                }
                else
                {
                    _player.FireTrigger(TRIGGER_CHARLIE_JUMP_STRONG);
                }
            }
            else if (_player.GetButtonDownWithBuffer("Kick"))
            {
                _player.FireTrigger(TRIGGER_CHARLIE_JUMP_KICK);
            }
        }

        // StandKick Movement
        if (CanMoveStandKick)
        {
            if (_player.HoldingForward)
            {
                _player.transform.Translate(playerTimeScale * StandKickMoveSpeed * (_player.IsFacingRight ? 1 : -1) * Utils.TimeCorrection, 0, 0);
            }
            else if (_player.HoldingBack)
            {
                _player.transform.Translate(playerTimeScale * -StandKickMoveSpeed* (_player.IsFacingRight ? 1 : -1) * Utils.TimeCorrection, 0, 0);
            }
        }
    }

    public void ThrowSlowFireball()
    {
        _player.ThrowFireball(SlowFireballPrefab);
    }

    public void ThrowMediumFireball()
    {
        _player.ThrowFireball(MediumFireballPrefab);
    }

    public void ThrowFastFireball()
    {
        _player.ThrowFireball(FastFireballPrefab);
    }
}
