using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public string nextScene;

    private enum Player { P1, P2, ANY }

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GetButton(Player.ANY, "Start"))
            SceneManager.LoadScene(nextScene);
	}

    private bool GetButton(Player player, string axis)
    {
        if (player == Player.P1)
        {
            return Input.GetButtonDown("P1_" + axis);
        }
        else if (player == Player.P2)
        {
            return Input.GetButtonDown("P2_" + axis);
        }
        
        // Check either p1 or p2
        return GetButton(Player.P1, axis) || GetButton(Player.P2, axis);
    }
}
