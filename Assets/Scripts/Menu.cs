using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public static string desiredPlayerScene;

    public GameObject p2;
    public GameObject p3;
    public GameObject p4;

    public GameObject kj3_p1;
    public GameObject kj3_p2;
    public GameObject kj3_p3;
    public GameObject kj3_s;

    public GameObject j4_p2;
    public GameObject j4_p3;
    public GameObject j4_p4;

    public GameObject kj3;
    public GameObject j4;

    public GameObject instructionQuad;

    public GameObject ticker;
    public GameObject tickerPlayerTgt;
    public GameObject tickerCtrlTgt;
    public GameObject tickerHTPTgt;
    public GameObject tickerQuitTgt;

    public GameObject[] TransferCameraPositions;

    public AudioSource menuElementChangeSound;

    public float cameraLerpTime = 1.5f;

    public float inputTime = .25f;

    private int selectOptions = 0;
    private bool ctrlType;

    private int playerCount = 0;

    private float timeSinceInput = 0;

    private bool instructionsShown;

    private bool playing = false;

    void Start ()
    {
        SceneManager.sceneLoaded += LoadedScene;
        SceneManager.LoadScene("Dohyo", LoadSceneMode.Additive);
	}

    void LoadedScene(Scene s, LoadSceneMode l)
    {
        SceneManager.SetActiveScene(s);
        Camera.main.transform.position = TransferCameraPositions[0].transform.position;
        Camera.main.transform.forward = TransferCameraPositions[0].transform.forward;
        SceneManager.sceneLoaded -= LoadedScene;
    }

    void Update ()
    {
        if(playing)
        {
            return;
        }

        p2.SetActive(playerCount == 0);
        p3.SetActive(playerCount == 1);
        p4.SetActive(playerCount == 2);

        kj3.SetActive(!ctrlType);
        j4.SetActive(ctrlType);

        kj3_p1.SetActive(playerCount == 0);
        kj3_p2.SetActive(playerCount == 1);
        kj3_p3.SetActive(playerCount == 2);
        kj3_s.SetActive(playerCount > 0);

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

        instructionQuad.SetActive(instructionsShown);

        if(Time.time - timeSinceInput > inputTime)
        {
            if(instructionsShown)
            {
                if(Input.GetAxis("menu_select") > 0)
                {
                    menuElementChangeSound.Play();
                    timeSinceInput = Time.time;
                    instructionsShown = false;
                }
                return;
            }

            if(Input.GetAxis("menu_select") > 0)
            {
                timeSinceInput = Time.time;
                if (selectOptions == 0 || selectOptions == 1)
                {
                    Play();
                }
                else if (selectOptions == 2)
                {
                    menuElementChangeSound.Play();
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
                menuElementChangeSound.Play();
                if (selectOptions > 3)
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
                menuElementChangeSound.Play();
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
                else if (selectOptions == 1)
                {
                    ctrlType = !ctrlType;
                }
            }
        }
    }

    void Play()
    {
        playing = true;
        int p = playerCount + 2;

        desiredPlayerScene = (ctrlType ? "J4_" : "KJ3_") + p + "P";

        FindObjectOfType<GameAudio>().PlayGameStartSound();

        SceneManager.LoadScene(desiredPlayerScene, LoadSceneMode.Additive);

        StartCoroutine(LerpPlayCamera());
    }

    void HowToPlay()
    {
        instructionsShown = true;
    }

    void Quit()
    {
        Application.Quit();
    }

    IEnumerator LerpPlayCamera()
    {
        float startTime;

        for (int i = 1; i < TransferCameraPositions.Length; i++)
        {
            startTime = Time.time;

            while (Time.time - startTime < cameraLerpTime)
            {
                float time = (Time.time - startTime) / cameraLerpTime;
                Camera.main.transform.position = Vector3.Lerp(TransferCameraPositions[i - 1].transform.position, TransferCameraPositions[i].transform.position, time);
                Camera.main.transform.forward = Vector3.Slerp(TransferCameraPositions[i - 1].transform.forward, TransferCameraPositions[i].transform.forward, time);
                yield return null;
            }
        }
        
        yield return null;
        SceneManager.UnloadSceneAsync("MenuStuff");
    }
}
