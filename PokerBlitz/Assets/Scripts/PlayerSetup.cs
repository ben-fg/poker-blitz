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
    Player localPlayer = PhotonNetwork.LocalPlayer;
    public Color[] playerColours = { Color.red, Color.blue, Color.green, Color.yellow };
    private Renderer playerRenderer;
    [SerializeField] private TextMeshProUGUI username;
    PhotonView view;
    Hashtable playerProperties = new Hashtable();

    void Start()
    {
        view = GetComponent<PhotonView>();

        if (GetComponent<Renderer>() != null)
        {
            playerRenderer = GetComponent<Renderer>();
        }
        else
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }

        if (view.IsMine)
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("Username");
        }
        view.RPC("SetPlayerUsername", RpcTarget.AllBuffered, PhotonNetwork.NickName);

        AssignPlayerColour();
    }
    void Update()
    {

    }

    void AssignPlayerColour()
    {
        int actorNumber = localPlayer.ActorNumber;

        //Loops through the array incase a player leaves in the waiting lobby and joins back
        int colourIndex = (actorNumber - 1) % playerColours.Length;

        //Sets the player a colour property based on their number
        playerProperties["PlayerColour"] = colourIndex;
        localPlayer.SetCustomProperties(playerProperties);

        Debug.Log($"Player {actorNumber} has been assigned color {playerColours[colourIndex]}.");

        view.RPC("SetPlayerColour", RpcTarget.AllBuffered, colourIndex);
    }

    [PunRPC]
    public void SetPlayerColour(int colourIndex)
    {
        //Ensures all player colours are updated for everyone
        Debug.Log(colourIndex);
        playerRenderer.material.color = playerColours[colourIndex];
    }

    [PunRPC]
    public void SetPlayerUsername(string nickname)
    {
        //Ensures all player usernames are updated for everyone
        Debug.Log(nickname);
        username.text = nickname;
    }
}
