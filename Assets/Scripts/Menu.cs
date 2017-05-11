using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct MenuPage
{
    public MenuOption[] options;
    public GameObject cameraPosition;
}

[System.Serializable]
public struct MenuOption
{
    public GameObject scaleTarget;
    public MeshRenderer meshTarget;
}

public class Menu : MonoBehaviour
{
    public static string desiredGameScene = "Dohyo";
    public static string desiredPlayerScene = "Dohyo";

    [SerializeField]
    public MenuPage[] pages;
}
