using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharSelect : MonoBehaviour {

    public bool Updating = false;

    public MainMenuController MMController;

    public RectTransform ZangiefPortrait;
    public RectTransform CharliePortrait;
    public RectTransform RyuPortrait;
    public Image Cursor;
    public Text NamePreview;
    
    public MenuAudioPlayer AudioPlayer;

    public enum PlayerNum { P1, P2 }
    public PlayerNum Player = PlayerNum.P1;
    
    [HideInInspector]
    public bool SelectionMade = false;

    private enum Character { ZANGIEF = 0, CHARLIE, RYU, NUM_CHARACTERS }
    private Character _selectedCharacter = Character.CHARLIE;

    private const string GLOBALS_NAME = "Globals";
    private Globals _globals;
    private Animator _cursorAnimator;

    private float _horizontalPrevious;

    private int _controllerNum
    {
        get
        {
            return Player == PlayerNum.P1 ? _globals.Player1Controller : _globals.Player2Controller;
        }
    }

	// Use this for initialization
	void Start ()
    {
        _globals = Resources.Load<Globals>(GLOBALS_NAME);
        _cursorAnimator = Cursor.GetComponent<Animator>();
        _horizontalPrevious = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Updating)
        {
            // Move character selection
            if (!SelectionMade && GetHorizontalDown() != 0)
            {
                int newCharIndex = (int)_selectedCharacter;
                newCharIndex += (int)GetHorizontalDown() * (Player == PlayerNum.P1 ? 1 : -1);
                newCharIndex %= (int)Character.NUM_CHARACTERS;
                if (newCharIndex < 0)
                {
                    newCharIndex += (int)Character.NUM_CHARACTERS;
                }
                _selectedCharacter = (Character)newCharIndex;

                AudioPlayer.PlayButtonHighlightSFX();
            }

            // Select character
            if (GetSubmit())
            {
                if (!SelectionMade)
                {
                    SetSelectionMade(true);
                    AudioPlayer.PlayButtonSelectSFX();
                }
            }

            // Deselect character
            if (GetCancel())
            {
                if (SelectionMade)
                {
                    SetSelectionMade(false);
                }
                else
                {
                    MMController.CloseCharSelect();
                }
            }

            // Update value for use with getHorizontalDown()
            _horizontalPrevious = GetHorizontal();
        }

        // Move cursor to selected character, update name preview and update global selection
		switch(_selectedCharacter)
        {
            case Character.ZANGIEF:
                Cursor.rectTransform.position = ZangiefPortrait.position;
                NamePreview.text = "ZANGIEF";
                UpdateCharacter(Globals.Character.ZANGIEF);
                break;

            case Character.CHARLIE:
                Cursor.rectTransform.position = CharliePortrait.position;
                NamePreview.text = "CHARLIE";
                UpdateCharacter(Globals.Character.CHARLIE);
                break;

            case Character.RYU:
                Cursor.rectTransform.position = RyuPortrait.position;
                NamePreview.text = "RYU";
                UpdateCharacter(Globals.Character.RYU);
                break;
        }
	}

    public void ResetCharSelect()
    {
        _selectedCharacter = Character.CHARLIE;
        SetSelectionMade(false);
    }

    private void SetSelectionMade(bool selectionMade)
    {
        SelectionMade = selectionMade;
        _cursorAnimator.SetBool("selected", selectionMade);
    }

    private void UpdateCharacter(Globals.Character character)
    {
        if (Player == PlayerNum.P1)
        {
            _globals.Player1Character = character;
        }
        else
        {
            _globals.Player2Character = character;
        }
    }

    private bool GetSubmit()
    {
        return Input.GetButtonDown(string.Format("J{0}A", _controllerNum));
    }

    private bool GetCancel()
    {
        return Input.GetButtonDown(string.Format("J{0}B", _controllerNum));
    }

    private float GetHorizontal()
    {
        return Input.GetAxis(string.Format("J{0}Horizontal", _controllerNum));
    }

    private float GetHorizontalDown()
    {
        if (Math.Sign(GetHorizontal()) != Math.Sign(_horizontalPrevious))
        {
            return Math.Sign(GetHorizontal());
        }
        return 0;
    }

}
