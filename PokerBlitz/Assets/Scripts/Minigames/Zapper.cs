using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Zapper : MonoBehaviour
{
    Hashtable playerProperties = new Hashtable();
    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Hashtable customProps = PhotonNetwork.LocalPlayer.CustomProperties;

        // Display custom properties in the console
        foreach (var key in customProps.Keys)
        {
            Debug.Log("Key: " + key + " | Value: " + customProps[key]);
        }
        */
    }

    public void OrderForTesting()
    {
        for (int i = 1; i < 4; i++)
        {
            if (view.IsMine && view.Owner.ActorNumber == i)
            {
                playerProperties["Order"] = i;
            }
        }
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }
}
