using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    private Text _text;
    private float _timeSinceLastUpdate;
    private int _framesSinceLastUpdate;

    private const string GLOBALS_NAME = "Globals";
    private Globals _globals;

	// Use this for initialization
	void Start ()
    {
        _text = GetComponent<Text>();
        _timeSinceLastUpdate = 1;
        _globals = Resources.Load<Globals>(GLOBALS_NAME);
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_timeSinceLastUpdate <= 0)
        {
            _text.text = string.Format("TimeScale: {0}, FPS: {1}, vSyncCount = {2}", _globals.TimeScale, _framesSinceLastUpdate, QualitySettings.vSyncCount = 0);
            _timeSinceLastUpdate = 1;
            _framesSinceLastUpdate = 0;
        }

        _timeSinceLastUpdate -= Time.deltaTime;
        _framesSinceLastUpdate++;
    }
}
