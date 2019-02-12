using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Globals", menuName = "ScriptableObjects/Globals")]
public class Globals : ScriptableObject
{
    public int TargetFrameRate = 60;
    public bool DrawHitboxes = true;
    public bool ShowRecoveryColor = true;
    public bool DebugPreviewAnimationMovements = false;
    public float TimeScale = 1;
    public int HitFreezeLength = 4;
    public int InputBufferFrames = 7;

    [Header("Volume")]
    public float MasterVolume = 1;
}
