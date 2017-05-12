using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TeamUtility.IO;

[System.Serializable]
public struct MenuPage
{
    public GameObject cameraPosition;
    public GameObject defaultSelection;
}

public class Menu : MonoBehaviour
{
    public const int MAX_CONFIG_COUNT = 4;

    public static string desiredGameScene = "Dohyo";
    public static string desiredPlayerScene = "Dohyo";

    public static Menu current;
    
    [SerializeField]
    public MenuPage[] pages;

    // Values
    public float menuTransitionTime = .25f;

    // Scene References
    public Camera targetCamera;
    public EventSystem events;
    public TeamUtility.IO.StandaloneInputModule inputModule;

    // Update References
    public Text Controls_PlayerNumText;
    public Text Controls_M_Up;
    public Text Controls_M_Down;
    public Text Controls_M_Left;
    public Text Controls_M_Right;
    public Text Controls_D_Up;
    public Text Controls_D_Down;
    public Text Controls_D_Left;
    public Text Controls_D_Right;
    public Text Controls_A_Charge;

    private int currentPage;

    private bool canHandleAction = true;

    private int selectedControlConfig;
    private int bindingAction = 0;

    private void Start()
    {
        current = this;
    }

    private void Update()
    {
        Controls_PlayerNumText.text = "Player " + (1 + selectedControlConfig);

        AxisConfiguration bind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Vert");
        if(bind.type == InputType.DigitalAxis)
        {
            Controls_M_Up.text = "Up: " + bind.positive.ToString();
            Controls_M_Down.text = "Down: " + bind.negative.ToString();
        }
        else
        {
            string main = (bind.type == InputType.MouseAxis ? (bind.axis == 0 ? "Mouse X" : "Mouse Y") : (bind.type == InputType.AnalogAxis ? "Joystick" + (bind.joystick + 1) + "Axis" + (bind.axis + 1) : "<undefined>"));
            Controls_M_Up.text = "Up: " + main;
            Controls_M_Down.text = "Down: " + main;
        }
        if (bindingAction == 1) { Controls_M_Up.text = "Up: <binding>"; }
        if (bindingAction == 2) { Controls_M_Down.text = "Down: <binding>"; }
        bind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Horiz");
        if (bind.type == InputType.DigitalAxis)
        {
            Controls_M_Right.text = "Right: " + bind.positive.ToString();
            Controls_M_Left.text = "Left: " + bind.negative.ToString();
        }
        else
        {
            string main = (bind.type == InputType.MouseAxis ? (bind.axis == 0 ? "Mouse X" : "Mouse Y") : (bind.type == InputType.AnalogAxis ? "Joystick" + (bind.joystick + 1) + "Axis" + (bind.axis + 1) : "<undefined>"));
            Controls_M_Right.text = "Right: " + main;
            Controls_M_Left.text = "Left: " + main;
        }
        if (bindingAction == 3) { Controls_M_Left.text = "Left: <binding>"; }
        if (bindingAction == 4) { Controls_M_Right.text = "Right: <binding>"; }
        bind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Vert");
        if (bind.type == InputType.DigitalAxis)
        {
            Controls_D_Up.text = "Up: " + bind.positive.ToString();
            Controls_D_Down.text = "Down: " + bind.negative.ToString();
        }
        else
        {
            string main = (bind.type == InputType.MouseAxis ? (bind.axis == 0 ? "Mouse X" : "Mouse Y") : (bind.type == InputType.AnalogAxis ? "Joystick" + (bind.joystick + 1) + "Axis" + (bind.axis + 1) : "<undefined>"));
            Controls_D_Up.text = "Up: " + main;
            Controls_D_Down.text = "Down: " + main;
        }
        if (bindingAction == 5) { Controls_D_Up.text = "Up: <binding>"; }
        if (bindingAction == 6) { Controls_D_Down.text = "Down: <binding>"; }
        bind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Horiz");
        if (bind.type == InputType.DigitalAxis)
        {
            Controls_D_Right.text = "Right: " + bind.positive.ToString();
            Controls_D_Left.text = "Left: " + bind.negative.ToString();
        }
        else
        {
            string main = (bind.type == InputType.MouseAxis ? (bind.axis == 0 ? "Mouse X" : "Mouse Y") : (bind.type == InputType.AnalogAxis ? "Joystick" + (bind.joystick + 1) + "Axis" + (bind.axis + 1) : "<undefined>"));
            Controls_D_Right.text = "Right: " + main;
            Controls_D_Left.text = "Left: " + main;
        }
        if (bindingAction == 7) { Controls_D_Left.text = "Left: <binding>"; }
        if (bindingAction == 8) { Controls_D_Right.text = "Right: <binding>"; }
        bind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Charge");
        if (bind.type == InputType.DigitalAxis)
        {
            Controls_A_Charge.text = "Charge: " + bind.positive.ToString();
        }
        else
        {
            Controls_A_Charge.text = "Charge: " + (bind.type == InputType.MouseAxis ? (bind.axis == 0 ? "Mouse X" : "Mouse Y") : (bind.type == InputType.AnalogAxis ? "Joystick" + (bind.joystick + 1) + "Axis" + (bind.axis + 1) : "<undefined>"));
        }
        if (bindingAction == 9) { Controls_A_Charge.text = "Charge: <binding>"; }
    }

