using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static string desiredPlayerScene;

    public GameObject p2;
    public GameObject p3;
    public GameObject p4;

    public GameObject kj3_p1;
    public GameObject kj3_p2;
    public GameObject kj3_p3;

    public GameObject j4_p2;
    public GameObject j4_p3;
    public GameObject j4_p4;

    public GameObject kj3;
    public GameObject j4;

    public GameObject ticker;
    public GameObject tickerPlayerTgt;
    public GameObject tickerCtrlTgt;
    public GameObject tickerHTPTgt;
    public GameObject tickerQuitTgt;

    public float inputTime = .25f;

    private int selectOptions = 0;
    private bool ctrlType;

    private int playerCount = 0;

    private float timeSinceInput = 0;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        p2.SetActive(playerCount == 0);
        p3.SetActive(playerCount == 1);
        p4.SetActive(playerCount == 2);

        kj3.SetActive(!ctrlType);
        j4.SetActive(ctrlType);

        kj3_p1.SetActive(playerCount == 0);
        kj3_p2.SetActive(playerCount == 1);
        kj3_p3.SetActive(playerCount == 2);

        j4_p2.SetActive(playerCount == 0);
        j4_p3.SetActive(playerCount == 1);
        j4_p4.SetActive(playerCount == 2);

        switch (selectOptions)
        {
            case 0:
                ticker.transform.position = tickerPlayerTgt.transform.position;
                break;
            case 1:
                ticker.transform.position = tickerCtrlTgt.transform.position;
                break;
            case 2:
                ticker.transform.position = tickerHTPTgt.transform.position;
                break;
            case 3:
                ticker.transform.position = tickerQuitTgt.transform.position;
                break;
        }

        if(Time.time - timeSinceInput > inputTime)
        {
            if(Input.GetAxis("menu_select") > 0)
            {
                timeSinceInput = Time.time;
                if(selectOptions == 0 || selectOptions == 1)
                {
                    Play();
                }
                else if (selectOptions == 2)
                {
                    HowToPlay();
                }
                else if (selectOptions == 3)
                {
                    Quit();
                }
            }
            else if (Mathf.Abs(Input.GetAxis("menu_move_vert")) > 0)
            {
                timeSinceInput = Time.time;
                selectOptions -= (int)Mathf.Sign(Input.GetAxis("menu_move_vert"));
                if(selectOptions > 3)
                {
                    selectOptions = 0;
                }
                else if (selectOptions < 0)
                {
                    selectOptions = 3;
                }
            }
            else if(Mathf.Abs(Input.GetAxis("menu_move_horiz")) > 0)
            {
                timeSinceInput = Time.time;
                if (selectOptions == 0)
                {
                    playerCount += (int)Mathf.Sign(Input.GetAxis("menu_move_horiz"));
                    if (playerCount > 3)
                    {
                        playerCount = 0;
                    }
                    else if (playerCount < 0)
                    {
                        playerCount = 3;
                    }
                }
                else if (selectOptions == 1)
                {
                    ctrlType = !ctrlType;
                }
            }
        }
    }

    void Play()
    {
        playerCount += 2;

        desiredPlayerScene = (ctrlType ? "J4_" : "KJ3_") + (playerCount) + "P";
    }

    void HowToPlay()
    {

    }

    void Quit()
    {

    }
}
