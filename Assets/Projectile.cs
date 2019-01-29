using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public AttackData AttackData;
    public Vector3 Velocity;
    
    [HideInInspector]
    public PlayerController Owner;
    [HideInInspector]
    public PlayerController Opponent;
    [HideInInspector]
    public GameController GameController;

    private Animator _animator;
    private BoxCollider2D _bounds;

    private bool _dissapated;

    private const string PLAY_DISSAPATE = "play_dissapate";

	// Use this for initialization
	void Start ()
    {
        _bounds = GetComponent<BoxCollider2D>();
        _animator = this.GetComponent<Animator>();

        _dissapated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_dissapated)
        {
            transform.Translate(Velocity);
        }
    }

    void LateUpdate ()
    {
        if (!_dissapated)
        {
            if (_bounds.bounds.max.x < GameController.CurrentGameCamera.Left
                || _bounds.bounds.min.x > GameController.CurrentGameCamera.Right)
            {
                GameController.Projectiles.Remove(this);
                Destroy(gameObject);
            }

            // Collision with other projectiles
            foreach (Projectile otherProjectile in GameController.Projectiles)
            {
                // Don't test against self
                if (otherProjectile == this)
                    continue;

                // Skip if both projectiles have the same owner
                if (otherProjectile.Owner == Owner)
                    continue;

                // Skip if other projectile is dissapated
                if (otherProjectile._dissapated)
                    continue;

                // Dissapate on collision with another projectile
                if (_bounds.bounds.Intersects(otherProjectile._bounds.bounds))
                {
                    Dissapate();
                    otherProjectile.Dissapate();
                }
            }

            // Collision with opponent
            Vector3 hitPos;
            Debug.Log(_bounds.IsIntersecting(Opponent.Hurtboxes, out hitPos));
            if (_bounds.IsIntersecting(Opponent.Hurtboxes, out hitPos))
            {
                Opponent.HandleContact(AttackData, hitPos);
                Dissapate();
            }
        }
    }

    public void Dissapate()
    {
        GameController.Projectiles.Remove(this);
        _animator.SetTrigger(PLAY_DISSAPATE);
        _dissapated = true;
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
