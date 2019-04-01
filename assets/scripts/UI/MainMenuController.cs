using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    public string EVAN_URL;

    public Animator ButtonGroup;
    public Animator Settings;
    public Animator Credits;
    public Animator ControllerAssign;
    public Animator CharacterSelect;

    public Button VersusButton;
    public Button CreditsBackButton;
    public Button SettingsBackButton;
    public Button ControllerAssignBackButton;
    public Button ControllerAssignCharSelectButton;

    public CharSelect P1CharSelect;
    public CharSelect P2CharSelect;

    public bool PollControllers = false;

    public StandaloneInputModule InputModule;

    public string GameSceneName;

    private string GLOBALS_NAME = "Globals";
    private Globals _globals;

	// Use this for initialization
	void Start ()
    {
		_globals = Resources.Load<Globals>(GLOBALS_NAME);
        updateInputAxes();
    }

	// Update is called once per frame
	void Update ()
    {
        updateInputAxes();

        // Poll controllers for assignment
        if (PollControllers)
        {
            Utils.PollControllersForUnassignedPlayers("Start");
        }

        // Update "clickability" of continue button on controller select
        ControllerAssignCharSelectButton.interactable = _globals.Player1Controller != -1 && _globals.Player2Controller != -1;

        if (ControllerAssignCharSelectButton.interactable)
        {
            Navigation nav = ControllerAssignBackButton.navigation;
            nav.mode = Navigation.Mode.Explicit;
            ControllerAssignBackButton.navigation = nav;
        }
        else
        {
            Navigation nav = ControllerAssignBackButton.navigation;
            nav.mode = Navigation.Mode.None;
            ControllerAssignBackButton.navigation = nav;
        }

        // Check if we can play the game
        if (P1CharSelect.SelectionMade && P2CharSelect.SelectionMade)
        {
            PlayGame();
        }
    }

    private void updateInputAxes()
    {
        InputModule.horizontalAxis = string.Format("J{0}{1}", _globals.BackupP1Controller, "Horizontal");
        InputModule.verticalAxis = string.Format("J{0}{1}", _globals.BackupP1Controller, "Vertical");
        InputModule.submitButton = string.Format("J{0}{1}", _globals.BackupP1Controller, "A");
        InputModule.cancelButton = string.Format("J{0}{1}", _globals.BackupP1Controller, "B");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void OpenVersus()
    {
        Utils.ResetControllerAssignment();

        ButtonGroup.SetBool("Hidden", true);
        ControllerAssign.SetTrigger("OpenControllerAssign");
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseVersus()
    {
        if (_globals.Player1Controller == -1)
        {
            _globals.Player1Controller = _globals.BackupP1Controller;
        }

        ButtonGroup.SetBool("Hidden", false);
        ControllerAssign.SetTrigger("BackToMainMenu");
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenCharSelect()
    {
        ControllerAssign.SetTrigger("OpenCharSelect");
        CharacterSelect.SetBool("Hidden", false);
        P1CharSelect.ResetCharSelect();
        P2CharSelect.ResetCharSelect();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseCharSelect()
    {
        CharacterSelect.SetBool("Hidden", true);
        ButtonGroup.SetBool("Hidden", false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        ButtonGroup.SetBool("Hidden", true);
        Settings.SetBool("Hidden", false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        ButtonGroup.SetBool("Hidden", false);
        Settings.SetBool("Hidden", true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenCredits()
    {
        ButtonGroup.SetBool("Hidden", true);
        Credits.SetBool("Hidden", false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseCredits()
    {
        ButtonGroup.SetBool("Hidden", false);
        Credits.SetBool("Hidden", true);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SetMasterVolume(Slider slider)
    {
        _globals.MasterVolume = slider.value;
    }

    public void DiscoverSickAssMusic()
    {
        Application.OpenURL(EVAN_URL);
    }
}
