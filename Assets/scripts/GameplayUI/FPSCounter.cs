using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour {

    private Text _text;
    private float _timeSinceLastUpdate;
    private int _framesSinceLastUpdate;

	// Use this for initialization
	void Start ()
    {
        _text = GetComponent<Text>();
        _timeSinceLastUpdate = 1;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (_timeSinceLastUpdate <= 0)
        {
            _text.text = string.Format("FPS: {0}, vSyncCount = {1}", _framesSinceLastUpdate, QualitySettings.vSyncCount = 0);
            _timeSinceLastUpdate = 1;
            _framesSinceLastUpdate = 0;
        }

        _timeSinceLastUpdate -= Time.deltaTime;
        _framesSinceLastUpdate++;
    }
}
