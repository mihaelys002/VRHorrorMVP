using DG.Tweening.Core.Easing;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System;

public class CameraFeedController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentCameraLabel;
    [SerializeField] private CameraEvent cameraEvent;



    private void OnEnable()
    {
        cameraEvent.OnCameraViewed += OnCameraViewed;
    }

    private void OnCameraViewed(SecurityCameraData camera)
    {
        // change titles and text;
        SwitchToCamera(camera);
    }

    private void OnDisable()
    {
        cameraEvent.OnCameraViewed -= OnCameraViewed;
    }



    public bool SwitchToCamera(SecurityCameraData camera)
    {
        // Update UI
        currentCameraLabel.text = $"CAMERA: {camera.Id}";
        return true;
    }
}