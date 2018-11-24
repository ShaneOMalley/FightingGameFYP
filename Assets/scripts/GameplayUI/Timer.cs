using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour {

    public GameplayData GameplayData;
    public Sprite[] Digits;

    public Image LeftDigit;
    public Image  RightDigit;

    // Use this for initialization
    void Start ()
    {
        GameplayData.ResetTime();
	}
	
	// Update is called once per frame
	void Update ()
    {
        int secondsLeft = GameplayData.framesLeft > 0 ? (GameplayData.framesLeft - 1) / 60 + 1 : 0;
            
        secondsLeft = (int)Mathf.Clamp(secondsLeft, 0, 99);
        int ldigit = secondsLeft / 10;
        int rdigit = secondsLeft % 10;

        LeftDigit.sprite = Digits[ldigit];
        RightDigit.sprite = Digits[rdigit];

        if (GameplayData.CurrentState == GameplayData.State.GAMEPLAY)
        {
            GameplayData.framesLeft--;
        }
	}
}
