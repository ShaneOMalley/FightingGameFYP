using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHPDisplay : MonoBehaviour {

    public PlayerController Player;

    private string _player_prefix;
    private Text _text;
    
	void Start ()
    {
        _player_prefix = Player.Prefix;
        _text = GetComponent<Text>();
	}

    private void Update()
    {
        _text.text = string.Format("{0}HP: {1}", _player_prefix, Player.HP);
    }
}
