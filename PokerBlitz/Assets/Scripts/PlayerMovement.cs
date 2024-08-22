using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    private enum MovementType
    {
        TopDown,
    }
    [SerializeField] private MovementType movType;
    [SerializeField] private float movSpeed;
    private Rigidbody2D playerRb;
    PhotonView view;
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (view.IsMine)
        {
            //Minimalist top down movement for testing
            if (movType == MovementType.TopDown)
            {
                playerRb.velocity = new Vector2(Input.GetAxis("Horizontal") * movSpeed, Input.GetAxis("Vertical") * movSpeed);
            }
        }
    }
}
