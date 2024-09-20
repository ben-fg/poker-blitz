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
    private float hMov;
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

    private bool isStunned;
    public static bool isFrozen;
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
        if (view.IsMine && !isFrozen && !isStunned)
        //if (true)
        {
            //Minimalist top down movement for testing
            if (movType == MovementType.TopDown)
            {
                playerRb.velocity = new Vector2(Input.GetAxis("Horizontal") * movSpeed, Input.GetAxis("Vertical") * movSpeed);
            }

            if (movType == MovementType.Platformer)
            {
                hMov = Input.GetAxis("Horizontal");

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
                if (isGrounded == true && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)))
                {
                    isJumping = true;
                    jumpTimeCounter = jumpTime;
                    playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce);
                }

                //Keeps pushing the player up, but stops when the height limit is reached
                if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W)) && isJumping == true)
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
                if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))
                {
                    isJumping = false;
                }
            }
        }
        //Freezes the player
        if (isFrozen)
        {
            playerRb.velocity = new Vector2(0, 0);
        }
        if (isStunned)
        {
            if (movType == MovementType.Platformer)
            {
                //Applies horizontal motion to the player
                playerRb.velocity = new Vector2(playerRb.velocity.x, playerRb.velocity.y);
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
                playerRb.velocity = new Vector2(playerRb.velocity.x, jumpForce * 1.25f);
                doubleJumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isDoubleJumping = false;
            }
        }
    }

    public IEnumerator StunPlayer(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameMaster.gameNumber == 1)
        {
            if (collision.CompareTag("PU1"))
            {
                StartCoroutine(StunPlayer(1));
                if (collision.GetComponent<SpriteRenderer>().flipX)
                {
                    playerRb.AddForce(new Vector2(-1000, 400f));
                }
                else
                {
                    playerRb.AddForce(new Vector2(1000, 400f));
                }
            }

            if (collision.CompareTag("PU2"))
            {
                StartCoroutine(StunPlayer(4));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (GameMaster.gameNumber == 1)
        {
            if (collision.CompareTag("PU2"))
            {
                Vector2 magnetPos = collision.GetComponent<Transform>().position;
                Vector2 direction = (magnetPos - (Vector2)transform.position).normalized;
                playerRb.AddForce(direction * 100);
            }
        }
    }
}
