using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 movementInput;

    private void Update()
    {
        transform.position += (Vector3)movementInput * moveSpeed * Time.deltaTime;
    }

    public void OnMove(InputValue value)
    {
        Debug.Log("OnMove in PlayerController");
        movementInput = value.Get<Vector2>();
    }

    public void OnMove()
    {
        Debug.LogWarning("On Move");
    }
}
