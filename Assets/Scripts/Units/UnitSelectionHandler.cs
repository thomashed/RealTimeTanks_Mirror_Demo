using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask = new LayerMask();
    [SerializeField] private RectTransform dragBox = null;
    private RTSPlayer player = null;
    private Camera mainCamera = null;
    private Vector2 startMousePosition = Vector2.zero;
    public List<Unit> SelectedUnits { get; private set; } = new List<Unit>();

    private void Start()
    {
        mainCamera = Camera.main;
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver; // we wanna disable client to select etc
    }

    private void OnDestroy()
    {
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {
        if (player is null) // TODO: the definition of dodgy
        {
            try
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }
            catch (Exception)
            {

            }
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            UpdateSelectionArea(); // keep updating the dragBox
        }
    }

    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }

    private void ClearSelectionArea()
    {
        dragBox.gameObject.SetActive(false);

        if (dragBox.sizeDelta.magnitude == 0) // check if it was only a single click
        {
            var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            if (!hit.transform.TryGetComponent<Unit>(out Unit unit)) return ;
            if (!unit.hasAuthority) return;
            SelectedUnits.Add(unit);
            foreach (var selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        // get the min and max value for the points of our dragBox
        Vector2 min = dragBox.anchoredPosition - (dragBox.sizeDelta / 2);
        Vector2 max = dragBox.anchoredPosition + (dragBox.sizeDelta / 2);

        foreach (var unit in player.GetMyUnits())
        {
            if (SelectedUnits.Contains(unit)) continue; // don't call select on the unit if it's already selected
            // the tank is in world space, but we need it in screen space
            Vector3 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
            if (screenPos.x > min.x && screenPos.y < max.x && screenPos.y > min.y && screenPos.y < max.y) // if we're within the dragbox
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    private void UpdateSelectionArea()
    {
        var currentMousePosition = Mouse.current.position.ReadValue();
        float width = currentMousePosition.x - startMousePosition.x;
        float height = currentMousePosition.y - startMousePosition.y;

        dragBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        dragBox.anchoredPosition = startMousePosition + new Vector2(width / 2, height / 2);
    }

    private void StartSelectionArea()
    {
        dragBox.gameObject.SetActive(true);

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (var unit in SelectedUnits)
            {
                unit.Deselect();
            }
            SelectedUnits.Clear();
        }

        startMousePosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    private void ClientHandleGameOver(string playerName)
    {
        enabled = false;
    }
}
