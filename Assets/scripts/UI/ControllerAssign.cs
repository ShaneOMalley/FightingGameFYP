using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerAssign : MonoBehaviour {

    public enum PlayerNum { P1, P2 };
    public PlayerNum Player = PlayerNum.P1;

    private Animator _animator;
    public Text Prompt;

    private const string GLOBALS_NAME = "Globals";
    private Globals _globals;

	// Use this for initialization
	void Start ()
    {
        _globals = Resources.Load<Globals>(GLOBALS_NAME);
        _animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Check which controller is assigned to this
        int controllerAssigned = (Player == PlayerNum.P1 ? _globals.Player1Controller : _globals.Player2Controller);

        // Update prompt text
        if (controllerAssigned == -1)
        {
            Prompt.text = "Press Start";
        }
        else
        {
            Prompt.text = string.Format("Player {0} OK", Player == PlayerNum.P1 ? "1" : "2");
        }

        // Animate icons
        _animator.SetBool("assigned", controllerAssigned != -1);
	}
}
