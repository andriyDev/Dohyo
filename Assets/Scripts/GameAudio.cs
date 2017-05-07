using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudio : MonoBehaviour
{
    [Header("Values")]
    public float birdFadeSpeed = 3;

    [Header("References")]
    public AudioSource birds;
    public AudioSource startGameSound;
    public AudioSource backgroundMusic;
	
	void Update ()
    {
        bool playBirds = false;

        Player[] p = FindObjectsOfType<Player>();
        for(int i = 0; i < p.Length; i++)
        {
            if(p[i].GetState() == PlayerState.AfterCharged)
            {
                playBirds = true;
                break;
            }
        }

        birds.volume = Mathf.MoveTowards(birds.volume, playBirds ? 1 : 0, Time.deltaTime * birdFadeSpeed);
    }

    public void PlayGameStartSound()
    {
        startGameSound.Play();
    }
}
