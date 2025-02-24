using Sirenix.OdinInspector;
using UnityEngine;


[System.Serializable]
public class SecurityCameraData : BaseObjectData
{

    [SerializeField] private CameraEvent cameraEvent;
    public bool IsViewed;
    [Button]
    public void SetActive(bool isViewed)
    {
        if (IsViewed != isViewed)
        {
            IsViewed = isViewed;
            ThrowChangeStateEvent();
            cameraEvent.OnCameraViewed(this);
        }
    }

    private void OnEnable()
    {
        cameraEvent.OnCameraViewed += OnCameraViewed;
    }

    private void OnCameraViewed(SecurityCameraData arg0)
    {
        if (arg0 != this)
            SetActive(false);
    }

    private void OnDisable()
    {
        cameraEvent.OnCameraViewed -= OnCameraViewed;
    }
}

