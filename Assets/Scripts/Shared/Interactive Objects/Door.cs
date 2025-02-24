using UnityEngine;

public class Door : MonoBehaviour
{
    private DoorData doorData;
    private Animator animator;

    private void Awake()
    {
        doorData = GetComponent<DoorData>();
        animator = GetComponent<Animator>();
        doorData.StateChanged += OnDoorStateChanged;
    }


    private void Start()
    {
        OnDoorStateChanged(doorData); // Initialize door state
    }


    public void UpdateState()
    {
        animator.SetBool("IsOpen", doorData.IsOpen);
    }

    // When the door state changes through game events, sync with map
    public void OnDoorStateChanged(BaseObjectData objectData)
    {
            UpdateState();        
    }
}