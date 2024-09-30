using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerSetup : MonoBehaviourPunCallbacks
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
            view.RPC("SetPlayerUsername", RpcTarget.AllBuffered, view.ViewID, PhotonNetwork.LocalPlayer.NickName);
        }
        AssignPlayerColour();
    }

    private SpriteRenderer FindSprite(GameObject parent)
    {
        if (parent.GetComponent<SpriteRenderer>() != null)
        {
            playerRenderer = parent.GetComponent<SpriteRenderer>();
        }
        else
        {
            playerRenderer = parent.GetComponentInChildren<SpriteRenderer>();
        }
        return playerRenderer;
    }

    public static T FindComponentInChildren<T>(GameObject parent, string childName) where T : Component
    {
        if (parent.GetComponent<T>() != null)
        {
            return parent.GetComponent<T>();
        }

        //Get all components of type T in this GameObject and its children
        T[] components = parent.GetComponentsInChildren<T>();

        //If you want just the first found component
        if (components.Length > 0)
        {
            if (childName == "")
            {
                return components[0]; //Return the first found component
            }
            else
            {
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i].name == childName)
                    {
                        return components[i];
                    }
                }
            }
        }

        return null; //Return null if not found
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

        //Debug.Log("Set Player Colour: " + colourIndex);

        //Sync the color for all players
        view.RPC("SetPlayerColour", RpcTarget.AllBuffered, view.ViewID, colourIndex);
    }

    [PunRPC]
    public void SetPlayerColour(int viewID, int colourIndex)
    {
        //Debug.Log($"Set player color to {playerColours[colourIndex]} for {view.Owner.NickName}");
        //Access the owner of this PhotonView to get the player's custom properties
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        FindComponentInChildren<SpriteRenderer>(targetPhotonView.gameObject, "").color = playerColours[colourIndex];
        //FindSprite(targetPhotonView.gameObject).color = playerColours[colourIndex];
    }

    [PunRPC]
    public void SetPlayerUsername(int viewID, string nickname)
    {
        //Update the username text for all players
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        FindComponentInChildren<TextMeshProUGUI>(targetPhotonView.gameObject, "Username").text = nickname;
        //username.text = nickname;
        //Debug.Log("Set Player Username: " + view.Owner.NickName);
    }
}
