using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioPlayer : MonoBehaviour
{
    public AudioClip ButtonHighlightSFX;
    public AudioClip ButtonSelectSFX;

    private AudioSource _audioSource;

	void Start ()
    {
        _audioSource = GetComponent<AudioSource>();
	}

    public void PlayButtonHighlightSFX()
    {
        _audioSource.PlayOneShot(ButtonHighlightSFX);
    }

    public void PlayButtonSelectSFX()
    {
        _audioSource.PlayOneShot(ButtonSelectSFX);
    }
}
