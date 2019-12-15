using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnouncementGraphics : MonoBehaviour {

    public GameplayData Data;

    public Sprite Round1Sprite;
    public Sprite Round2Sprite;
    public Sprite Round3Sprite;
    public Sprite Round4Sprite;
    public Sprite Round5Sprite;
    public Sprite Round6Sprite;
    public Sprite RoundFinalSprite;
    public Sprite FightSprite;
    public Sprite KOSprite;
    public Sprite DoubleKOSprite;
    public Sprite TimeOverSprite;
    public Sprite RyuWinSprite;
    public Sprite CharlieWinSprite;
    public Sprite ZangiefWinSprite;
    public Sprite DrawSprite;

    public RectTransform Graphic1;
    public RectTransform Graphic2;

    public float TravelX = 500;
    public float TravelSpeed = 15;

    private float _posX;
    private Image _graphic1Img;
    private Image _graphic2Img;
    private GameplayData.State? _prevState;

    private Dictionary<GameplayData.State, Sprite> _spriteMap;
    
    void Start ()
    {
        Graphic1.localPosition = new Vector3(-TravelX, Graphic1.localPosition.y, Graphic1.localPosition.z);
        Graphic2.localPosition = new Vector3(TravelX, Graphic1.localPosition.y, Graphic1.localPosition.z);
        _posX = TravelX;

        _spriteMap = new Dictionary<GameplayData.State, Sprite>{
            { GameplayData.State.ANNOUNCE_ROUND_1, Round1Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_2, Round2Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_3, Round3Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_4, Round4Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_5, Round5Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_6, Round6Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_FINAL, RoundFinalSprite },
            { GameplayData.State.ANNOUNCE_FIGHT, FightSprite },
            { GameplayData.State.ANNOUNCE_KO, KOSprite },
            { GameplayData.State.ANNOUNCE_DOUBLE_KO, DoubleKOSprite },
            { GameplayData.State.TIMEOUT_PAUSE, TimeOverSprite },
            { GameplayData.State.ANNOUNCE_RYU_WIN, RyuWinSprite },
            { GameplayData.State.ANNOUNCE_CHARLIE_WIN, CharlieWinSprite },
            { GameplayData.State.ANNOUNCE_ZANGIEF_WIN, ZangiefWinSprite },
            { GameplayData.State.ANNOUNCE_DRAW, DrawSprite },
        };

        _graphic1Img = Graphic1.GetComponent<Image>();
        _graphic2Img = Graphic2.GetComponent<Image>();

        SetSprite(Data.CurrentState);
        _prevState = null;
    }
	
	void FixedUpdate ()
    {
        Debug.Log("AnnoucementGraphics FixedUpdate");
        if (Data.CurrentState != _prevState)
        {
            FireAnnouncement(Data.CurrentState);
        }

        if (_spriteMap.ContainsKey(Data.CurrentState))
        {
            _posX -= TravelSpeed;
            _posX = Mathf.Clamp(_posX, 0, TravelX);
        }

        Graphic1.localPosition = new Vector3(-_posX, Graphic1.localPosition.y, Graphic1.localPosition.z);
        Graphic2.localPosition = new Vector3(_posX, Graphic1.localPosition.y, Graphic1.localPosition.z);

        _prevState = Data.CurrentState;
    }
    
    private void FireAnnouncement(GameplayData.State state)
    {
        _posX = TravelX;

        if (_spriteMap.ContainsKey(state))
        {
            SetSprite(state);
        }
    }

    private void SetSprite(GameplayData.State state)
    {
        if (!_spriteMap.ContainsKey(state))
            return;
        
        _graphic1Img.sprite = _spriteMap[state];
        _graphic1Img.SetNativeSize();

        _graphic2Img.sprite = _spriteMap[state];
        _graphic2Img.SetNativeSize();
    }
}
