using UnityEngine;

public class SetTargetFrameRate : MonoBehaviour
{
    public int TargetFrameRate = 60;

    private void Awake()
    {
        Screen.SetResolution(300, 200, false);
        QualitySettings.vSyncCount = 0;
    }

    private void Update()
    {
        if (TargetFrameRate != Application.targetFrameRate)
            Application.targetFrameRate = TargetFrameRate;
    }
}