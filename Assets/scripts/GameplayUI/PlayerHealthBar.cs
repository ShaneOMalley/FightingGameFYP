using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour {

    public RectTransform Mask;
    public RectTransform MaskLag;
    public PlayerController Player;
    public int DelayLagMax = 25;
    public float LerpFactorLag = 0.25f;

    private float _height;
    private float _heightLag;
    private float _widthMax;
    private float _widthMaxLag;
    private float _HPScaleLag;
    private float _previousPlayerHP;
    private float _delayLag;

	// Use this for initialization
	void Start ()
    {
        _height = Mask.sizeDelta.y;
        _heightLag = MaskLag.sizeDelta.y;
        _widthMax = Mask.sizeDelta.x;
        _widthMaxLag = MaskLag.sizeDelta.x;
        _previousPlayerHP = Player.HP;
        _delayLag = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Update the green bar immediately
        float hpScale = (float)Player.HP / Player.MaxHP;
        Mask.sizeDelta = new Vector2(_widthMax * hpScale, _height);

        // Check to see if player's HP has changed since last frame
        if (Player.HP != _previousPlayerHP)
        {
            _delayLag = DelayLagMax;
        }

        // Update the lagging bar if not in delay
        if (_delayLag-- <= 0)
        {
            _HPScaleLag = Mathf.Lerp(_HPScaleLag, hpScale, LerpFactorLag);
            MaskLag.sizeDelta = new Vector2(_widthMax * _HPScaleLag, _height);
        }

        // Store player's previous HP
        _previousPlayerHP = Player.HP;
	}
}
