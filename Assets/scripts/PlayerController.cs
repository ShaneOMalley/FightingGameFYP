using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameplayData Data;

    public int MaxHP = 100;
    private int _hp;

    public float WalkSpeedForward = 0.06f;
    public float WalkSpeedBackward = 0.035f;

    public float JumpPower = 2f;
    public float JumpSpeedHorizontal = 0.15f;
    public float JumpGravity = 0.05f;

    private float _initialY;
    private float _gravity = 0;
    private float _velY = 0;
    private float _jumpVelX = 0;

    public GameCamera CurrentGameCamera;
    public PlayerController OtherPlayer;
    public GameObject HitSpark;
    public GameObject BlockSpark;
    public AttackData RoundEndDefault;

    public enum PlayerEnum { P1, P2 }
    public PlayerEnum PlayerNum = PlayerEnum.P1;

    private enum State {
        STATE_IDLE = 0,
        STATE_WALK = 1,
        STATE_HIT = 2,
        STATE_BLOCK = 3,
        STATE_CROUCH = 4,
        STATE_KNOCKDOWN_AIR = 5,
        STATE_KNOCKDOWN_GROUND = 6,
        STATE_KNOCKDOWN_GETUP = 7,
        STATE_PRE_JUMP = 8,
        STATE_NEUTRAL_JUMP = 9,
        STATE_BACK_JUMP = 10,
        STATE_FORWARD_JUMP = 11,
        STATE_FLIP_OUT = 12,
        STATE_JUMP_ATTACK = 13,

        STATE_JUMP_NODECISION = 500,
    }
    private State _state = State.STATE_IDLE;
    private enum Direction { RIGHT, LEFT }

    private const string TRIGGER_RYU_ATTACK = "play_attack";
    private const string TRIGGER_RYU_KICK = "play_kick";
    private const string TRIGGER_RYU_HIT = "play_hit";
    private const string TRIGGER_RYU_BLOCK = "play_block";
    private const string TRIGGER_RYU_CROUCH = "play_crouch";
    private const string TRIGGER_RYU_SWEEP = "play_sweep";
    private const string TRIGGER_RYU_CROUCH_PUNCH = "play_crouch_punch";
    private const string TRIGGER_RYU_OVERHEAD = "play_overhead";
    private const string TRIGGER_RYU_JUMP_KICK = "play_jump_kick";
    private const string TRIGGER_RYU_KNOCKDOWN_AIR = "play_knockdown_air";
    private const string TRIGGER_RYU_KNOCKDOWN_GROUND = "play_knockdown_ground";
    private const string TRIGGER_RYU_KNOCKDOWN_GETUP = "play_knockdown_getup";

    private static string[] StuckAnims = 
    {
        "ryu_attack", "ryu_kick", "ryu_sweep", "ryu_overhead", "ryu_crouch_punch",
        "ryu_knockdown_air", "ryu_knockdown_ground", "ryu_knockdown_getup"
    };

    private static State[] StuckStates =
    {
        State.STATE_HIT, State.STATE_BLOCK, State.STATE_KNOCKDOWN_AIR,
        State.STATE_KNOCKDOWN_GROUND, State.STATE_KNOCKDOWN_GETUP,
        State.STATE_PRE_JUMP, State.STATE_NEUTRAL_JUMP, State.STATE_BACK_JUMP,
        State.STATE_FORWARD_JUMP, State.STATE_FLIP_OUT,
    };

    private static string[] AirStuckAnims =
    {
        "ryu_jump_kick", "ryu_flip_out"
    };

    private static State[] AirStuckStates =
    {
        State.STATE_FLIP_OUT
    };

    private Transform _currentBoxesFrame;
    private Animator _animator;
    private Direction _direction = Direction.RIGHT;
    private bool _movingForward = true;

    private bool _isCrouching = false;
    
    private bool _isAirborne = false;
    private State _jumpDecision = State.STATE_NEUTRAL_JUMP;

    private const float KNOCKBACK_DECELERATION = 0.05f;
    private int _hitstun_frames = 0;
    private int _blockstun_frames = 0;
    private int _frames_on_ground = 0;
    private float _knockback_speed = 0;

    private bool _currentHitboxHit = false;
    private Transform _nextBoxesFrame = null;

    // Properties
    public int HP
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public Transform CurrentBoxesFrame
    {
        get { return _currentBoxesFrame; }
    }

    public BoxCollider2D[] HitBoxes
    {
        get
        {
            Transform boxGroup = CurrentBoxesFrame.Find("hit");
            if (boxGroup == null)
                return null;
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D[] Hurtboxes
    {
        get
        {
            Transform boxGroup = CurrentBoxesFrame.Find("hurt");
            if (boxGroup == null)
                return null;
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D[] ProxBoxes
    {
        get
        {
            Transform boxGroup = CurrentBoxesFrame.Find("prox");
            if (boxGroup == null)
                return null;
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }


    public BoxCollider2D PushBox
    {
        get { return CurrentBoxesFrame.Find("push").GetComponent<BoxCollider2D>(); }
    }

    private bool _isP1
    {
        get { return PlayerNum == PlayerEnum.P1; }
    }

    public string Prefix
    {
        get { return _isP1 ? "P1_" : "P2_"; }
    }

    private bool _isFacingRight
    {
        get { return _direction == Direction.RIGHT; }
    }

    private bool _onLeftSide
    {
        get { return transform.position.x < OtherPlayer.transform.position.x; }
    }

    public bool InHitStun
    {
        get { return _hitstun_frames > 0; }
    }

    public bool InBlockStun
    {
        get { return _blockstun_frames > 0; }
    }

    public bool InAirKnockdown
    {
        get { return _state == State.STATE_KNOCKDOWN_AIR; }
    }

    private bool _inGroundKnockdown
    {
        get { return _state == State.STATE_KNOCKDOWN_GROUND; }
    }

    private bool _inGetup
    {
        get { return _state == State.STATE_KNOCKDOWN_GETUP; }
    }

    private bool _inPreJump
    {
        get { return _state == State.STATE_PRE_JUMP; }
    }

    private bool _isStuck
    {
        get
        {
            // Is it not in gameplay state?
            if (Data.CurrentState != GameplayData.State.GAMEPLAY)
                return true;

            // Is the player attacking?
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in StuckAnims)
                if (stateInfo.IsName(anim_name))
                    return true;

            // Is the player in a stuck state?
            foreach (State state in StuckStates)
                if (_state == state)
                    return true;

            // Is the player in hitstun or blockstun?
            if (InHitStun || InBlockStun)
                return true;

            return false;
        }
    }
    
    public bool _isAirStuck
    {
        get
        {
            // Is the player attacking?
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in AirStuckAnims)
                if (stateInfo.IsName(anim_name))
                    return true;

            // Is the player in a stuck state?
            foreach (State state in AirStuckStates)
                if (_state == state)
                    return true;

            return false;
        }
    }

    // Holding Directions
    private bool HoldingBack
    {
        get
        {
            float horizontal = GetAxis("Horizontal");
            return (_isFacingRight && horizontal < 0)
                || (!_isFacingRight && horizontal > 0);
        }
    }

    public bool HoldingForward
    {
        get { return GetAxis("Horizontal") != 0 && !HoldingBack; }
    }

    public bool HoldingDown
    {
        get { return GetAxis("Vertical") < 0; }
    }

    public bool IsBlocking
    {
        get { return HoldingBack && !_isStuck; }
    }

    public bool IsCrouching
    {
        get { return _isCrouching; }
    }
    
    public bool IsStanding
    {
        get { return !IsCrouching; }
    }

    public bool IsAirborne
    {
        get { return _isAirborne; }
    }

    public bool IsGrounded
    {
        get { return !IsAirborne; }
    }

    // Use this for initialization
    void Start ()
    {
        _animator = this.GetComponent<Animator>();
        _initialY = transform.position.y;
        _hp = MaxHP;
    }

    // Update is called once per frame
    void Update()
    {
        // Set the collision boxes frame
        UpdateSetCollisionBoxesFrame();

        // Jumping
        if (!_isStuck && IsGrounded)
        {
            if (GetAxis("Vertical") > 0)
            {
                SetState(State.STATE_PRE_JUMP);
                SetJumpDecision(State.STATE_JUMP_NODECISION);
            }
        }

        // Branch from Pre-jump to one of three jumping options: neutral, back, forward
        if (_inPreJump && _jumpDecision == State.STATE_JUMP_NODECISION)
        {
            if (GetAxis("Horizontal") * (_isFacingRight ? 1 : -1) < 0)
            {
                SetJumpDecision(State.STATE_BACK_JUMP);
            }
            else if (GetAxis("Horizontal") * (_isFacingRight ? 1 : -1) > 0)
            {
                SetJumpDecision(State.STATE_FORWARD_JUMP);
            }
        }

        // Crouching
        if (!_isStuck)
        {
            if (GetAxis("Vertical") < 0)
            {
                SetState(State.STATE_CROUCH);
                _isCrouching = true;
            }
            else
            {
                SetState(State.STATE_IDLE);
                _isCrouching = false;
            }

            SetCrouching(_isCrouching);
        }

        // Movement
        if (!_isStuck && !_isCrouching)
        {
            if (GetAxis("Horizontal") > 0)
            {
                SetState(State.STATE_WALK);

                SetForward(_isFacingRight);
                float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

                transform.Translate(speed, 0, 0);
            }
            else if (GetAxis("Horizontal") < 0)
            {
                SetState(State.STATE_WALK);

                SetForward(!_isFacingRight);
                float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

                transform.Translate(-speed, 0, 0);
            }
            else
            {
                SetState(State.STATE_IDLE);
            }
        }

        // Attacking
        if (!_isStuck)
        {
            // Standing Attacks
            if (IsStanding)
            {
                if (GetButtonDown("Attack"))
                {
                    if (HoldingForward)
                    {
                        FireTrigger(TRIGGER_RYU_OVERHEAD);
                    }
                    else
                    {
                        FireTrigger(TRIGGER_RYU_ATTACK);
                    }
                }
                else if (GetButtonDown("Kick"))
                {
                    FireTrigger(TRIGGER_RYU_KICK);
                }
            }
            // Crouching Attacks
            else if (IsCrouching)
            {
                if (GetButtonDown("Attack"))
                {
                    FireTrigger(TRIGGER_RYU_CROUCH_PUNCH);
                }
                else if (GetButtonDown("Kick"))
                {
                    FireTrigger(TRIGGER_RYU_SWEEP);
                }
            }
        }

        // Air attacks
        if (IsAirborne && !_isAirStuck)
        {
            if (GetButtonDown("Kick"))
            {
                FireTrigger(TRIGGER_RYU_JUMP_KICK);
            }
        }

        // Update direction to face the other player
        if (!_isStuck)
        {
            UpdateDirection();
        }

        // Handle enabling of collision boxes
        foreach (Transform group in transform.Find("collision_boxes"))
        {
            foreach (Transform frame in group)
            {
                bool isActive = (frame == _currentBoxesFrame);
                frame.gameObject.SetActive(isActive);
            }
        }

        // Keep the player in the camera
        BoxCollider2D camera_box = _currentBoxesFrame.Find("camera").GetComponent<BoxCollider2D>();
        if (camera_box.bounds.min.x < CurrentGameCamera.Left)
        {
            float new_x = CurrentGameCamera.Left + camera_box.bounds.extents.x;
            transform.position = new Vector3(new_x, transform.position.y, transform.position.z);
            _knockback_speed = 0;
        }
        else if (camera_box.bounds.max.x > CurrentGameCamera.Right)
        {
            float new_x = CurrentGameCamera.Right - camera_box.bounds.extents.x;
            transform.position = new Vector3(new_x, transform.position.y, transform.position.z);
            _knockback_speed = 0;
        }

        // Push off other player
        if (PushBox.IsIntersecting(OtherPlayer.PushBox))
        {
            PlayerController left, right;
            if (PushBox.bounds.center.x < OtherPlayer.PushBox.bounds.center.x)
            {
                left = this;
                right = OtherPlayer;
            }
            else
            {
                left = OtherPlayer;
                right = this;
            }

            float overlap_amount = left.PushBox.bounds.max.x - right.PushBox.bounds.min.x;

            left.transform.Translate(-overlap_amount / 2, 0, 0);
            right.transform.Translate(overlap_amount / 2, 0, 0);
        }

        // Proximity guard / Hit Detection
        if (OtherPlayer.Hurtboxes != null)
        {
            // Hit detection
            Vector3 hitPos;
            if (!_currentHitboxHit && HitBoxes != null && HitBoxes.IsIntersecting(OtherPlayer.Hurtboxes, out hitPos))
            {
                hitPos.z -= 0.1f;

                Hitbox hitbox = HitBoxes[0].GetComponent<Hitbox>();
                AttackData.HitLevel level = hitbox.AttackData.Level;

                bool blocked = OtherPlayer.IsBlocking;
                if (blocked)
                {
                    if (level == AttackData.HitLevel.MID && OtherPlayer.HoldingDown)
                    {
                        blocked = false;
                    }
                    else if (level == AttackData.HitLevel.LOW && !OtherPlayer.HoldingDown)
                    {
                        blocked = false;
                    }
                }

                if (blocked)
                {
                    OtherPlayer.Block(hitbox);
                    Instantiate(BlockSpark, hitPos, Quaternion.identity);
                    KnockbackSpeed(hitbox.AttackData.PushbackBlock);
                }
                else
                {
                    OtherPlayer.TakeDamage(hitbox);
                    if ((hitbox.AttackData.KnocksDownGround && OtherPlayer.IsGrounded) 
                        || (hitbox.AttackData.KnocksDownAir && OtherPlayer.IsAirborne))
                    {
                        OtherPlayer.KnockDown(hitbox);
                    }
                    else
                    {
                        OtherPlayer.Hit(hitbox);
                    }
                    Instantiate(HitSpark, hitPos, Quaternion.identity);
                    KnockbackSpeed(hitbox.AttackData.PushbackHit);
                }

                _currentHitboxHit = true;
            }

            // Proximity guard detection
            //if (!contact && ProxBoxes != null && ProxBoxes.IsIntersecting(OtherPlayer.Hurtboxes, out hitPos))
            //{
            //    if (OtherPlayer.IsBlocking)
            //    {
            //        OtherPlayer.Block(1, 0);
            //    }
            //}
        }

        // Hitstun handling
        if (InHitStun)
        {
            if (--_hitstun_frames == 0)
            {
                SetState(State.STATE_IDLE);
            }
            else
            {
                SetState(State.STATE_HIT);
            }
        }

        // Blockstun handling
        else if (InBlockStun)
        {
            if (--_blockstun_frames == 0)
            {
                SetState(State.STATE_IDLE);
            }
            else
            {
                SetState(State.STATE_BLOCK);
            }
        }

        // Airborne handling
        if (IsAirborne)
        {
            _velY -= JumpGravity;
            transform.Translate(_jumpVelX, _velY, 0);

            // Check if player touched the ground
            if (_velY <= 0 && transform.position.y <= _initialY)
            {
                transform.position = new Vector3(transform.position.x, _initialY, transform.position.z);
                SetState(State.STATE_IDLE);
                SetAirborne(false);
                UpdateDirection();
            }
        }

        // Air knockdown handling
        else if (InAirKnockdown)
        {
            transform.Translate(0, _velY, 0);
            _velY -= _gravity;

            // Check if player hit the ground
            if (_velY <= 0 && transform.position.y <= _initialY)
            {
                transform.position = new Vector3(transform.position.x, _initialY, transform.position.z);
                SetState(State.STATE_KNOCKDOWN_GROUND);
                FireTrigger(TRIGGER_RYU_KNOCKDOWN_GROUND);
            }
        }

        // Ground knockdown handling
        else if (_inGroundKnockdown)
        {
            if (--_frames_on_ground == 0)
            {
                SetState(State.STATE_KNOCKDOWN_GETUP);
                FireTrigger(TRIGGER_RYU_KNOCKDOWN_GETUP);
            }
        }

        // Knockback
        if (_knockback_speed != 0)
        {
            transform.Translate(_knockback_speed, 0, 0);

            // Only decelerate if the player isn't being knocked through the air
            if (_state != State.STATE_KNOCKDOWN_AIR)
            {
                float prev_speed = _knockback_speed;
                _knockback_speed -= KNOCKBACK_DECELERATION * Mathf.Sign(_knockback_speed);
                if (Mathf.Sign(_knockback_speed) != Mathf.Sign(prev_speed))
                {
                    _knockback_speed = 0;
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        // Apply damage
        HP -= damage;
    }

    public void TakeDamage(Hitbox hitbox)
    {
        TakeDamage(hitbox.AttackData.Damage);
    }

    public void Hit(int num_frames, float relative_knockback = 0)
    {
        // Flip out in air if player is airborne
        if (IsAirborne)
        {
            _velY = Math.Abs(_velY);
            _jumpVelX = JumpSpeedHorizontal * (_isFacingRight ? -1 : 1);
            SetState(State.STATE_FLIP_OUT);
        }
        // Apply hit as normal if player is grounded
        else
        {
            KnockbackSpeed(relative_knockback);
            _hitstun_frames = num_frames;
            FireTrigger(TRIGGER_RYU_HIT);
        }
    }

    public void Hit(Hitbox hitbox)
    {
        Hit(hitbox.AttackData.Hitstun, hitbox.AttackData.KnockbackHit);
    }

    public void KnockDown(float velY, float gravity, float relative_knockback, int frames_on_ground)
    {
        _frames_on_ground = frames_on_ground;
        KnockbackSpeed(relative_knockback);
        _velY = velY;
        _gravity = gravity;
        FireTrigger(TRIGGER_RYU_KNOCKDOWN_AIR);

        SetState(State.STATE_KNOCKDOWN_AIR);
        SetAirborne(false);
        _hitstun_frames = 0;
        _blockstun_frames = 0;
    }

    public void KnockDown(Hitbox hitbox)
    {
        KnockDown(hitbox.AttackData.KnockdownLaunch, hitbox.AttackData.KnockdownGravity, 
            hitbox.AttackData.KnockbackHit, hitbox.AttackData.KnockdownGroundFrames);
    }

    public void Block(int num_frames, float relative_knockback = 0)
    {
        _blockstun_frames = num_frames;
        KnockbackSpeed(relative_knockback);
        FireTrigger(TRIGGER_RYU_BLOCK);
    }

    public void Block(Hitbox hitbox)
    {
        Block(hitbox.AttackData.Blockstun, hitbox.AttackData.KnockbackBlock);
    }

    public void KnockbackSpeed(float relative_knockback)
    {
        _knockback_speed = relative_knockback * (_onLeftSide ? -1 : 1);
    }

    public void JumpNeutral()
    {
        _velY = JumpPower;
        SetAirborne(true);
        _jumpVelX = 0;
    }

    public void JumpBack()
    {
        _velY = JumpPower;
        SetAirborne(true);
        _jumpVelX = JumpSpeedHorizontal * (_isFacingRight ? -1 : 1);
    }

    public void JumpForward()
    {
        _velY = JumpPower;
        SetAirborne(true);
        _jumpVelX = JumpSpeedHorizontal * (_isFacingRight ? 1 : -1);
    }

    private void UpdateSetCollisionBoxesFrame()
    {
        if (CurrentBoxesFrame != _nextBoxesFrame)
        {
            _currentBoxesFrame = _nextBoxesFrame;
            _currentHitboxHit = false;
        }
    }

    private void SetCollisionBoxesFrame(string name)
    {
        string anim_name = name.Substring(0, name.Length - 3);

        Transform collision_boxes = transform.Find("collision_boxes");
        Transform collisionBoxGroup = collision_boxes.Find(anim_name);

        foreach (Transform frame in collisionBoxGroup)
        {
            if (frame.name == "all" || frame.name == name)
            {
                _nextBoxesFrame = frame;
                break;
            }
        }
    }

    private void SetState(State state)
    {
        _animator.SetInteger("state", (int)state);
        _state = state;
    }

    private void SetJumpDecision(State state)
    {
        _animator.SetInteger("jump_decision", (int)state);
        _jumpDecision = state;
    }

    private void FireTrigger(string trigger)
    {
        _animator.SetTrigger(trigger);
    }

    private void ResetTrigger(string trigger)
    {
        _animator.ResetTrigger(trigger);
    }

    private void SetForward(bool isForward)
    {
        _animator.SetBool("moving_forward", isForward);
        _movingForward = isForward;
    }

    private void SetCrouching(bool isCrouching)
    {
        _animator.SetBool("is_crouching", IsCrouching);
    }

    private void SetAirborne(bool isAirborne)
    {
        _animator.SetBool("grounded", !isAirborne);
        _isAirborne = isAirborne;
    }

    private void UpdateDirection()
    {
        // Update the player's direction
        if (OtherPlayer.transform.position.x > transform.position.x)
        {
            _direction = Direction.RIGHT;
        }
        else
        {
            _direction = Direction.LEFT;
        }

        // Update the direction (x-scale of the transform)
        float scaleX = transform.localScale.x;
        if ((_isFacingRight && scaleX < 0) || (!_isFacingRight && scaleX > 0))
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    // InputManager helpers
    private float GetAxis(string axis)
    {
        return Input.GetAxisRaw(Prefix + axis);
    }

    private bool GetButton(string button)
    {
        return Input.GetButton(Prefix + button);
    }

    private bool GetButtonDown(string button)
    {
        return Input.GetButtonDown(Prefix + button);
    }
}
