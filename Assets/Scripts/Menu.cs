using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject two;
    public GameObject three;
    public GameObject four;

    public GameObject ticker;
    public GameObject tickerPlayerTgt;
    public GameObject tickerHTPTgt;
    public GameObject tickerQuitTgt;

    public float inputTime = .25f;

    private int selectOptions = 0;

    private int playerCount = 0;

    private float timeSinceInput = 0;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        two.SetActive(playerCount == 0);
        three.SetActive(playerCount == 1);
        four.SetActive(playerCount == 2);

        ticker.transform.position = selectOptions == 0 ? tickerPlayerTgt.transform.position : selectOptions == 1 ? tickerHTPTgt.transform.position : tickerQuitTgt.transform.position;

        if(Time.time - timeSinceInput > inputTime)
        {
            if(Input.GetAxis("menu_select") > 0)
            {
                timeSinceInput = Time.time;
                if(selectOptions == 0)
                {
                    Play();
                }
                else if (selectOptions == 1)
                {
                    HowToPlay();
                }
                else
                {
                    Quit();
                }
            }
            else if (Mathf.Abs(Input.GetAxis("menu_move_vert")) > 0)
            {
                timeSinceInput = Time.time;
                selectOptions -= (int)Mathf.Sign(Input.GetAxis("menu_move_vert"));
                if(selectOptions > 2)
                {
                    selectOptions = 0;
                }
                else if (selectOptions < 0)
                {
                    selectOptions = 2;
                }
            }
            else if(Mathf.Abs(Input.GetAxis("menu_move_horiz")) > 0)
            {
                timeSinceInput = Time.time;
                if (selectOptions == 0)
                {
                    playerCount += (int)Mathf.Sign(Input.GetAxis("menu_move_horiz"));
                    if (playerCount > 2)
                    {
                        playerCount = 0;
                    }
                    else if (playerCount < 0)
                    {
                        playerCount = 2;
                    }
                }
            }
        }
    }

    void Play()
    {
        playerCount += 2;
    }

    void HowToPlay()
    {

    }

    void Quit()
    {

    }
}