    public void ChangePage(int page)
    {
        if (canHandleAction)
        {
            if (page < pages.Length)
            {
                currentPage = page;
                events.SetSelectedGameObject(pages[currentPage].defaultSelection);
                StartCoroutine(MenuTransition());
            }
        }
    }

    IEnumerator MenuTransition()
    {
        canHandleAction = false;

        if (currentPage > -1 && currentPage < pages.Length)
        {
            Vector3 startPos = targetCamera.transform.position;
            Vector3 startRot = targetCamera.transform.forward;
            Vector3 endPos = pages[currentPage].cameraPosition.transform.position;
            Vector3 endRot = pages[currentPage].cameraPosition.transform.forward;

            float startTime = Time.time;

            while(Time.time - startTime < menuTransitionTime)
            {
                float a = (Time.time - startTime) / menuTransitionTime;
                targetCamera.transform.position = Vector3.Lerp(startPos, endPos, a);
                targetCamera.transform.forward = Vector3.Slerp(startRot, endRot, a);
                yield return null;
            }

            targetCamera.transform.position = endPos;
            targetCamera.transform.forward = endRot;
        }

        yield return null;
        canHandleAction = true;
    }

    public void IncrementSelectedControlConfig()
    {
        selectedControlConfig++;
        selectedControlConfig %= MAX_CONFIG_COUNT;
    }

    public void BeginBinding(int bind)
    {
        bindingAction = bind;
        ScanSettings s = new ScanSettings();
        s.timeout = 5;
        s.scanFlags = ScanFlags.JoystickAxis | ScanFlags.JoystickButton | ScanFlags.MouseAxis | ScanFlags.Key;
        inputModule.allowInput = false;
        InputManager.StartScan(s, OnInputScanComplete);
    }

    private bool OnInputScanComplete(ScanResult result)
    {
        if (result.scanFlags == ScanFlags.Key || result.scanFlags == ScanFlags.JoystickButton)
        {
            if(result.key == KeyCode.Escape)
            {
                StartCoroutine(PostBind());
                bindingAction = 0;
                return true;
            }

            AxisConfiguration desiredBind;
            switch (bindingAction)
            {
                case 1:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Vert");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.positive = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 2:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Vert");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.negative = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 3:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Horiz");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.negative = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 4:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Horiz");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.positive = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 5:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Vert");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.positive = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 6:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Vert");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.negative = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 7:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Horiz");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.negative = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 8:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Horiz");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.positive = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
                case 9:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Charge");
                    desiredBind.type = InputType.DigitalAxis;
                    desiredBind.positive = result.scanFlags == ScanFlags.JoystickButton ? (KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick" + result.joystick + "Button" + result.key.ToString().Substring(14)) : result.key;
                    break;
            }
        }
        else if (result.scanFlags == ScanFlags.MouseAxis)
        {
            AxisConfiguration desiredBind;
            switch (bindingAction)
            {
                case 1:
                case 2:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Vert");
                    desiredBind.type = InputType.MouseAxis;
                    desiredBind.SetMouseAxis(result.mouseAxis);
                    break;
                case 3:
                case 4:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Horiz");
                    desiredBind.type = InputType.MouseAxis;
                    desiredBind.SetMouseAxis(result.mouseAxis);
                    break;
                case 5:
                case 6:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Vert");
                    desiredBind.type = InputType.MouseAxis;
                    desiredBind.SetMouseAxis(result.mouseAxis);
                    break;
                case 7:
                case 8:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Horiz");
                    desiredBind.type = InputType.MouseAxis;
                    desiredBind.SetMouseAxis(result.mouseAxis);
                    break;
                case 9:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Charge");
                    desiredBind.type = InputType.MouseAxis;
                    desiredBind.SetMouseAxis(result.mouseAxis);
                    break;
            }
        }
        else if (result.scanFlags == ScanFlags.JoystickAxis)
        {
            AxisConfiguration desiredBind;
            switch (bindingAction)
            {
                case 1:
                case 2:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Vert");
                    desiredBind.type = InputType.AnalogAxis;
                    desiredBind.SetAnalogAxis(result.joystick, result.joystickAxis);
                    break;
                case 3:
                case 4:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Move_Horiz");
                    desiredBind.type = InputType.AnalogAxis;
                    desiredBind.SetAnalogAxis(result.joystick, result.joystickAxis);
                    break;
                case 5:
                case 6:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Vert");
                    desiredBind.type = InputType.AnalogAxis;
                    desiredBind.SetAnalogAxis(result.joystick, result.joystickAxis);
                    break;
                case 7:
                case 8:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Dodge_Horiz");
                    desiredBind.type = InputType.AnalogAxis;
                    desiredBind.SetAnalogAxis(result.joystick, result.joystickAxis);
                    break;
                case 9:
                    desiredBind = InputManager.GetAxisConfiguration("Player" + (selectedControlConfig + 1), "Charge");
                    desiredBind.type = InputType.AnalogAxis;
                    desiredBind.SetAnalogAxis(result.joystick, result.joystickAxis);
                    break;
            }
        }
        else
        {
            return false;
        }
        bindingAction = 0;
        StartCoroutine(PostBind());
        return true;
    }

    private IEnumerator PostBind()
    {
        yield return new WaitForSecondsRealtime(.5f);
        inputModule.allowInput = true;
    }

    public void QuitGame()
    {
        if (canHandleAction)
        {
            Application.Quit();
        }
    }
}
