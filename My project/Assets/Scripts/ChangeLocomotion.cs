using System;
using System.Collections;
using TMPro;
using Unity.AppUI.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Canvas = UnityEngine.Canvas;
using Text = UnityEngine.UI.Text;

public class ChangeLocomotion : MonoBehaviour
{
    [SerializeField] XRInputButtonReader m_MenuInput = new XRInputButtonReader("Menu");
    
    public XRInputButtonReader jumpInput
    {
        get => m_MenuInput;
        set => XRInputReaderUtility.SetInputProperty(ref m_MenuInput, value, this);
    }

    [SerializeField] private Scene parkScene;
    private ControllerInputActionManager LeftController, RightController;
    private GameObject handMenu, restartButton, quitButton, muteButton, parkButton;
    
    
    private void Start()
    {
        LeftController = GameObject.Find("Left Controller").GetComponent<ControllerInputActionManager>();
        RightController = GameObject.Find("Right Controller").GetComponent<ControllerInputActionManager>();
        
        quitButton = GameObject.Find("Quit Button");
        restartButton = GameObject.Find("Reset Button");
        muteButton = GameObject.Find("Mute Button");
        parkButton = GameObject.Find("Park Button");
        
        quitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Quit";
        restartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Restart";
        muteButton.GetComponentInChildren<TextMeshProUGUI>().text = "Mute Music";
        parkButton.GetComponentInChildren<TextMeshProUGUI>().text = "Return to Park";
        
        handMenu = gameObject.GetComponentInChildren<Canvas>().gameObject;
        handMenu.SetActive(false);
    }

    private void Update()
    {
        if (m_MenuInput.ReadIsPerformed())
        {
            handMenu.SetActive(!handMenu.activeSelf);
        }
    }

    public void Mute()
    {
        Debug.Log("Mute Function");
    }
    
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Park()
    {
        SceneManager.LoadScene(parkScene.buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
    
    // public void ChangeTurn()
    // {
    //     LeftController.smoothTurnEnabled = !LeftController.smoothTurnEnabled;
    //     RightController.smoothTurnEnabled = !RightController.smoothTurnEnabled;
    //     
    //     if (LeftController.smoothTurnEnabled == false)
    //     {
    //         camButton.GetComponentInChildren<TextMeshProUGUI>().text = "Snap Turn";
    //     }
    //     else
    //     {
    //         camButton.GetComponentInChildren<TextMeshProUGUI>().text = "Smooth Turn";
    //     }
    // }
    
    // public void ChangeMovement()
    // {
    //     LeftController.smoothMotionEnabled = !LeftController.smoothMotionEnabled;
    //     RightController.smoothMotionEnabled = !RightController.smoothMotionEnabled;
    //     
    //     if (LeftController.smoothMotionEnabled)
    //     {
    //         movButton.GetComponentInChildren<TextMeshProUGUI>().text = "Smooth Movement";
    //     }
    //     else
    //     {
    //         movButton.GetComponentInChildren<TextMeshProUGUI>().text = "Teleport";
    //     }
    // }
}
