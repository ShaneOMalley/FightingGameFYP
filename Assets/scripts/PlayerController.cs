using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float WalkSpeedForward = 4;
    public float WalkSpeedBackward = 3.5f;

    public GameCamera CurrentGameCamera;
    public PlayerController OtherPlayer;

    public enum PlayerEnum { P1, P2 }
    public PlayerEnum PlayerNum = PlayerEnum.P1;

    private enum State {
        STATE_IDLE = 0,
        STATE_WALK = 1,
        STATE_ATTACK = 2,
        STATE_HIT = 3,
        STATE_BLOCK = 4
    }
    private enum Direction { RIGHT, LEFT }

    private const string TRIGGER_RYU_ATTACK = "play_attack";
    private const string TRIGGER_RYU_HIT = "play_hit";

    private Transform _currentBoxesFrame;
    private Animator _animator;
    private Direction _direction = Direction.RIGHT;
    private bool _movingForward = true;

    private const float KNOCKBACK_DECELERATION = 0.05f;
    private int _hitstun_frames = 0;
    private float _knockback_speed = 0;

    private bool _currentHitboxHit = false;

    // Properties
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

    public BoxCollider2D PushBox
    {
        get { return CurrentBoxesFrame.Find("push").GetComponent<BoxCollider2D>(); }
    }

    private bool _isP1
    {
        get { return PlayerNum == PlayerEnum.P1; }
    }

    private string _axis_prefix
    {
        get { return _isP1 ? "P1_" : "P2_"; }
    }

    private bool _inHitstun
    {
        get { return _hitstun_frames > 0; }
    }

    private bool _isStuck
    {
        get
        {
            // Is the player attacking?
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("ryu_attack"))
                return true;

            // Is the player in hitstun?
            if (_inHitstun)
                return true;

            return false;
        }
    }


    // Use this for initialization
    void Start ()
    {
        _animator = this.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Movement
        if (GetAxis("Horizontal") > 0 && !_isStuck)
        {
            SetState(State.STATE_WALK);

            SetForward(_direction == Direction.RIGHT);
            float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

            transform.Translate(speed * Time.deltaTime, 0, 0);
        }
        else if (GetAxis("Horizontal") < 0 && !_isStuck)
        {
            SetState(State.STATE_WALK);

            SetForward(_direction == Direction.LEFT);
            float speed = (_movingForward ? WalkSpeedForward : WalkSpeedBackward);

            transform.Translate(-speed * Time.deltaTime, 0, 0);
        }
        else if (!_isStuck)
        {
            SetState(State.STATE_IDLE);
        }

        // Attacking
        if (GetButtonDown("Attack") && !_isStuck)
        {
            FireTrigger(TRIGGER_RYU_ATTACK);
        }

        // Update direction to face the other player
        if (!_inHitstun)
        {
            if (OtherPlayer.transform.position.x > transform.position.x)
            {
                _direction = Direction.RIGHT;
            }
            else
            {
                _direction = Direction.LEFT;
            }

            UpdateTransformDirection();
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

        // Hit detection
        if (!_currentHitboxHit && HitBoxes != null && HitBoxes.IsIntersecting(OtherPlayer.Hurtboxes))
        {
            Hitbox hitbox = HitBoxes[0].GetComponent<Hitbox>();
            OtherPlayer.Hit(hitbox);
            KnockbackSpeed(hitbox.PushbackHit);
            _currentHitboxHit = true;
        }

        // Hitstun handling
        if (_inHitstun)
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

        // Knockback
        if (_knockback_speed != 0)
        {
            transform.Translate(_knockback_speed, 0, 0);

            float prev_speed = _knockback_speed;
            _knockback_speed -= KNOCKBACK_DECELERATION * Mathf.Sign(_knockback_speed);
            if (Mathf.Sign(_knockback_speed) != Mathf.Sign(prev_speed))
            {
                _knockback_speed = 0;
            }
        }
    }

    public void Hit(int num_frames, float relative_knockback = 0, int damage = 0)
    {
        Debug.LogFormat("{0:d}, {1:f}, {2:d}", num_frames, relative_knockback, damage);

        _hitstun_frames = num_frames;
        KnockbackSpeed(relative_knockback);
        FireTrigger(TRIGGER_RYU_HIT);
    }

    public void Hit(Hitbox hitbox)
    {
        Hit(hitbox.Hitstun, hitbox.KnocbackHit, hitbox.Damage);
    }

    public void KnockbackSpeed(float relative_knockback)
    {
        _knockback_speed = relative_knockback * (_direction == Direction.RIGHT ? -1 : 1);
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
                _currentBoxesFrame = frame;
                _currentHitboxHit = false;
                break;
            }
        }
    }

    private void SetState(State state)
    {
        _animator.SetInteger("state", (int)state);
    }

    private void FireTrigger(string trigger)
    {
        _animator.SetTrigger(trigger);
    }

    private void SetForward(bool isForward)
    {
        _animator.SetBool("moving_forward", isForward);
        _movingForward = isForward;
    }

    private void UpdateTransformDirection()
    {
        float scaleX = transform.localScale.x;
        if ((_direction == Direction.RIGHT && scaleX < 0) || (_direction == Direction.LEFT && scaleX > 0))
        {
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
        }
    }

    // InputManager helpers
    private float GetAxis(string axis)
    {
        return Input.GetAxisRaw(_axis_prefix + axis);
    }

    private bool GetButton(string button)
    {
        return Input.GetButton(_axis_prefix + button);
    }

    private bool GetButtonDown(string button)
    {
        return Input.GetButtonDown(_axis_prefix + button);
    }
}
