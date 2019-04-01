using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
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
    public Vector3 _spawnPos;

    public Transform FireballSpawnPos;
    public Projectile FireballPrefab;

    [HideInInspector]
    public bool HoldingOpponent = false;
    public Transform HoldPoint;
    public Transform TechReleasePosition;

    public Vector2 AnimatedDeltaPos;
    private Vector2 _animatedDeltaPosPrevious;

    [HideInInspector]
    public PlayerController OtherPlayer;

    public GameObject HitSpark;
    public GameObject BlockSpark;

    public AttackData RoundEndDefault;
    public AttackData ThrowAttackData;

    [HideInInspector]
    public float HitShakeScaleX;
    //private float _hitShakeDistance = 0.1f;
    
    public Globals.Player PlayerNum = Globals.Player.P1;

    private int ControllerNum
    {
        get
        {
            return PlayerNum == Globals.Player.P1 ? _globals.Player1Controller : _globals.Player2Controller;
        }
    }

    public Dictionary<string, string> ActionToAxis = new Dictionary<string, string>
    {
        { "Horizontal", "Horizontal" },
        { "Vertical", "Vertical" },
        { "Throw", "A" },
        { "Special", "B" },
        { "Attack", "X" },
        { "Kick", "Y" },
        { "Select", "Select" },
        { "Start", "Start" },
    };

    // A map of actions to the number of frames to repeat the action
    private Dictionary<string, int> _actionBuffers = new Dictionary<string, int>
    {
        { "Horizontal", 0 },
        { "Vertical", 0 },
        { "Throw", 0 },
        { "Special", 0 },
        { "Attack", 0 },
        { "Kick", 0 },
        { "Select", 0 },
        { "Start", 0 },
    };

    private static string[] actionsToRecord =
    {
        "Horizontal", "Vertical", "Throw", "Special", "Attack", "Kick"
    };

    // Specifies one frame of a recording of actions
    private class ActionRecordingFrame
    {
        private Dictionary<string, float> _axes;
        private Dictionary<string, bool> _button;
        private Dictionary<string, bool> _buttonDown;
        private Dictionary<string, bool> _buttonDownWithBuffer;
        private Direction _direction;

        public float GetAxis(Direction direction, string action)
        {
            float factor = (action == "Horizontal" && direction != _direction) ? -1 : 1;
            return _axes[action] * factor;
        }
        public bool GetButton(string action) { return _button[action]; }
        public bool GetButtonDown(string action) { return _buttonDown[action]; }
        public bool GetButtonDownWithBuffer(string action) { return _buttonDownWithBuffer[action]; }

        public ActionRecordingFrame(PlayerController player)
        {
            _axes = new Dictionary<string, float>();
            _button = new Dictionary<string, bool>();
            _buttonDown = new Dictionary<string, bool>();
            _buttonDownWithBuffer = new Dictionary<string, bool>();
            
            _direction = player._direction;
            
            foreach (string action in actionsToRecord)
            {
                _axes[action] = player.GetAxis(action);
                _button[action] = player.GetButton(action);
                _buttonDown[action] = player.GetButtonDown(action);
                _buttonDownWithBuffer[action] = player.GetButtonDownWithBuffer(action);
            }
        }
    }

    // An array of button inputs per frame. Record inputs to this and replay later
    private List<ActionRecordingFrame> _actionRecording = new List<ActionRecordingFrame>();
    private int _recordingFrames = 0;
    private bool isRecordingActions = false;
    private int _actionPlaybackFrame = -1;

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
        STATE_HELD = 14,

        STATE_JUMP_NODECISION = 500,
    }
    private State _state = State.STATE_IDLE;
    private enum Direction { RIGHT, LEFT }

    // Generic
    public const string TRIGGER_PLAYER_HIT = "play_hit";
    public const string TRIGGER_PLAYER_BLOCK = "play_block";
    public const string TRIGGER_PLAYER_CROUCH = "play_crouch";
    public const string TRIGGER_PLAYER_THROW_INITIAL = "play_throw_initial";
    public const string TRIGGER_PLAYER_HELD = "play_held";
    public const string TRIGGER_PLAYER_THROW_TECH = "play_throw_tech";
    public const string TRIGGER_PLAYER_THROWN = "play_thrown";
    public const string TRIGGER_PLAYER_KNOCKDOWN_AIR = "play_knockdown_air";
    public const string TRIGGER_PLAYER_KNOCKDOWN_GROUND = "play_knockdown_ground";
    public const string TRIGGER_PLAYER_KNOCKDOWN_GETUP = "play_knockdown_getup";
    public const string TRIGGER_PLAYER_VICTORY= "play_victory";
    public const string TRIGGER_PLAYER_TIMEOUT = "play_timeout";
    public const string TRIGGER_PLAYER_STOP_CELEBRATION = "stop_celebration";

    private CharacterSpecificController _characterSpecificController;

    // Ryu-specific
    //private const string TRIGGER_RYU_ATTACK = "play_attack";
    //private const string TRIGGER_RYU_KICK = "play_kick";
    //private const string TRIGGER_RYU_SWEEP = "play_sweep";
    //private const string TRIGGER_RYU_CROUCH_PUNCH = "play_crouch_punch";
    //private const string TRIGGER_RYU_OVERHEAD = "play_overhead";
    //private const string TRIGGER_RYU_SHORYU = "play_shoryu";
    //private const string TRIGGER_RYU_THROW_FIREBALL = "play_throw_fireball";
    //private const string TRIGGER_RYU_JUMP_KICK = "play_jump_kick";

    //private static string[] StuckAnims =
    //{
    //    "ryu_attack", "ryu_kick", "ryu_sweep", "ryu_overhead", "ryu_shoryu",
    //    "ryu_throw_fireball", "ryu_throw_initial", "ryu_throw_whiff",
    //    "ryu_throw_success", "ryu_throw_back_success", "ryu_thrown", "ryu_throw_tech",
    //    "ryu_crouch_punch", "ryu_knockdown_air", "ryu_knockdown_ground",
    //    "ryu_knockdown_getup", "ryu_victory"
    //};

    private static State[] StuckStates =
    {
        State.STATE_HIT, State.STATE_BLOCK, State.STATE_KNOCKDOWN_AIR,
        State.STATE_KNOCKDOWN_GROUND, State.STATE_KNOCKDOWN_GETUP,
        State.STATE_PRE_JUMP, State.STATE_NEUTRAL_JUMP, State.STATE_BACK_JUMP,
        State.STATE_FORWARD_JUMP, State.STATE_FLIP_OUT,
    };

    //private static string[] AirStuckAnims =
    //{
    //    "ryu_jump_kick", "ryu_flip_out"
    //};

    private static State[] AirStuckStates =
    {
        State.STATE_FLIP_OUT
    };

    //private static string[] MovingAnims =
    //{
    //    "ryu_shoryu"
    //};

    //private static string[] IgnoreGravityAirborneAnims =
    //{
    //    "ryu_shoryu"
    //};

    private bool _victoryFired = false;
    private bool _timeoutFired = false;

    private Transform _currentBoxesFrame;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;

    private Direction _direction = Direction.RIGHT;
    private bool _movingForward = true;

    private bool _isCrouching = false;

    private bool _isAirborne = false;
    private State _jumpDecision = State.STATE_NEUTRAL_JUMP;

    [HideInInspector]
    public bool InSpecialCancelWindow = false;
    
    public bool InThrowTechWindow = false;
    private bool _throwTechSuccess = false;
    public bool Holding = false;
    private bool _throw_success = false;
    
    private const float KNOCKBACK_DECELERATION = 0.05f;
    private float _hitstun_frames = 0;
    private float _blockstun_frames = 0;
    private float _freeze_time_frames = 0;
    private float _frames_on_ground = 0;
    private float _knockback_speed = 0;
    private bool _isDead = false;

    private bool _currentHitboxHit = false;
    private bool _currentAnimHit = false;
    private Transform _nextBoxesFrame = null;

    public GameController GameController;

    private const string GLOBALS_NAME = "Globals";
    private static Globals _globals;

    // Properties
    public int HP
    {
        get { return _hp; }
        set { _hp = value; }
    }

    public float HPPercentage
    {
        get { return (float)_hp / MaxHP; }
    }

    public Transform CurrentBoxesFrame
    {
        get { return _currentBoxesFrame; }
    }

    public BoxCollider2D[] HitBoxes
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return new BoxCollider2D[0];

            Transform boxGroup = CurrentBoxesFrame.Find("hit");
            if (boxGroup == null)
                return new BoxCollider2D[0];
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D[] Hurtboxes
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return new BoxCollider2D[0];

            Transform boxGroup = CurrentBoxesFrame.Find("hurt");
            if (boxGroup == null)
                return new BoxCollider2D[0];
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D[] ThrowBoxes
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return new BoxCollider2D[0];

            Transform boxGroup = CurrentBoxesFrame.Find("throw");
            if (boxGroup == null)
                return new BoxCollider2D[0];
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D[] ProxBoxes
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return new BoxCollider2D[0];

            Transform boxGroup = CurrentBoxesFrame.Find("prox");
            if (boxGroup == null)
                return new BoxCollider2D[0];
            return boxGroup.GetComponents<BoxCollider2D>();
        }
    }

    public BoxCollider2D PushBox
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return null;

            Transform pbTrans = CurrentBoxesFrame.Find("push");
            if (pbTrans == null)
                return null;
            return pbTrans.GetComponent<BoxCollider2D>();
        }
    }

    public BoxCollider2D CameraBox
    {
        get
        {
            if (CurrentBoxesFrame == null)
                return null;

            Transform cbTrans = CurrentBoxesFrame.Find("camera");
            if (cbTrans == null)
                return null;
            return cbTrans.GetComponent<BoxCollider2D>();
        }
    }

    private bool _isP1
    {
        get { return PlayerNum == Globals.Player.P1; }
    }

    public string Prefix
    {
        get { return _isP1 ? "P1_" : "P2_"; }
    }

    public bool IsFacingRight
    {
        get { return _direction == Direction.RIGHT; }
    }

    private bool _onLeftSide
    {
        get { return transform.position.x < OtherPlayer.transform.position.x; }
    }

    public bool IsAboveOpponent
    {
        get { return transform.position.y > OtherPlayer.transform.position.y; }
    }

    public float VelY
    {
        get { return _velY; }
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

    public bool InGroundKnockdown
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

    private bool _isThrowable
    {
        get
        {
            if (IsAirborne || InHitStun || InBlockStun || Data.FramesLeft <= 0)
                return false;

            return true;
        }
    }

    public bool IsStuck
    {
        get
        {
            // Is it not in gameplay state?
            if (Data.CurrentState != GameplayData.State.GAMEPLAY)
                return true;

            // Is the player attacking?
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in _characterSpecificController.StuckAnims)
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
    
    public bool IsAirStuck
    {
        get
        {
            // Is the player attacking?
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in _characterSpecificController.AirStuckAnims)
                if (stateInfo.IsName(anim_name))
                    return true;

            // Is the player in a stuck state?
            foreach (State state in AirStuckStates)
                if (_state == state)
                    return true;

            return false;
        }
    }

    private bool _isInMovingAnimation
    {
        get
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in _characterSpecificController.MovingAnims)
                if (stateInfo.IsName(anim_name))
                    return true;

            return false;
        }
    }

    private bool _isInIgnoreGravityAnimation
    {
        get
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            foreach (string anim_name in _characterSpecificController.IgnoreGravityAirborneAnims)
                if (stateInfo.IsName(anim_name))
                    return true;

            return false;
        }
    }

    public bool IsDead
    {
        get { return _isDead; }
    }

    // Holding Directions
    public bool HoldingBack
    {
        get
        {
            float horizontal = GetAxis("Horizontal");
            return (IsFacingRight && horizontal < 0)
                || (!IsFacingRight && horizontal > 0);
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
        get { return HoldingBack && (InBlockStun || !IsStuck); }
    }

    public bool IsCrouching
    {
        get { return _isCrouching; }
    }

    public bool InWalkAnim
    {
        get
        {
            return _animator.GetCurrentAnimatorStateInfo(0).IsName("walk_forward")
                || _animator.GetCurrentAnimatorStateInfo(0).IsName("walk_backward");
        }
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
        _globals = Resources.Load<Globals>(GLOBALS_NAME);

        _animator = this.GetComponent<Animator>();
        _spriteRenderer = this.GetComponent<SpriteRenderer>();

        _initialY = transform.position.y;
        _hp = MaxHP;

        _spawnPos = transform.position;

        _animatedDeltaPosPrevious = Vector2.zero;
        _characterSpecificController = GetComponent<CharacterSpecificController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update recovery color
        if (_globals.ShowRecoveryColor && IsStuck && Data.CurrentState == GameplayData.State.GAMEPLAY)
        {
            _spriteRenderer.color = Color.blue;
        }
        else
        {
            _spriteRenderer.color = Color.white;
        }

        // Record / Replay actions
        if (GetButtonDown("Start"))
        {
            if (!isRecordingActions)
            {
                StartActionRecording();
            }
            else if (isRecordingActions)
            {
                StopActionRecording();
            }
        }
        else if (GetButtonDown("Select"))
        {
            if (_recordingFrames > 0 && !isRecordingActions)
            {
                StartActionPlayback();
            }
        }

        if (isRecordingActions)
        {
            RecordActions();
        }

        // Update buffered inputs
        UpdateBufferedInputs();

        // Update animator flags
        _animator.SetBool("in_special_cancel_window", InSpecialCancelWindow);
        _animator.SetBool("throw_success", _throw_success);
        _animator.SetBool("contact_made", _currentHitboxHit);

        // Set the animation speed
        float playerTimeScale = _freeze_time_frames-- <= 0 ? _globals.TimeScale : 0;
        _animator.speed = playerTimeScale;

        // Play victory animation
        if (Data.CurrentState == GameplayData.State.CELEBRATION && !IsDead)
        {
            if (!_victoryFired && _globals.PlayerWinning == PlayerNum)
            {
                FireTrigger(TRIGGER_PLAYER_VICTORY);
                _victoryFired = true;
            }

            // Play timeout defeat/draw animation
            if (!_timeoutFired && _globals.PlayerWinning != PlayerNum)
            {
                FireTrigger(TRIGGER_PLAYER_TIMEOUT);
                _timeoutFired = true;
            }
        }

        // Go to idle on not gameplay
        if (!IsDead && Data.CurrentState != GameplayData.State.GAMEPLAY && (IsCrouching || InWalkAnim))
        {
            SetState(State.STATE_IDLE);
            SetCrouching(_isCrouching);
        }

        // Throw teching
        if (OtherPlayer.InThrowTechWindow)
        {
            if (GetButtonDownWithBuffer("Throw"))
                _throwTechSuccess = true;
        }
        else
        {
            _throwTechSuccess = false;
        }

        // Being thrown
        if (OtherPlayer.Holding)
        {
            transform.position = OtherPlayer.HoldPoint.position;

            if (_throwTechSuccess)
            {
                BothTech();
            }
        }

        // Jumping
        if (!IsStuck && IsGrounded)
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
            if (GetAxis("Horizontal") * (IsFacingRight ? 1 : -1) < 0)
            {
                SetJumpDecision(State.STATE_BACK_JUMP);
            }
            else if (GetAxis("Horizontal") * (IsFacingRight ? 1 : -1) > 0)
            {
                SetJumpDecision(State.STATE_FORWARD_JUMP);
            }
        }

        // Crouching
        if (!IsStuck)
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
        if (!IsStuck && !_isCrouching)
        {
            if (GetAxis("Horizontal") > 0)
            {
                SetState(State.STATE_WALK);

                SetForward(IsFacingRight);
                float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

                transform.Translate(speed * playerTimeScale * Utils.TimeCorrection, 0, 0);
            }
            else if (GetAxis("Horizontal") < 0)
            {
                SetState(State.STATE_WALK);

                SetForward(!IsFacingRight);
                float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

                transform.Translate(-speed * playerTimeScale * Utils.TimeCorrection, 0, 0);
            }
            else
            {
                SetState(State.STATE_IDLE);
            }
        }

        _characterSpecificController.HandleAttacking(playerTimeScale);

        // Update direction to face the other player
        if (!IsStuck 
            || (Data.CurrentState >= GameplayData.State.ANNOUNCE_ROUND_1 && Data.CurrentState <= GameplayData.State.ANNOUNCE_ROUND_FINAL))
        {
            UpdateDirection();
        }

        // Keep the player in the camera
        if (CameraBox != null)
        {
            if (CameraBox.bounds.min.x < GameController.Camera.Left)
            {
                float new_x = GameController.Camera.Left + CameraBox.bounds.extents.x;
                transform.position = new Vector3(new_x, transform.position.y, transform.position.z);
                _knockback_speed = 0;
            }
            else if (CameraBox.bounds.max.x > GameController.Camera.Right)
            {
                float new_x = GameController.Camera.Right - CameraBox.bounds.extents.x;
                transform.position = new Vector3(new_x, transform.position.y, transform.position.z);
                _knockback_speed = 0;
            }
        }

        // Push off other player
        if (PushBox != null && OtherPlayer.PushBox != null && PushBox.IsIntersecting(OtherPlayer.PushBox))
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

        // Hitstun handling
        if (InHitStun)
        {
            _hitstun_frames -= 1f * playerTimeScale;
            if (_hitstun_frames <= 0)
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
            _blockstun_frames -= 1f * playerTimeScale;
            if (_blockstun_frames <= 0)
            {
                SetState(State.STATE_IDLE);
            }
            else
            {
                SetState(State.STATE_BLOCK);
            }
        }

        // Airborne handling
        if (IsAirborne && !_isInIgnoreGravityAnimation)
        {
            if (_freeze_time_frames <= 0)
            {
                _velY -= JumpGravity * playerTimeScale;
                transform.Translate(_jumpVelX * playerTimeScale, _velY * playerTimeScale, 0);

                // Check if player touched the ground
                if (_velY <= 0 && transform.position.y <= _initialY)
                {
                    transform.position = new Vector3(transform.position.x, _initialY, transform.position.z);
                    SetState(State.STATE_IDLE);
                    SetAirborne(0);
                    UpdateDirection();
                }
            }
        }

        // Air knockdown handling
        else if (InAirKnockdown)
        {
            if (_freeze_time_frames <= 0)
            {
                _velY -= _gravity * playerTimeScale;
                transform.Translate(0, _velY * playerTimeScale, 0);

                // Check if player hit the ground
                if (_velY <= 0 && transform.position.y <= _initialY)
                {
                    transform.position = new Vector3(transform.position.x, _initialY, transform.position.z);
                    SetState(State.STATE_KNOCKDOWN_GROUND);
                    FireTrigger(TRIGGER_PLAYER_KNOCKDOWN_GROUND);
                }
            }
        }

        // Ground knockdown handling
        else if (InGroundKnockdown)
        {
            if (_freeze_time_frames <= 0)
            {
                _frames_on_ground -= 1f * playerTimeScale;
                if (_frames_on_ground <= 0 && !IsDead)
                {
                    SetState(State.STATE_KNOCKDOWN_GETUP);
                    FireTrigger(TRIGGER_PLAYER_KNOCKDOWN_GETUP);
                }
            }
        }

        // Knockback
        if (_knockback_speed != 0)
        {
            transform.Translate(_knockback_speed * playerTimeScale, 0, 0);

            // Only decelerate if the player isn't being knocked through the air
            if (_state != State.STATE_KNOCKDOWN_AIR)
            {
                float prev_speed = _knockback_speed;
                _knockback_speed -= KNOCKBACK_DECELERATION * Mathf.Sign(_knockback_speed) * playerTimeScale;
                if (Mathf.Sign(_knockback_speed) != Mathf.Sign(prev_speed))
                {
                    _knockback_speed = 0;
                }
            }
        }
    }

    public void LateUpdate()
    {
        // Set the collision boxes frame
        UpdateSetCollisionBoxesFrame();

        // Handle enabling of collision boxes
        foreach (Transform group in transform.Find("collision_boxes"))
        {
            foreach (Transform frame in group)
            {
                bool isActive = (frame == CurrentBoxesFrame);
                frame.gameObject.SetActive(isActive);
            }
        }

        // Handle animations which move the player
        if (_isInMovingAnimation || _globals.DebugPreviewAnimationMovements)
        {
            Vector2 deltaDeltaPos = AnimatedDeltaPos - _animatedDeltaPosPrevious;
            
            // Flip direciton if player is on P2 side
            if (_direction == Direction.LEFT)
                deltaDeltaPos = Vector2.Scale(deltaDeltaPos, new Vector2(-1, 1));

            transform.Translate(deltaDeltaPos.x, deltaDeltaPos.y, 0);
            _animatedDeltaPosPrevious = AnimatedDeltaPos;
        }
        else
        {
            _animatedDeltaPosPrevious = Vector2.zero;
        }

        // Proximity guard / Hit Detection
        Vector3 hitPos;
        if (HitBoxes.Length > 0 && HitBoxes.IsIntersecting(OtherPlayer.Hurtboxes, out hitPos) && !_currentHitboxHit)
        {
            Hitbox hitbox = HitBoxes[0].GetComponent<Hitbox>();

            if ((!_currentAnimHit || !hitbox.AnimationHitOnce) && Data.FramesLeft > 0)
            {
                OtherPlayer.HandleContact(hitbox.AttackData, hitPos);
            }
            _currentHitboxHit = true;
            _currentAnimHit = true;
        }

        // Throw detection
        if (ThrowBoxes.Length > 0 && ThrowBoxes.IsIntersecting(OtherPlayer.Hurtboxes, out hitPos))
        {
            bool opponent_held = OtherPlayer._isThrowable;
            if (opponent_held)
            {
                _throw_success = true;
                Holding = true;
                OtherPlayer.FireTrigger(TRIGGER_PLAYER_HELD);
            }
        }

        // Advance in recording
        if (_actionPlaybackFrame > -1)
        {
            _actionPlaybackFrame++;
            if (_actionPlaybackFrame >= _recordingFrames)
            {
                _actionPlaybackFrame = -1;
            }
        }
    }

    public void HandleContact(AttackData attackData, Vector3 hitPos)
    {
        hitPos = new Vector3(hitPos.x, hitPos.y, -2);

        // Proximity guard / Hit Detection
        if (Hurtboxes.Length > 0)
        {                
            AttackData.HitLevel level = attackData.Level;

            // Is player blocking?
            bool blocked = IsBlocking;

            // Check if player blocked incorrectly on mid or low
            if (blocked)
            {
                if (level == AttackData.HitLevel.MID && HoldingDown)
                {
                    blocked = false;
                }
                else if (level == AttackData.HitLevel.LOW && !HoldingDown)
                {
                    blocked = false;
                }
            }

            // Blocking
            if (blocked)
            {
                // Put other player in grounded blockstun
                Block(attackData);
                // Create block spark
                Instantiate(BlockSpark, hitPos, Quaternion.identity);
                // Apply pushback to other player
                OtherPlayer.KnockbackSpeed(attackData.PushbackBlock);
            }
            // Hitting
            else
            {
                // Take damage
                TakeDamage(attackData);

                // Kill player
                if (HP <= 0)
                {
                    // If attack wont knock down, use `RoundEndDefault` to kill player
                    AttackData roundEndAttackData;
                    if (!(attackData.KnocksDownGround && IsGrounded)
                        && !(attackData.KnocksDownAir && IsAirborne))
                    {
                        roundEndAttackData = RoundEndDefault;
                    }
                    else
                    {
                        roundEndAttackData = attackData;
                    }
                    Kill(roundEndAttackData);
                    GameController.FireRoundEnd();
                }
                // Knock other player down
                else if ((attackData.KnocksDownGround && IsGrounded)
                    || (attackData.KnocksDownAir && IsAirborne))
                {
                    KnockDown(attackData);
                }
                // Put other player in grounded hitstun
                else
                {
                    Hit(attackData);
                }

                // Create hit spark
                Instantiate(HitSpark, hitPos, Quaternion.identity);
                // Apply pushback to other player
                OtherPlayer.KnockbackSpeed(attackData.PushbackHit);
            }

            if (attackData.FreezeDefender)
            {
                HitFreeze(attackData.FreezeTimeFrames);
            }
            if (attackData.FreezeAttacker)
            {
                OtherPlayer.HitFreeze(attackData.FreezeTimeFrames);
            }
            GameController.CameraShake(attackData.FreezeTimeFrames);
        }
    }

    public void HandleThrow(AttackData attackData)
    {
        KnockDown(attackData);
        TakeDamage(attackData);

        // Freeze time
        if (attackData.FreezeDefender)
        {
            HitFreeze(attackData.FreezeTimeFrames);
        }
        if (attackData.FreezeAttacker)
        {
            OtherPlayer.HitFreeze(attackData.FreezeTimeFrames);
        }
        GameController.CameraShake(attackData.FreezeTimeFrames);

        // Kill player
        if (HP <= 0)
        {
            Kill(attackData);
            GameController.FireRoundEnd();
        }

        // Push Back Opponent
        OtherPlayer.KnockbackSpeed(attackData.PushbackHit);
    }

    public void TakeDamage(int damage)
    {
        // Apply damage, but only if there is time left on the clock
        if (Data.FramesLeft > 0 && Data.CurrentState == GameplayData.State.GAMEPLAY)
        {
            HP -= damage;
        }
    }

    public void TakeDamage(AttackData attackData)
    {
        TakeDamage(attackData.Damage);
    }

    public void Hit(int num_frames, float relative_knockback = 0)
    {
        // Flip out in air if player is airborne
        if (IsAirborne)
        {
            _velY = Math.Abs(_velY);
            _jumpVelX = JumpSpeedHorizontal * (IsFacingRight ? -1 : 1);
            SetState(State.STATE_FLIP_OUT);
        }
        // Apply hit as normal if player is grounded
        else
        {
            KnockbackSpeed(relative_knockback);
            _hitstun_frames = num_frames;
            FireTrigger(TRIGGER_PLAYER_HIT);
        }

        GameController.PlayHitSFX();
    }

    public void Hit(AttackData attackData)
    {
        Hit(attackData.Hitstun, attackData.KnockbackHit);
    }

    public void KnockDown(float velY, float gravity, float relative_knockback, int frames_on_ground)
    {
        _frames_on_ground = frames_on_ground;
        KnockbackSpeed(relative_knockback);
        _velY = velY;
        _gravity = gravity;
        FireTrigger(TRIGGER_PLAYER_KNOCKDOWN_AIR);

        SetState(State.STATE_KNOCKDOWN_AIR);
        SetAirborne(0);
        _hitstun_frames = 0;
        _blockstun_frames = 0;

        GameController.PlayHitHardSFX();
    }

    public void KnockDown(AttackData attackData)
    {
        KnockDown(attackData.KnockdownLaunch, attackData.KnockdownGravity,
            attackData.KnockbackHit, attackData.KnockdownGroundFrames);
    }

    public void Kill(float velY, float gravity, float relative_knockback)
    {
        KnockDown(velY, gravity, relative_knockback, -1);
        _isDead = true;
    }

    public void Kill(AttackData attackData)
    {
        Kill(attackData.KnockdownLaunch, attackData.KnockdownGravity, attackData.KnockbackHit);
    }

    public void Block(int num_frames, float relative_knockback = 0)
    {
        _blockstun_frames = num_frames;
        KnockbackSpeed(relative_knockback);
        FireTrigger(TRIGGER_PLAYER_BLOCK);

        GameController.PlayBlockSFX();
    }

    public void Block(AttackData attackData)
    {
        Block(attackData.Blockstun, attackData.KnockbackBlock);
    }

    public void KnockbackSpeed(float relative_knockback, bool use_direction_facing = false)
    {
        if (use_direction_facing)
            _knockback_speed = relative_knockback * (_direction == Direction.RIGHT ? -1 : 1);
        else
            _knockback_speed = relative_knockback * (_onLeftSide ? -1 : 1);
    }

    public bool WillKill(int damage)
    {
        return Data.CurrentState == GameplayData.State.GAMEPLAY && damage > HP;
    }

    public bool WillKill(Hitbox hitbox)
    {
        return WillKill(hitbox.AttackData.Damage);
    }

    public void BothTech()
    {
        ThrowTech();
        OtherPlayer.ThrowTech();
    }

    public void ThrowTech()
    {
        Holding = false;
        _throw_success = false;
        FireTrigger(TRIGGER_PLAYER_THROW_TECH);

        transform.position = new Vector3(OtherPlayer.TechReleasePosition.position.x, 0, transform.position.z);
        KnockbackSpeed(ThrowAttackData.PushbackBlock);

        Instantiate(BlockSpark, TechReleasePosition.position, Quaternion.identity);
    }

    public void HitFreeze(int frames)
    {
        _freeze_time_frames = frames;
    }

    public void JumpNeutral()
    {
        _velY = JumpPower;
        SetAirborne(1);
        _jumpVelX = 0;
    }

    public void JumpBack()
    {
        _velY = JumpPower;
        SetAirborne(1);
        _jumpVelX = JumpSpeedHorizontal * (IsFacingRight ? -1 : 1);
    }

    public void JumpForward()
    {
        _velY = JumpPower;
        SetAirborne(1);
        _jumpVelX = JumpSpeedHorizontal * (IsFacingRight ? 1 : -1);
    }

    private void UpdateSetCollisionBoxesFrame()
    {
        if (_currentBoxesFrame != _nextBoxesFrame)
        {
            _currentBoxesFrame = _nextBoxesFrame;
            _currentHitboxHit = false;
        }
    }

    private void SetCollisionBoxesFrame(string name)
    {
        if (_isP1)
            Debug.Log("hurtboxes length: " + Hurtboxes.Length);

        Match match = Regex.Match(name, @"(.*)_\d\d.*");
        if (!match.Success)
        {
            Debug.LogError("Invalid collision boxes frame name: " + name);
        }

        string anim_name = match.Groups[1].Value;

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

    public void FireTrigger(string trigger)
    {
        _currentAnimHit = false;
        _animator.SetTrigger(trigger);
    }

    public void ResetTrigger(string trigger)
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

    private void SetAirborne(int isAirborne)
    {
        _animator.SetBool("grounded", isAirborne == 0);
        _isAirborne = isAirborne != 0;
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
        if ((IsFacingRight && scaleX < 0) || (!IsFacingRight && scaleX > 0))
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    public void ResetRound()
    {
        if (_animator != null)
        {
            FireTrigger(TRIGGER_PLAYER_STOP_CELEBRATION);
        }
        transform.position = _spawnPos;
        HP = MaxHP;
        _victoryFired = false;
        _timeoutFired = false;
        _isDead = false;
        _state = State.STATE_IDLE;
    }

    public void ThrowFireball()
    {
        ThrowFireball(FireballPrefab);
    }

    public void ThrowFireball(Projectile fireballPrefab)
    {
        // Return if a fireball already exists from this owner
        if (FireballExists())
        {
            return;
        }

        Projectile fireball = Instantiate(fireballPrefab, FireballSpawnPos.position, Quaternion.identity);

        fireball.GameController = GameController;
        fireball.Owner = this;
        fireball.Opponent = OtherPlayer;

        if (_direction == Direction.LEFT)
        {
            fireball.transform.localScale = Vector3.Scale(fireball.transform.localScale, new Vector3(-1, 1, 1));
            fireball.Velocity.Scale(new Vector3(-1, 1, 1));
        }

        GameController.Projectiles.Add(fireball);

        GameController.PlayThrowFireballSFX();
    }

    public bool FireballExists()
    {
        foreach (Projectile projectile in GameController.Projectiles)
        {
            if (projectile.Owner == this)
            {
                return true;
            }
        }
        return false;
    }

    private void ReleaseOpponent()
    {
        _throw_success = false;
        Holding = false;
        OtherPlayer.HandleThrow(ThrowAttackData);
    }

    // InputManager helpers
    private float GetAxis(string action)
    {
        if (Array.IndexOf(actionsToRecord, action) > -1 && _actionPlaybackFrame >= 0)
        {
            return _actionRecording[_actionPlaybackFrame].GetAxis(_direction, action);
        }
        return Input.GetAxis(string.Format("J{0}{1}", ControllerNum, ActionToAxis[action]));
    }

    private bool GetButton(string action)
    {
        if (Array.IndexOf(actionsToRecord, action) > -1 && _actionPlaybackFrame >= 0)
        {
            return _actionRecording[_actionPlaybackFrame].GetButton(action);
        }
        return Input.GetButton(string.Format("J{0}{1}", ControllerNum, ActionToAxis[action]));
    }

    private bool GetButtonDown(string action)
    {
        if (Array.IndexOf(actionsToRecord, action) > -1 && _actionPlaybackFrame >= 0)
        {
            return _actionRecording[_actionPlaybackFrame].GetButtonDown(action);
        }
        return Input.GetButtonDown(string.Format("J{0}{1}", ControllerNum, ActionToAxis[action]));
    }

    private void UpdateBufferedInputs()
    {
        string[] keys = new string[_actionBuffers.Keys.Count];
        _actionBuffers.Keys.CopyTo(keys, 0);

        foreach (string action in keys)
        {
            if (GetButtonDown(action))
            {
                _actionBuffers[action] = _globals.InputBufferFrames;
            }
            else
            {
                _actionBuffers[action]--;
            }
        }
    }

    public bool GetButtonDownWithBuffer(string action)
    {
        if (Array.IndexOf(actionsToRecord, action) > -1 && _actionPlaybackFrame >= 0)
        {
            return _actionRecording[_actionPlaybackFrame].GetButtonDownWithBuffer(action);
        }
        return _actionBuffers[action] > 0;
    }

    private void StartActionRecording()
    {
        _actionRecording.Clear();
        _recordingFrames = 0;
        isRecordingActions = true;
    }

    private void RecordActions()
    {        
        _actionRecording.Add(new ActionRecordingFrame(this));
        _recordingFrames++;
    }

    private void StopActionRecording()
    {
        isRecordingActions = false;
    }

    private void StartActionPlayback()
    {
        _actionPlaybackFrame = 0;
    }

    private void PlayHitSoundEffect()
    {
        GameController.PlayHitSFX();
    }

    private void PlayHitHardSoundEffect()
    {
        GameController.PlayHitHardSFX();
    }
}
