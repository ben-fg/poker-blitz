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
        RoomOptions options = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = false
    };
        PhotonNetwork.CreateRoom(createInput.text, options, TypedLobby.Default);
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

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {message}");
    }
}
