using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuPanel : MonoBehaviour {

    public Button StartingButton;

    public void EnableStartingButton()
    {
        EventSystem.current.SetSelectedGameObject(StartingButton.gameObject);
    }
}
