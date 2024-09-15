using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerSetup : MonoBehaviour
{
    public Color[] playerColours = { Color.red, Color.blue, Color.green, Color.yellow };
    private SpriteRenderer playerRenderer;
    [SerializeField] private TextMeshProUGUI username;
    PhotonView view;
    Hashtable playerProperties;

    void Start()
    {
        view = GetComponent<PhotonView>();

        if (GetComponent<SpriteRenderer>() != null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }
        else
        {
            playerRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (view.IsMine)
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
            AssignPlayerColour();
        }

        view.RPC("SetPlayerUsername", RpcTarget.AllBuffered, PhotonNetwork.NickName);
    }

    void AssignPlayerColour()
    {
        int actorNumber = view.Owner.ActorNumber;
        int colourIndex = (actorNumber - 1) % playerColours.Length;

        //Set custom properties for the local player
        playerProperties = new Hashtable
        {
            { "PlayerColour", colourIndex },
            { "PlayerNumber", actorNumber }
        };
        view.Owner.SetCustomProperties(playerProperties);

        Debug.Log("Set Player Colour: " + colourIndex);

        //Sync the color for all players
        view.RPC("SetPlayerColour", RpcTarget.AllBuffered, colourIndex);
    }

    [PunRPC]
    public void SetPlayerColour(int colourIndex)
    {
        //Access the owner of this PhotonView to get the player's custom properties
        playerRenderer.color = playerColours[colourIndex];
        Debug.Log($"Set player color to {playerColours[colourIndex]} for {view.Owner.NickName}");
    }

    [PunRPC]
    public void SetPlayerUsername(string nickname)
    {
        // Update the username text
        username.text = view.Owner.NickName;
        Debug.Log("Set Player Username: " + nickname);
    }
}
