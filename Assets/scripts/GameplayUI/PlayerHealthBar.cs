using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour {

    public RectTransform Mask;
    public RectTransform MaskLag;
    public PlayerController Player;
    public int LaunchDelayLag = 25;
    public float LerpFactorLag = 0.25f;

    private float _height;
    private float _heightLag;
    private float _widthMax;
    private float _widthMaxLag;
    private float _HPScaleLag;
    private float _previousPlayerHP;
    private bool _playerInHitStunPrevious;
    private bool _playerInAirKnockdownPrevious;
    private float _delayLag;

	// Use this for initialization
	void Start ()
    {
        _height = Mask.sizeDelta.y;
        _heightLag = MaskLag.sizeDelta.y;
        _widthMax = Mask.sizeDelta.x;
        _widthMaxLag = MaskLag.sizeDelta.x;
        _previousPlayerHP = Player.HP;
        _playerInHitStunPrevious = Player.InHitStun;
        _playerInAirKnockdownPrevious = Player.InAirKnockdown;
        _delayLag = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Update green bar pos
        float hpScale = (float)Player.HP / Player.MaxHP;

        // Apply an artificial lag to the red bar when player is launched
        if (Player.InAirKnockdown && !_playerInAirKnockdownPrevious)
        {
            _delayLag = LaunchDelayLag;
        }

        // Snap red bar to old hp when player has been put into hitstun from
        // not hitstun
        if (Player.InHitStun && !_playerInHitStunPrevious)
        {
            _HPScaleLag = (float)_previousPlayerHP / Player.MaxHP;
        }

        // Decrease red bar when not in hitstun, or if not artificially lagging
        if (_delayLag-- <= 0 && !Player.InHitStun)
        {
            _HPScaleLag = Mathf.Lerp(_HPScaleLag, hpScale, LerpFactorLag);
        }

        // Update bars
        Mask.sizeDelta = new Vector2(_widthMax * hpScale, _height);
        MaskLag.sizeDelta = new Vector2(_widthMax * _HPScaleLag, _height);

        // Store player's previous HP and hitstun state
        _previousPlayerHP = Player.HP;
        _playerInHitStunPrevious = Player.InHitStun;
        _playerInAirKnockdownPrevious = Player.InAirKnockdown;
	}
}
