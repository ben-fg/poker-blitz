using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{
    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        //PhotonNetwork.LoadLevel("WaitingLobby");
        Player localPlayer = PhotonNetwork.LocalPlayer;
        Debug.Log("Player joined with ActorNumber: " + localPlayer.ActorNumber);
        PhotonNetwork.LoadLevel("WaitingLobby");
    }
}
