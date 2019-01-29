using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PGBackgroundScroll : MonoBehaviour {

    public float ScrollSpeed = 0.5f;
    public Renderer Rend;

    public Image Img;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        float offset = Time.time * ScrollSpeed;
        //Img.material.mainTextureOffset = new Vector2(offset, 0);
        Img.material.SetTextureOffset("_MainTex", new Vector2(offset, 0));

        Debug.Log(offset);
        //this.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
	}
}
