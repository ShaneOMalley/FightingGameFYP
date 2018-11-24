using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameplayData", menuName = "ScriptableObjects/GameplayData")]
public class GameplayData : ScriptableObject
{
    public int maxFrames = 5940; // 60 frames * 99 seconds
    public int framesLeft;

    public enum State {
        ANNOUNCE_ROUND_1, ANNOUNCE_ROUND_2, ANNOUNCE_ROUND_3, ANNOUNCE_ROUND_4,
        ANNOUNCE_ROUND_FINAL, ANNOUNCE_FIGHT, ANNOUNCE_KO, ANNOUNCE_DOUBLE_KO, GAMEPLAY
    }
    public State CurrentState;

    public void ResetTime()
    {
        framesLeft = maxFrames;
    }
}
