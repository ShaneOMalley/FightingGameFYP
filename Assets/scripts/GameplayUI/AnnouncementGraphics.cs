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
    public Sprite RoundFinalSprite;
    public Sprite FightSprite;
    public Sprite KOSprite;
    public Sprite DoubleKOSprite;

    public RectTransform Graphic1;
    public RectTransform Graphic2;

    public float TravelX = 500;
    public float TravelSpeed = 15;

    private float _posX;
    private Image _graphic1Img;
    private Image _graphic2Img;
    private GameplayData.State _prevState;

    private Dictionary<GameplayData.State, Sprite> spriteMap;
    
    void Start ()
    {
        Graphic1.localPosition = new Vector3(-TravelX, Graphic1.localPosition.y, Graphic1.localPosition.z);
        Graphic2.localPosition = new Vector3(TravelX, Graphic1.localPosition.y, Graphic1.localPosition.z);
        _posX = TravelX;

        spriteMap = new Dictionary<GameplayData.State, Sprite>{
            { GameplayData.State.ANNOUNCE_ROUND_1, Round1Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_2, Round2Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_3, Round3Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_4, Round4Sprite },
            { GameplayData.State.ANNOUNCE_ROUND_FINAL, RoundFinalSprite },
            { GameplayData.State.ANNOUNCE_FIGHT, FightSprite },
            { GameplayData.State.ANNOUNCE_KO, KOSprite },
            { GameplayData.State.ANNOUNCE_DOUBLE_KO, DoubleKOSprite },
        };

        _graphic1Img = Graphic1.GetComponent<Image>();
        _graphic2Img = Graphic2.GetComponent<Image>();

        SetSprite(spriteMap[Data.CurrentState]);
        _prevState = Data.CurrentState;
    }
	
	void Update ()
    {
        if (Data.CurrentState != _prevState)
        {
            Debug.Log("firing with " + Data.CurrentState);
            FireAnnouncement(Data.CurrentState);
        }

        if (Data.CurrentState != GameplayData.State.GAMEPLAY)
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

        if (spriteMap.ContainsKey(state))
        {
            SetSprite(spriteMap[state]);
        }
    }

    private void SetSprite(Sprite sprite)
    {
        _graphic1Img.sprite = sprite;
        _graphic1Img.SetNativeSize();

        _graphic2Img.sprite = sprite;
        _graphic2Img.SetNativeSize();
    }
}
