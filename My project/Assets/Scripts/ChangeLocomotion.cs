using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ChangeLocomotion : MonoBehaviour
{
    private ControllerInputActionManager LeftController, RightController;

    private void Start()
    {
        LeftController = GameObject.Find("Left Controller").GetComponent<ControllerInputActionManager>();
        RightController = GameObject.Find("Right Controller").GetComponent<ControllerInputActionManager>();
    }

    void ChangeTurn()
    {
        LeftController.smoothTurnEnabled = !LeftController.smoothTurnEnabled;
        RightController.smoothTurnEnabled = !RightController.smoothTurnEnabled;
    }

    void ChangeMovement()
    {
        LeftController.smoothMotionEnabled = !LeftController.smoothMotionEnabled;
        RightController.smoothMotionEnabled = !RightController.smoothMotionEnabled;
    }
}
