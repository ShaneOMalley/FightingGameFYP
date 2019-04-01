using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public string nextScene;

    private const string GLOBALS_NAME = "Globals";
    private Globals _globals;

	// Use this for initialization
	void Start ()
    {
        _globals = Resources.Load<Globals>(GLOBALS_NAME);
        Utils.ResetControllerAssignment();
	}
	
	// Update is called once per frame
	void Update ()
    {
        //if (GetButton(Player.ANY, "Start"))
        //    SceneManager.LoadScene(nextScene);

        if (_globals.Player1Controller == -1)
        {
            if (Utils.PollControllersForAssignment(1, "Start"))
                SceneManager.LoadScene(nextScene);
        }
	}

    //private bool GetButton(Player player, string axis)
    //{
    //    if (player == Player.P1)
    //    {
    //        return Input.GetButtonDown("P1_" + axis);
    //    }
    //    else if (player == Player.P2)
    //    {
    //        return Input.GetButtonDown("P2_" + axis);
    //    }
        
    //    // Check either p1 or p2
    //    return GetButton(Player.P1, axis) || GetButton(Player.P2, axis);
    //}
}
