using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MiniMap : MonoBehaviour, IPointerDownHandler, IDragHandler
{

    [SerializeField] private RectTransform minimapRect = null; // UI's transform is a RectTRansform
    [SerializeField] private float mapScale = 10f;
    [SerializeField] private float offset = -25f;

    private Transform playerCameraTransform = null;

    private void Update()
    {
        if (playerCameraTransform != null){ return; }
        if (NetworkClient.connection.identity == null) return; // not ready yet
        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().CameraTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }

    private void MoveCamera() // when player clicks minimap, we move camera in word space accordingly 
    {
        var mousePos = Mouse.current.position.ReadValue(); // this is screenspace, we need it in the little minimap square --> minimapRect

        // "convert" cursor position to local screenpoint to check if we click inside minimap
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, mousePos, null, out Vector2 localPoint)) return;

        // we're inside the minimap
        // to make it work irrespective of minimap size(in case we change size at some point), we divide by width and height of image 

        Vector2 lerp = new Vector2((localPoint.x - minimapRect.rect.x) / minimapRect.rect.width, (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        // find out where to move our camera to
        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x), 
            playerCameraTransform.position.y, 
            Mathf.Lerp(-mapScale, mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset); // take offset into acccount
    }

}
