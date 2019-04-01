using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Globals", menuName = "ScriptableObjects/Globals")]
public class Globals : ScriptableObject
{
    public enum Character { RYU, ZANGIEF, CHARLIE }
    public enum Player { P1, P2, NEITHER }

    public int TargetFrameRate = 60;
    public bool DrawHitboxes = true;
    public bool ShowRecoveryColor = true;
    public bool DebugPreviewAnimationMovements = false;
    public float TimeScale = 1;
    public int HitFreezeLength = 4;
    public int InputBufferFrames = 7;

    [Header("Volume")]
    public float MasterVolume = 1;

    [Header("Character Selection")]
    public Character Player1Character = Character.RYU;
    public Character Player2Character = Character.RYU;
    public int Player1Controller = -1;
    public int BackupP1Controller = -1;
    public int Player2Controller = -1;

    public Player PlayerWinning = Player.NEITHER;
}
