using UnityEngine;
using System;
using System.Collections;

public class GameController : MonoBehaviour
{
    public GameplayData Data;

    public float GraphicTimerMax = 90;
    private float _graphicTimer;

    private GameplayData.State[] _initialStates;
    private int _initialStatesIndex;

    public float KOFreezeTimer = 90;
    public float DeadPlayerOnGroundTimer = 20;
    public float CelebrationTimer = 30;
    public float PostCelebrationTimer = 50;
    public float SlowdownTimeScale = 0.35f;
    public float TimeScaleSpeedRegen = 0.025f;
    private float _postKOStateTimer;
    private bool _inPostKO;

    private const string GLOBALS_NAME = "Globals";
    private static Globals _globals;

    private PlayerController _deadPlayer;
    private bool _doubleKO;

    // Use this for initialization
    void Start()
    {
        _graphicTimer = GraphicTimerMax;
        _initialStates = new GameplayData.State[] {
            GameplayData.State.ANNOUNCE_ROUND_1, GameplayData.State.ANNOUNCE_FIGHT,
            GameplayData.State.GAMEPLAY
        };
        _initialStatesIndex = 0;

        Data.CurrentState = _initialStates[_initialStatesIndex];

        _postKOStateTimer = 0;
        _inPostKO = false;
        _doubleKO = false;

        if (_globals == null)
        {
            _globals = Resources.Load<Globals>(GLOBALS_NAME);
        }

        _globals.TimeScale = 1;
    }

    // Update is called once per frame
    void Update()
    {
        // Update in initial states
        if (_graphicTimer-- <= 0 && _initialStatesIndex < _initialStates.Length - 1)
        {
            _initialStatesIndex++;
            _graphicTimer = GraphicTimerMax;

            Data.CurrentState = _initialStates[_initialStatesIndex];
        }

        // Update in ko states
        if (_inPostKO)
        {
            switch (Data.CurrentState)
            {
                // Time freeze when attack connects
                case GameplayData.State.ANNOUNCE_KO:
                case GameplayData.State.ANNOUNCE_DOUBLE_KO:
                    if (_postKOStateTimer-- <= 0)
                    {
                        Data.CurrentState = GameplayData.State.KO_POST_FREEZE;
                        _postKOStateTimer = DeadPlayerOnGroundTimer;
                    }
                    else
                    {
                        _globals.TimeScale = 0;
                    }
                    break;

                // Time slowdown when player is falling to ground
                case GameplayData.State.KO_POST_FREEZE:
                    if (_deadPlayer.InGroundKnockdown && _postKOStateTimer-- <= 0)
                    {
                        if (_globals.TimeScale < 1)
                        {
                            _globals.TimeScale += TimeScaleSpeedRegen;
                            _globals.TimeScale = Mathf.Min(_globals.TimeScale, 1);
                        }
                        else
                        {
                            Data.CurrentState = GameplayData.State.CELEBRATION;
                            _postKOStateTimer = CelebrationTimer;
                        }
                    }
                    else
                    {
                        _globals.TimeScale = SlowdownTimeScale;
                    }
                    break;

                // Winner victory pose/quote
                case GameplayData.State.CELEBRATION:
                    Data.CurrentState = GameplayData.State.POST_CELEBRATION;
                    break;

                // Apply round win, restart after this state
                case GameplayData.State.POST_CELEBRATION:
                    break;
            }
        }
    }

    public void FireRoundEnd(PlayerController deadPlayer)
    {
        Debug.Log("FIRE ROUND END");

        _postKOStateTimer = KOFreezeTimer;
        _inPostKO = true;

        // If dead player has already been set before, assume that a double ko
        // has happened
        if (_deadPlayer != null)
        {
            _doubleKO = true;
            Data.CurrentState = GameplayData.State.ANNOUNCE_DOUBLE_KO;
        }
        else
        {
            Data.CurrentState = GameplayData.State.ANNOUNCE_KO;
            _deadPlayer = deadPlayer;
        }
    }
}
