using Codice.Client.Common;
using UnityEngine;

public class DoorMapObject : InteractiveMapObject
{
    private Animator animator;
    private DoorData doorData => data as DoorData;

    protected override string GetCommand()
    {
        return $"{(doorData.IsOpen ? "close" : "open")} {ObjectId}";
    }
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public override bool TryExecuteCommand(string command, string[] args, int accessLevel)
    {
        if (!base.TryExecuteCommand(command, args, accessLevel))
            return false;


        switch (command)
        {
            case "open":
                doorData.SetOpen(true);
                return true;
            case "close":
                doorData.SetOpen(false);
                return true;
            default:
                return false;
        }
    }

    private void UpdateVisualState()
    {
        // Update the visual representation based on isOpen state
        animator.SetBool("IsOpen", doorData.IsOpen);
    }

    protected override void OnStateChanged(BaseObjectData data)
    {
        UpdateVisualState();
    }
}