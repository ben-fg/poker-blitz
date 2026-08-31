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

    private bool requestInFlight; // blocks a second click while the first is still resolving

    void Start()
    {

    }

    public void CreateRoom()
    {
        if (requestInFlight || !PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Not ready to create a room yet (still connecting, or a request is already in flight).");
            return;
        }

        RoomOptions options = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = false
    };
        requestInFlight = true;
        PhotonNetwork.CreateRoom(createInput.text, options, TypedLobby.Default);
    }

    public void JoinRoom()
    {
        if (requestInFlight || !PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Not ready to join a room yet (still connecting, or a request is already in flight).");
            return;
        }

        requestInFlight = true;
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
        requestInFlight = false;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Create Room Failed: {message}");
        requestInFlight = false;
    }
}
