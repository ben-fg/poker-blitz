using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkCursor : MonoBehaviourPun
{
    public GameObject cursorPrefab;
    private GameObject cursorInstance;
    [SerializeField] private Texture2D customCursor;

    void Start()
    {
        Vector2 hotspot;
        hotspot = new Vector2(customCursor.width / 2, customCursor.height / 2);
        Cursor.SetCursor(customCursor, hotspot, CursorMode.Auto);

        //Only instantiate the cursor if it's not the local player
        if (!photonView.IsMine)
        {
            cursorInstance = Instantiate(cursorPrefab);
        }
    }

    /*
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
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure it's in 2D space

        transform.position = mousePosition;
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
    */
}
