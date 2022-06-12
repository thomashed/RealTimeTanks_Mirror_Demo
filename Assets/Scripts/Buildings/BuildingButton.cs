using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    private Renderer buildingRendererInstance;
    private BoxCollider buildingCollider;

    void Start()
    {
        mainCamera = Camera.main;
        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    void Update()
    {
        if (player is null) 
        {
            try
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }
            catch (Exception)
            {

            }
        }

        if (buildingPreviewInstance == null) return;
        UpdateBuildingPreview();

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (player.Resources < building.Price) return;

        buildingPreviewInstance = Instantiate(building.BuildingPreview);
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false); // we hide preview by default, will be visible when in valid location
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            // valid location, thus place building
            player.CmdTryPlaceBuilding(building.Id, hit.point);
        }

        Destroy(buildingPreviewInstance); // we've released, so we can get rid of preview
    }

    // while player is dragging preview intsance before placing 
    private void UpdateBuildingPreview() 
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return; // if it's not hitting anything
        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        // set color on Renderer based on whether we can place it or not
        var color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);

    }

}
