using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerAssignPanel : MonoBehaviour {

    public MainMenuController MainMenuController;

    private void SetControllerPolling(int isPolling)
    {
        MainMenuController.PollControllers = isPolling != 0;
    }
}
