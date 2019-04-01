using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PostGameController : MonoBehaviour {

    public Image WinnerImage;
    public Text WinnerQuote;

    public Sprite RyuVictory;
    public Sprite CharlieVictory;
    public Sprite ZangiefVictory;

    public string RyuWinQuote;
    public string CharlieWinQuote;
    public string ZangiefWinQuote;

    public string GameSceneName;
    public string MainMenuSceneName;

    public StandaloneInputModule InputModule;

    private const string GLOBALS_NAME = "Globals";
    private Globals _globals;

    private void Start()
    {
        _globals = Resources.Load<Globals>(GLOBALS_NAME);
        updateInputAxes();

        bool p1Win = _globals.PlayerWinning == Globals.Player.P1;
        switch (p1Win ? _globals.Player1Character : _globals.Player2Character)
        {
            case Globals.Character.RYU:
                WinnerImage.sprite = RyuVictory;
                WinnerQuote.text = RyuWinQuote;
                break;

            case Globals.Character.CHARLIE:
                WinnerImage.sprite = CharlieVictory;
                WinnerQuote.text = CharlieWinQuote;
                break;

            case Globals.Character.ZANGIEF:
                WinnerImage.sprite = ZangiefVictory;
                WinnerQuote.text = ZangiefWinQuote;
                break;
        }
    }

    private void Update()
    {
        updateInputAxes();
    }

    public void Rematch()
    {
        SceneManager.LoadScene(GameSceneName);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void updateInputAxes()
    {
        InputModule.horizontalAxis = string.Format("J{0}{1}", _globals.BackupP1Controller, "Horizontal");
        InputModule.verticalAxis = string.Format("J{0}{1}", _globals.BackupP1Controller, "Vertical");
        InputModule.submitButton = string.Format("J{0}{1}", _globals.BackupP1Controller, "A");
        InputModule.cancelButton = string.Format("J{0}{1}", _globals.BackupP1Controller, "B");
    }
}
