using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinMenu : MonoBehaviour
{
    public GameObject P1;
    public GameObject P2;
    public GameObject P3;
    public GameObject P4;

    public GameObject menu;

    public GameObject ticker;
    public GameObject playAgainTgtPos;
    public GameObject backToMenuTgtPos;

    public GameObject GameCameraPosition;

    public float inputTime = .25f;

    public int winningPlayer;

    private float timeSinceInput = 0;

    private bool selectedOption;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        menu.SetActive(winningPlayer != 0);

        P1.SetActive(winningPlayer == 1);
        P2.SetActive(winningPlayer == 2);
        P3.SetActive(winningPlayer == 3);
        P4.SetActive(winningPlayer == 4);

        ticker.transform.position = selectedOption ? backToMenuTgtPos.transform.position : playAgainTgtPos.transform.position;

        if(winningPlayer != 0 && Time.time - timeSinceInput > inputTime)
        {
            if (Input.GetAxis("menu_select") > 0)
            {
                timeSinceInput = Time.time;
                if (selectedOption)
                {
                    BackToMenu();
                }
                else
                {
                    PlayAgain();
                }
            }
            else if(Mathf.Abs(Input.GetAxis("menu_move_vert")) > 0)
            {
                timeSinceInput = Time.time;
                selectedOption = !selectedOption;
            }
        }
    }

    void PlayAgain()
    {
        SceneManager.sceneUnloaded += SceneUnloaded;
        SceneManager.UnloadSceneAsync(Menu.desiredPlayerScene);
    }

    void SceneUnloaded(Scene s)
    {
        winningPlayer = 0;
        SceneManager.LoadScene(Menu.desiredPlayerScene, LoadSceneMode.Additive);
        SceneManager.sceneUnloaded -= SceneUnloaded;
        Camera.main.transform.position = GameCameraPosition.transform.position;
        Camera.main.transform.forward = GameCameraPosition.transform.forward;
    }

    void BackToMenu()
    {
        SceneManager.LoadScene("MenuStuff");
    }
}
