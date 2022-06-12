using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{

    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer player = null;


    void Update()
    {
        if (player is null) // TODO: the definition of dodgy
        {
            try
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

                if (player != null)
                {
                    // make sure the resources display shows initial starting value for resources
                    ClientHandleResourcesUpdated(player.Resources);
                    player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
                }
            }
            catch (Exception)
            {

            }
        }
    }

    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    private void ClientHandleResourcesUpdated(int updatedResources)
    {
        resourcesText.text = $"Resources: {updatedResources}";
    }
}
