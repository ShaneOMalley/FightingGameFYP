using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Animator ButtonGroup;
    public Animator Settings;
    public Animator Credits;

    public string GameSceneName;

    private string GLOBALS_NAME = "Globals";
    private Globals _globals;

	// Use this for initialization
	void Start ()
    {
		_globals = Resources.Load<Globals>(GLOBALS_NAME);
    }
	
	// Update is called once per frame
	void Update ()
    {

	}

    public void PlayGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void OpenSettings()
    {
        ButtonGroup.SetBool("Hidden", true);
        Settings.SetBool("Hidden", false);
    }

    public void CloseSettings()
    {
        ButtonGroup.SetBool("Hidden", false);
        Settings.SetBool("Hidden", true);
    }

    public void OpenCredits()
    {
        ButtonGroup.SetBool("Hidden", true);
        Credits.SetBool("Hidden", false);
    }

    public void CloseCredits()
    {
        ButtonGroup.SetBool("Hidden", false);
        Credits.SetBool("Hidden", true);
    }

    public void SetMasterVolume(Slider slider)
    {
        _globals.MasterVolume = slider.value;
    }
}
