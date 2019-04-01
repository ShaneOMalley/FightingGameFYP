using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundCounters : MonoBehaviour {
    
    public GameplayData Data;

    [HideInInspector]
    public PlayerController Player;

    private const string COUNT_ROUND = "count_round";
    
    void Start ()
    {
        // Deactivate extra round counters
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i + 1 > Data.MaxRounds)
                transform.GetChild(i).gameObject.SetActive(false);
        }
	}
	
	void Update ()
    {
        bool isP1 = Player.PlayerNum == Globals.Player.P1;
        int roundsWon = isP1 ? Data.P1Rounds : Data.P2Rounds;

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponentInChildren<Animator>().SetBool(COUNT_ROUND, i + 1 <= roundsWon);
        }
	}
}
