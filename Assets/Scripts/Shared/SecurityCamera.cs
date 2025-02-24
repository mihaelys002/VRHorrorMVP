using System;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{

    private SecurityCameraData cameraData;
    private Camera cameraComponent;
    private const int frameRate = 15;
    private const float frameInterval = 1f / frameRate;
    private float lastUpdate = 0;

    public void Awake()
    {
        cameraData = GetComponent<SecurityCameraData>();
        cameraComponent = GetComponentInChildren<Camera>();
        //Only Render manually
        cameraComponent.enabled = false;
    }

    private void Start()
    {
        cameraData.StateChanged += OnCameraStateChanged;
    }

    private void OnCameraStateChanged(BaseObjectData data)
    {
        if (cameraData.IsViewed)
        {
            //enable red light
        }
        else
        {
            //disable red light
        }
    }

    private void Update()
    {
        if (cameraData.IsViewed)
        {
            if (Time.time - lastUpdate > frameInterval)
            {
                lastUpdate = Time.time;
                //capture frame
                cameraComponent.Render();
            }
        }
    }

}