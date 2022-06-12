using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandHandler : MonoBehaviour
{
    [SerializeField] public UnitSelectionHandler unitSelecctionHandler = null;
    [SerializeField] public LayerMask layerMask = new LayerMask();

    private Camera mainCamera = null;

    void Start()
    {
        mainCamera = Camera.main;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
        //check if we should attack or move
        if (hit.collider.TryGetComponent<Targetable>(out var target))
        {
            
            if (target.hasAuthority) // then it's our unit, and we just wanna move
            {
                TryMove(hit.point);
                return;
            }

            TryTarget(target);
            return;
        }
        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (var unit in unitSelecctionHandler.SelectedUnits)
        {
            unit.Targeter.CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point)
    {
        foreach (var unit in unitSelecctionHandler.SelectedUnits)
        {
            unit.UnitMovement.CmdValidateAndMovePlayer(point);
        }
    }

    private void ClientHandleGameOver(string playerName)
    {
        enabled = false;
    }
}
