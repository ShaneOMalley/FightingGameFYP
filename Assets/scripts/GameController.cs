using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameplayData Data;
    public PlayerController Player1Charlie;
    public PlayerController Player1Ryu;
    public PlayerController Player1Zangief;
    public PlayerController Player2Charlie;
    public PlayerController Player2Ryu;
    public PlayerController Player2Zangief;
    public GameCamera Camera;
    public PlayerHealthBar P1HealthBar;
    public PlayerHealthBar P2HealthBar;
    public RoundCounters P1RoundCounters;
    public RoundCounters P2RoundCounters;

    public AudioClip HitSFX;
    public AudioClip HitHardSFX;
    public AudioClip BlockSFX;
    public AudioClip ThrowFireballSFX;
    private AudioSource _audioSource;

    public Image Black;
    public float BlackAlpha = 0;

    public float GraphicTimerMax = 90;
    private float _graphicTimer;

    private GameplayData.State[] _initialStates;
    private int _initialStatesIndex;

    public float BaseTimeScale = 1;
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
    
    public List<Projectile> Projectiles;

    // Use this for initialization
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        _initialStates = new GameplayData.State[] {
            GameplayData.State.ANNOUNCE_ROUND_1, GameplayData.State.ANNOUNCE_FIGHT,
            GameplayData.State.GAMEPLAY
        };

        if (_globals == null)
        {
            _globals = Resources.Load<Globals>(GLOBALS_NAME);
        }

        // Set up players
        switch (_globals.Player1Character)
        {
            case Globals.Character.CHARLIE:
                Data.P1 = Player1Charlie;
                break;

            case Globals.Character.RYU:
                Data.P1 = Player1Ryu;
                break;

            case Globals.Character.ZANGIEF:
                Data.P1 = Player1Zangief;
                break;
        }

        switch (_globals.Player2Character)
        {
            case Globals.Character.CHARLIE:
                Data.P2 = Player2Charlie;
                break;

            case Globals.Character.RYU:
                Data.P2 = Player2Ryu;
                break;

            case Globals.Character.ZANGIEF:
                Data.P2 = Player2Zangief;
                break;
        }

        // Give the Players references to each other
        Data.P1.OtherPlayer = Data.P2;
        Data.P2.OtherPlayer = Data.P1;

        // Ectivate the Player GameObjects appropriately
        Player1Charlie.gameObject.SetActive(Data.P1 == Player1Charlie);
        Player1Ryu.gameObject.SetActive(Data.P1 == Player1Ryu);
        Player1Zangief.gameObject.SetActive(Data.P1 == Player1Zangief);
        Player2Charlie.gameObject.SetActive(Data.P2 == Player2Charlie);
        Player2Ryu.gameObject.SetActive(Data.P2 == Player2Ryu);
        Player2Zangief.gameObject.SetActive(Data.P2 == Player2Zangief);

        // Setup GameCamera
        Camera.Subjects = new Transform[2];
        Camera.Subjects[0] = Data.P1.transform;
        Camera.Subjects[1] = Data.P2.transform;

        // Setup Health Bars
        P1HealthBar.Player = Data.P1;
        P2HealthBar.Player = Data.P2;

        // Setup Round Counters
        P1RoundCounters.Player = Data.P1;
        P2RoundCounters.Player = Data.P2;

        ResetGame();
        Data.ResetAll();

        Projectiles = new List<Projectile>();

        _animator = GetComponent<Animator>();
        _globals.TimeScale = BaseTimeScale;
        _cameraShakeLeft = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Update Black's alpha
        Black.color = new Color(Black.color.r, Black.color.g, Black.color.b, BlackAlpha);

        // Who is winning?
        //_globals.PlayerWinning = Data.P1.HP > Data.P2.HP ? Data.P1 : Data.P2;
        _globals.PlayerWinning = Data.P1.HPPercentage > Data.P2.HPPercentage ? Globals.Player.P1 : Globals.Player.P2;
        if (Data.P1.HPPercentage == Data.P2.HPPercentage || Data.P1.HPPercentage < 0 && Data.P2.HPPercentage < 0)
            _globals.PlayerWinning = Globals.Player.NEITHER;

        // Update in initial states
        if (_graphicTimer-- <= 0 && _initialStatesIndex < _initialStates.Length - 1)
        {
            _initialStatesIndex++;
            _graphicTimer = GraphicTimerMax;

            Data.CurrentState = _initialStates[_initialStatesIndex];
        }

        // Timeout if time is up
        if (Data.FramesLeft <= 0 && !_inPostKO)
        {
            FireTimeout();
        }

        // Update in ko states
        if (_inPostKO)
        {
            switch (Data.CurrentState)
            {
                // Pause after time runs out
                case GameplayData.State.TIMEOUT_PAUSE:
                    if (_postKOStateTimer-- <= 0)
                    {
                        Data.CurrentState = GameplayData.State.CELEBRATION;
                        _postKOStateTimer = CelebrationTimer;
                    }
                    break;

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
                            _globals.TimeScale = Mathf.Min(_globals.TimeScale, BaseTimeScale);
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
                        if (_globals.PlayerWinning == Globals.Player.P1)
                            Data.P1Rounds++;
                        else if (_globals.PlayerWinning == Globals.Player.P2)
                            Data.P2Rounds++;
                        _awardedRound = true;
                    }

                    if (_postKOStateTimer-- <= 0)
                    {
                        if (_globals.PlayerWinning == Globals.Player.NEITHER)
                        {
                            Data.CurrentState = GameplayData.State.ANNOUNCE_DRAW;
                        }
                        else
                        {
                            if (_globals.PlayerWinning == Globals.Player.P1)
                            {
                                switch (_globals.Player1Character)
                                {
                                    case Globals.Character.RYU:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_RYU_WIN;
                                        break;

                                    case Globals.Character.CHARLIE:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_CHARLIE_WIN;
                                        break;

                                    case Globals.Character.ZANGIEF:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_ZANGIEF_WIN;
                                        break;
                                }
                            }

                            else
                            {
                                switch (_globals.Player2Character)
                                {
                                    case Globals.Character.RYU:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_RYU_WIN;
                                        break;

                                    case Globals.Character.CHARLIE:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_CHARLIE_WIN;
                                        break;

                                    case Globals.Character.ZANGIEF:
                                        Data.CurrentState = GameplayData.State.ANNOUNCE_ZANGIEF_WIN;
                                        break;
                                }
                            }
                        }

                        _postKOStateTimer = PostCelebrationTimer;
                    }

                    break;

                // Apply round win, restart after this state
                case GameplayData.State.ANNOUNCE_RYU_WIN:
                case GameplayData.State.ANNOUNCE_CHARLIE_WIN:
                case GameplayData.State.ANNOUNCE_ZANGIEF_WIN:
                case GameplayData.State.ANNOUNCE_DRAW:
                    if (_postKOStateTimer-- <= 0 && !_nextRoundFired)
                    {
                        _animator.SetTrigger(NEXT_ROUND_TRIGGER);
                        _nextRoundFired = true;
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

        _globals.TimeScale += Input.GetAxis("KeyboardHorizontal") * 0.005f;
        if (Input.GetButtonDown("KeyboardEnter"))
        {
            _globals.TimeScale = BaseTimeScale;
        }
    }

    public void FireRoundEnd()
    {
        _postKOStateTimer = KOFreezeTimer;
        _inPostKO = true;
        
        // Double KO if both players are dead
        if (Data.P1.IsDead && Data.P2.IsDead)
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

    public void FireTimeout()
    {
        Data.CurrentState = GameplayData.State.TIMEOUT_PAUSE;
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

    public void PlayHitSFX()
    {
        _audioSource.PlayOneShot(HitSFX);
    }

    public void PlayHitHardSFX()
    {
        _audioSource.PlayOneShot(HitHardSFX);
    }

    public void PlayBlockSFX()
    {
        _audioSource.PlayOneShot(BlockSFX);
    }

    public void PlayThrowFireballSFX()
    {
        _audioSource.PlayOneShot(ThrowFireballSFX);
    }
}
