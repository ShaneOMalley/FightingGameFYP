using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayData", menuName = "ScriptableObjects/GameplayData")]
public class GameplayData : ScriptableObject
{
    public int MaxFrames = 5940; // 60 frames * 99 seconds
    public int FramesLeft { get; set; }

    public PlayerController P1;
    public PlayerController P2;

    public int MaxRounds = 2;
    private int _p1Rounds = 0;
    private int _p2Rounds = 0;
    public int P1Rounds
    {
        get { return _p1Rounds; }
        set { _p1Rounds = value; }
    }
    public int P2Rounds
    {
        get { return _p2Rounds; }
        set { _p2Rounds = value; }
    }

    public enum State {
        ANNOUNCE_ROUND_1, ANNOUNCE_ROUND_2, ANNOUNCE_ROUND_3, ANNOUNCE_ROUND_4,
        ANNOUNCE_ROUND_5, ANNOUNCE_ROUND_6, ANNOUNCE_ROUND_FINAL, ANNOUNCE_FIGHT,
        ANNOUNCE_KO, ANNOUNCE_DOUBLE_KO, ANNOUNCE_DRAW, ANNOUNCE_RYU_WIN,
        ANNOUNCE_CHARLIE_WIN, ANNOUNCE_ZANGIEF_WIN, POST_ANNOUNCE_WIN,
        GAMEPLAY, KO_FREEZE, KO_POST_FREEZE, CELEBRATION, POST_CELEBRATION,
        TIMEOUT_PAUSE, TIMEOUT_POST_PAUSE,
    }
    public State CurrentState;

    public void ResetTime()
    {
        FramesLeft = MaxFrames;
    }

    public void ResetRounds()
    {
        P1Rounds = 0;
        P2Rounds = 0;
    }

    public void ResetAll()
    {
        ResetTime();
        ResetRounds();
    }
}
