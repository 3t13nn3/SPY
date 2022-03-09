using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSystemBridge : MonoBehaviour
{
    // Camera
    public GameObject cameraAssociate;

    // Active ou desactive le syst�me
    public void SetCameraSystem(bool value)
    {
        // Comment faire pour mettre le syst�me en pause puis le relancer
        CameraSystem.instance.SetCameraSystem(value);
    }
}
