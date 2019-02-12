using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManagerDebug : MonoBehaviour {

    private Text _text;

	// Use this for initialization
	void Start ()
    {
        _text = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        StringBuilder text = new StringBuilder();
        for (int i = 1; i <= 5; ++i)
        {
            text.AppendFormat(
                "J{0} .. H:{1:f3}, V:{2:f3}, A:{3}, B:{4}, X:{5}, Y{6}, S{7}\n",
                i,
                Input.GetAxis(string.Format("J{0}Horizontal", i)),
                Input.GetAxis(string.Format("J{0}Vertical", i)),
                Input.GetButton(string.Format("J{0}A", i)) ? "1" : "0",
                Input.GetButton(string.Format("J{0}B", i)) ? "1" : "0",
                Input.GetButton(string.Format("J{0}X", i)) ? "1" : "0",
                Input.GetButton(string.Format("J{0}Y", i)) ? "1" : "0",
                Input.GetButton(string.Format("J{0}Start", i)) ? "1" : "0"
                );
        }
        _text.text = text.ToString();
	}
}
