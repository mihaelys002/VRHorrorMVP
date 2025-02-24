using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractiveMapObject : MonoBehaviour
{
    protected Button button;
    protected CommandProcessor commandProcessor;
    protected BaseObjectData data;

    public int RequiredAccessLevel => data.RequiredAccessLevel;

    public string ObjectId => data.Id;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public virtual void  Initialize(BaseObjectData data)
    {
        this.data = data;
        data.StateChanged += OnStateChanged;
    }

    protected abstract void OnStateChanged(BaseObjectData data);

    protected virtual void OnClick()
    {
        string command = GetCommand();
        commandProcessor.ExecuteCommandFromMapObject(command);
    }

    protected abstract string GetCommand();

    public virtual bool TryExecuteCommand(string command, string[] args, int accessLevel)
    {
        return accessLevel >= RequiredAccessLevel && args!= null && args.Length>1 && args[1] == ObjectId;
    }
}