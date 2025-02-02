using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkCursor : MonoBehaviourPun
{
    private GameObject cursorInstance;

    void Start()
    {
        cursorInstance = gameObject;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            //Local player updates their cursor
            UpdateLocalCursor();
        }
        else if (cursorInstance != null)
        {
            //Update the position of the remote player's cursor
            cursorInstance.transform.position = transform.position;
        }
    }

    //Update the local player's cursor
    void UpdateLocalCursor()
    {
        Vector3 myMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        myMousePosition.z = 10; // Ensure it's in 2D space

        if (cursorInstance != null)
        {
            transform.position = myMousePosition;
        }
    }

    //Sync the cursor position over the network
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Send the cursor position to other players
            stream.SendNext(transform.position);
        }
        else
        {
            //Receive the cursor position from another player
            transform.position = (Vector3)stream.ReceiveNext();
        }
    }
}
