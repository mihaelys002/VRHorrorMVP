using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CameraMapObject : InteractiveMapObject
{
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Material selectedMaterial;
    private Image cameraIcon;

    private SecurityCameraData cameraData => data as SecurityCameraData;



    public override void Initialize(BaseObjectData data)
    {
        base.Initialize(data);
        cameraIcon = GetComponent<Image>();
        UpdateVisualState();
    }


    protected override string GetCommand()
    {
        return $"camera {ObjectId}";
    }

    public override bool TryExecuteCommand(string command, string[] args, int accessLevel)
    {
        if (!base.TryExecuteCommand(command, args, accessLevel))
        {
            return false;
        }

        switch (command)
        {
            case "camera":
                ViewCamera();
                return true;
            default:
                return false;
        }
    }

    private void ViewCamera()
    {
        (data as SecurityCameraData).SetActive(true);
    }

    private void UpdateVisualState()
    {
        if (cameraIcon != null)
        {
            if (cameraData.IsViewed)
            {
                cameraIcon.material = selectedMaterial;
            }
            else
            {
                cameraIcon.material = inactiveMaterial;
            }
        }
    }

    protected override void OnStateChanged(BaseObjectData data)
    {
        UpdateVisualState();
    }
}