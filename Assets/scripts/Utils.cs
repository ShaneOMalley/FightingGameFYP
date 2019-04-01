using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pair<T1, T2>
{
    public T1 First { get; set; }
    public T2 Second { get; set; }

    public Pair(T1 first, T2 second)
    {
        First = first;
        Second = second;
    }
}

public static class Utils
{
    private static Globals _globals = Resources.Load<Globals>("Globals");

    private const int numControllers = 5;

    public static float TimeCorrection
    {
        get
        {
            //return Time.deltaTime / 0.016f;
            return 1;
        }
    }

    public static float KnockbackSpeedToDst(float speed, float dec)
    {
        int num_steps = (int)(speed / dec) + 1;
        return speed * num_steps - (dec * (((num_steps - 1) * num_steps) / 2));
    }

    public static float NormalDeltaTime()
    {
        return Time.deltaTime * 60;
    }

    public static void ResetControllerAssignment(int playerNum)
    {
        if (playerNum == 1)
        {
            _globals.Player1Controller = -1;
        }
        else if (playerNum == 2)
        {
            _globals.Player2Controller = -1;
        }
    }

    public static void ResetControllerAssignment()
    {
        ResetControllerAssignment(1);
        ResetControllerAssignment(2);
    }
    
    public static bool PollControllersForAssignment(int playerNum, string axis)
    {
        for (int controllerNum = 1; controllerNum <= numControllers; controllerNum++)
        {
            if (Input.GetButtonDown(string.Format("J{0}{1}", controllerNum, axis)))
            {
                // Is controller already in use?
                if (_globals.Player1Controller == controllerNum || _globals.Player2Controller == controllerNum)
                {
                    continue;
                }

                // Assign controller
                if (playerNum == 1)
                {
                    _globals.Player1Controller = controllerNum;
                    _globals.BackupP1Controller = controllerNum;
                }
                else
                {
                    _globals.Player2Controller = controllerNum;
                }
                return true;
            }
        }
        return false;
    }

    public static void PollControllersForUnassignedPlayers(string axis)
    {
        if (_globals.Player1Controller == -1)
        {
            PollControllersForAssignment(1, axis);
        }
        else if (_globals.Player2Controller == -1)
        {
            PollControllersForAssignment(2, axis);
        }
    }
}
