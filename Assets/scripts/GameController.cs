using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameplayData Data;
    public Image Black;
    public float BlackAlpha = 0;

    public float GraphicTimerMax = 90;
    private float _graphicTimer;

    private GameplayData.State[] _initialStates;
    private int _initialStatesIndex;

    public float KOFreezeTimer = 90;
    public float DeadPlayerOnGroundTimer = 20;
    public float CelebrationTimer = 30;
    public float PostCelebrationTimer = 50;
    public float TimeoutTimer = 90;
    public float SlowdownTimeScale = 0.35f;
    public float TimeScaleSpeedRegen = 0.025f;
    private float _postKOStateTimer;
    private bool _inPostKO;
    private bool _nextRoundFired;
    private bool _awardedRound;

    private int _cameraShakeLeft;

    private const string GLOBALS_NAME = "Globals";
    private static Globals _globals;
    private bool _doubleKO;
    private bool _KOFired;

    private int _currentRoundNum;

    private Animator _animator;
    private const string NEXT_ROUND_TRIGGER = "play_next_round";
    private const string TRIGGER_STOP_CELEBRATION = "stop_celebration";

    private const string POST_GAME_SCENE_NAME = "PostGame";

    public GameCamera CurrentGameCamera;
    public List<Projectile> Projectiles;

    // Use this for initialization
    void Start()
    {
        _initialStates = new GameplayData.State[] {
            GameplayData.State.ANNOUNCE_ROUND_1, GameplayData.State.ANNOUNCE_FIGHT,
            GameplayData.State.GAMEPLAY
        };

        if (_globals == null)
        {
            _globals = Resources.Load<Globals>(GLOBALS_NAME);
        }

        _animator = GetComponent<Animator>();

        _globals.TimeScale = 1;

        ResetGame();
        Data.ResetAll();

        _cameraShakeLeft = 0;

        Projectiles = new List<Projectile>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update Black's alpha
        Black.color = new Color(Black.color.r, Black.color.g, Black.color.b, BlackAlpha);

        // Who is winning?
        PlayerController playerWinning = Data.P1.HP > Data.P2.HP ? Data.P1 : Data.P2;
        if (Data.P1.HP == Data.P2.HP || Data.P1.HP < 0 && Data.P2.HP < 0)
            playerWinning = null;

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
                    if ((Data.P1.InGroundKnockdown || Data.P2.InGroundKnockdown) && _postKOStateTimer-- <= 0)
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

                    if (!_awardedRound)
                    {
                        if (playerWinning == Data.P1)
                            Data.P1Rounds++;
                        else if (playerWinning == Data.P2)
                            Data.P2Rounds++;
                        _awardedRound = true;
                    }

                    if (_postKOStateTimer-- <= 0)
                    {
                        if (playerWinning != null)
                            Data.CurrentState = GameplayData.State.ANNOUNCE_RYU_WIN;
                        else
                            Data.CurrentState = GameplayData.State.ANNOUNCE_DRAW;
                        _postKOStateTimer = PostCelebrationTimer;
                    }

                    break;

                // Apply round win, restart after this state
                case GameplayData.State.ANNOUNCE_RYU_WIN:
                case GameplayData.State.ANNOUNCE_DRAW:
                    if (_postKOStateTimer-- <= 0 && !_nextRoundFired)
                    {
                        _animator.SetTrigger(NEXT_ROUND_TRIGGER);
                        _nextRoundFired = true;
                    }
                    break;

                // Timeout
                case GameplayData.State.TIMEOUT_PAUSE:
                    if (_postKOStateTimer-- <= 0)
                    {
                        Data.CurrentState = GameplayData.State.CELEBRATION;
                    }
                    break;
            }
        }

        // Handle time freezing
        //if (Data.CurrentState == GameplayData.State.GAMEPLAY)
        //{
        //    if (_cameraShakeLeft > 0)
        //    {
        //        _globals.TimeScale = 0;
        //    }
        //    else
        //    {
        //        _globals.TimeScale = 1;
        //    }
        //}
        _cameraShakeLeft--;
    }

    public void FireRoundEnd()
    {
        _postKOStateTimer = KOFreezeTimer;
        _inPostKO = true;

        // If KO has been fired already, assume that a double KO has happened
        if (_KOFired)
        {
            _doubleKO = true;
            Data.CurrentState = GameplayData.State.ANNOUNCE_DOUBLE_KO;
        }
        else
        {
            Data.CurrentState = GameplayData.State.ANNOUNCE_KO;
        }
        _KOFired = true;
    }

    public void FireTimeout(PlayerController p1, PlayerController p2)
    {
        _postKOStateTimer = TimeoutTimer;
        _inPostKO = true;
    }

    public void ResetToRoundN(int roundNum)
    {
        Data.P1.ResetRound();
        Data.P2.ResetRound();

        if (roundNum < Data.MaxRounds * 2 - 1)
            _initialStates[0] = GameplayData.State.ANNOUNCE_ROUND_1 + roundNum - 1;
        else
            _initialStates[0] = GameplayData.State.ANNOUNCE_ROUND_FINAL;

        _initialStatesIndex = 0;
        Data.CurrentState = _initialStates[_initialStatesIndex];

        _graphicTimer = GraphicTimerMax;
        
        _postKOStateTimer = 0;
        _inPostKO = false;
        _doubleKO = false;
        _KOFired = false;
        _nextRoundFired = false;
        _awardedRound = false;

        Data.ResetTime();
    }

    public void ResetToNextRound()
    {
        ResetToRoundN(++_currentRoundNum);
    }

    public void ResetGame()
    {
        _currentRoundNum = 1;
        ResetToRoundN(_currentRoundNum);
        Data.P1Rounds = 0;
        Data.P2Rounds = 0;
    }

    public void EndGame()
    {
        SceneManager.LoadScene(POST_GAME_SCENE_NAME);
    }
    
    private void HandleRoundMovement()
    {
        if (_doubleKO)
        {
            ResetToRoundN(_currentRoundNum);
        }
        else if (Data.P1Rounds < Data.MaxRounds && Data.P2Rounds < Data.MaxRounds)
        {
            ResetToNextRound();
        }
        else
        {
            EndGame();
        }
    }

    //public void HitFreeze(int hitFreezeLength)
    //{
    //    _cameraShakeLeft = hitFreezeLength;
    //}

    public void CameraShake(int frames)
    {
        _cameraShakeLeft = frames;
    }

    public bool InHitFreeze()
    {
        return _cameraShakeLeft > 0;
            //&& (Data.P1.InHitStun || Data.P1.InBlockStun || Data.P1.InAirKnockdown 
            //|| Data.P2.InHitStun || Data.P2.InBlockStun || Data.P2.InAirKnockdown);
    }
}
