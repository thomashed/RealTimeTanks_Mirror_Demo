using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : NetworkBehaviour
{
    // get reference to the cirtualCameraTransform
    // get screenLimits X and Y
    // define speed float

    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float screenBorderThickness = 5f; // basically margin from screen border to register when camera should move
    [SerializeField] private Vector2 screenXlimits = Vector2.zero;
    [SerializeField] private Vector2 screenZlimits = Vector2.zero;

    private Vector2 previousInput;
    private Controls controls;

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true); // playerCamera is inactive by default, so we activate only this player's camera 
        controls = new Controls();

        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || !Application.isFocused) return; // we shouldn't try to move other people's cameras. Stop Camera logic if window isn't in focus
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        var pos = playerCameraTransform.position; // current position

        if (previousInput == Vector2.zero) // if no keyboard input, we listen for cursor hitting screenLimits
        {
            Vector3 cursorMovement = Vector3.zero;
            Vector2 cursorPosition = Mouse.current.position.ReadValue();
            if (cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z += 1;
            }
            else if (cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }
            if (cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x += 1;
            }
            else if (cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else // we have keyboard input
        {
            pos += new Vector3(previousInput.x, 0f, previousInput.y) * speed * Time.deltaTime;
        }

        pos.x = Mathf.Clamp(pos.x, screenXlimits.x, screenXlimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZlimits.x, screenZlimits.y);

        playerCameraTransform.position = pos;
    }

    private void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }


}
