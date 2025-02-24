using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/CameraEvent")]
public class CameraEvent:ScriptableObject
{
    public UnityAction<SecurityCameraData> OnCameraViewed;

    public void Raise(SecurityCameraData camera)
    {
        OnCameraViewed?.Invoke(camera);
    }
}
