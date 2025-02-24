using Mirror;
using System;
using UnityEngine;

[System.Serializable]
public abstract class BaseObjectData: MonoBehaviour
{
    public string Id;
    public int RequiredAccessLevel;
    public event Action<BaseObjectData> StateChanged;
    protected void ThrowChangeStateEvent()
    {
        StateChanged?.Invoke(this);
    }
}

[System.Serializable]
public class DoorData : BaseObjectData
{
    public bool IsOpen;
    public void SetOpen(bool isOpened)
    {
        if (IsOpen != isOpened)
        {
            IsOpen = isOpened;
            ThrowChangeStateEvent();
        }
    }
}

[System.Serializable]
public class TurretData : BaseObjectData
{
    public bool isActive;
    public float range;
}

// Similar classes for other object types