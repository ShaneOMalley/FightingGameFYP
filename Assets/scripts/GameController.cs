using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public GameplayData Data;

    public float GraphicTimerMax = 90;
    private float _graphicTimer;

    private GameplayData.State[] _initialStates;
    private int _initialStatesIndex;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (_graphicTimer-- <= 0 && _initialStatesIndex < _initialStates.Length - 1)
        {
            _initialStatesIndex++;
            _graphicTimer = GraphicTimerMax;

            Data.CurrentState = _initialStates[_initialStatesIndex];
        }
    }
}
