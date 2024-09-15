using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviour
{
    private enum MovementType
    {
        TopDown,
        Platformer,
    }
    [SerializeField] private MovementType movType;
    [SerializeField] private float movSpeed;
    private Rigidbody2D playerRb;
    private SpriteRenderer playerRenderer;

    [SerializeField] private float jumpForce;
    [SerializeField] private Transform feetPos;
    [SerializeField] private float checkRadius;
    [SerializeField] private LayerMask whatIsGround;
    private float jumpTimeCounter;
    [SerializeField] private float jumpTime;
    private bool isGrounded;
    private bool isJumping;

    PhotonView view;
    void Start()
    {
        playerRb = GetComponent<Rigidbody2D>();
        view = GetComponent<PhotonView>();
        if (GetComponent<SpriteRenderer>() != null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }
        else
        {
            playerRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (view.IsMine)
        //if (true)
        {
            //Minimalist top down movement for testing
            if (movType == MovementType.TopDown)
            {
                playerRb.velocity = new Vector2(Input.GetAxis("Horizontal") * movSpeed, Input.GetAxis("Vertical") * movSpeed);
            }

            if (movType == MovementType.Platformer)
            {
                float hMov = Input.GetAxis("Horizontal");

                //Applies horizontal motion to the player
                playerRb.velocity = new Vector2(hMov * movSpeed, playerRb.velocity.y);

                //Flips the player sprite when they turn around
                if (hMov > 0)
                {
                    playerRenderer.flipX = false;
                }
                else if (hMov < 0)
                {
                    playerRenderer.flipX = true;
                }

                //Determines if the player is standing on ground
                //This takes into account if their FEET are within a certain RADIUS of the GROUND
                isGrounded = Physics2D.OverlapCircle(feetPos.position, checkRadius, whatIsGround);

                //Prevents the player from falling too fast
                if (playerRb.velocity.y < -20)
                {
                    playerRb.velocity = new Vector2(playerRb.velocity.x, -20);
                }

                //Applies an upwards force on the player when the space bar is hit
                if (isGrounded == true && Input.GetKeyDown(KeyCode.Space))
                {
                    isJumping = true;
                    jumpTimeCounter = jumpTime;
                    playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
                }

                //Keeps pushing the player up, but stops when the height limit is reached
                if (Input.GetKey(KeyCode.Space) && isJumping == true)
                {
                    if (jumpTimeCounter > 0)
                    {
                        playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
                        jumpTimeCounter -= Time.deltaTime;
                    }
                    else
                    {
                        isJumping = false;
                    }
                }

                //Cancels the upwards force on the player when the key is released
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    isJumping = false;
                }
            }
        }
    }

    //Only works in platformer mode
    public void DoubleJump()
    {
        bool isDoubleJumping = true;
        float doubleJumpTimeCounter = 0.1f;
        feetPos.gameObject.GetComponent<AudioSource>().Play();
        //Applies a jump that can be used in mid air
        while (isDoubleJumping == true)
        {
            if (doubleJumpTimeCounter > 0)
            {
                playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce * 1.5f);
                doubleJumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isDoubleJumping = false;
            }
        }
    }
}
