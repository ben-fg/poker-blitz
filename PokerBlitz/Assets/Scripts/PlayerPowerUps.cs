using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerPowerUps : MonoBehaviour
{
    private float[] powerUpCooldowns = new float[3];
    private PlayerMovement playerMovement;
    PhotonView view;
    private GameObject myPowerUp = null;
    private SpriteRenderer playerRenderer;
    private int powerUpNum;
    private float displacement = 0;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Sprite[] powerUpSprites = new Sprite[3];
    [SerializeField] private TextMeshProUGUI cooldown;

    private CannonShoot cannon;
    private Rigidbody2D playerRb;

    public static bool selectionEnd = false;

    void Start()
    {
        PlayerMovement.isFrozen = true;
        view = GetComponent<PhotonView>();
        playerMovement = GetComponent<PlayerMovement>();
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
        if (GameMaster.gameNumber == 1)
        {
            //Debug.Log("num "+powerUpNum);
            //Debug.Log("cooldown " + powerUpCooldowns[2]);
            if (selectionEnd)
            {
                //Selects the appropriate powerup
                powerUpNum = (int)PhotonNetwork.LocalPlayer.CustomProperties["PowerUp"];
                if (powerUpNum == 1)
                {
                    myPowerUp = GetChildByTag(GetComponent<Transform>(), "PU1");
                    displacement = 1.666667f;
                }
                else if (powerUpNum == 2)
                {
                    myPowerUp = GetChildByTag(GetComponent<Transform>(), "PU2");
                }

                abilityIcon.sprite = powerUpSprites[powerUpNum - 1];
                abilityIcon.color = Color.white;

                PlayerMovement.isFrozen = false;
                selectionEnd = false;
            }

            if (powerUpNum != 0)
            {
                //Counts down any ability on cooldown
                if (powerUpCooldowns[powerUpNum - 1] > 0)
                {
                    cooldown.color = Color.white;
                    cooldown.text = (powerUpCooldowns[powerUpNum - 1] + 0.5).ToString("0");
                    abilityIcon.color = Color.black;
                    powerUpCooldowns[powerUpNum - 1] -= Time.deltaTime;
                }
                else
                {
                    if (powerUpNum != 0)
                    {
                        cooldown.color = Color.clear;
                        abilityIcon.color = Color.white;
                    }
                }

                //Makes sure the boxing glove faces the right direction
                if (powerUpNum == 1)
                {
                    view.RPC("FlipItemRPC", RpcTarget.All, "PU1", Input.GetAxis("Horizontal"));
                }

                //Applies the selected powerups ability
                if (view.IsMine && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
                {
                    if (powerUpNum == 1 && powerUpCooldowns[0] <= 0)
                    {
                        StartCoroutine(TogglePowerUp(0.5f, "PU1"));
                        powerUpCooldowns[0] = 7;
                        myPowerUp.GetComponent<AudioSource>().Play();
                    }
                    else if (powerUpNum == 2 && powerUpCooldowns[1] <= 0)
                    {
                        StartCoroutine(TogglePowerUp(4f, "PU2"));
                        powerUpCooldowns[1] = 12;
                        myPowerUp.GetComponent<AudioSource>().Play();
                    }
                    else if (powerUpNum == 3 && powerUpCooldowns[2] <= 0)
                    {
                        playerMovement.DoubleJump();
                        powerUpCooldowns[2] = 4;
                    }
                }
            }
        }
        else if (GameMaster.gameNumber == 2)
        {
            if (selectionEnd)
            {
                //Selects the appropriate powerup
                powerUpNum = (int)PhotonNetwork.LocalPlayer.CustomProperties["PowerUp"];
                if (view.IsMine)
                {
                    view.RPC("ChangeCannon", RpcTarget.All);
                }
                cannon = GetComponent<CannonShoot>();
                playerRb = GetComponent<Rigidbody2D>();
                if (powerUpNum == 1)
                {
                    cannon.SetCannonProperties(15, 25, view.Owner.ActorNumber);
                }
                else if (powerUpNum == 2)
                {
                    cannon.SetCannonProperties(10, 100, view.Owner.ActorNumber);
                    GetComponent<CircleCollider2D>().radius = 1;
                }
                else if (powerUpNum == 3)
                {
                    playerMovement.SetSpeed(10);
                }
                else if (powerUpNum == 0)
                {
                    cannon.SetCannonProperties(12.5f, 50, view.Owner.ActorNumber);
                }
                PlayerMovement.isFrozen = false;
                selectionEnd = false;
            }

            //Counts down a cooldown (fire rate)
            if (powerUpCooldowns[powerUpNum] > 0)
            {
                powerUpCooldowns[powerUpNum] -= Time.deltaTime;
            }

            //Allows the player to shoot
            if (view.IsMine && Input.GetKey(KeyCode.Mouse0))
            {
                //Vector2 direction = new Vector2(Mathf.Sin(Mathf.Abs(playerRenderer.transform.rotation.z) + 90), Mathf.Cos(Mathf.Abs(playerRenderer.transform.rotation.z) + 90));
                Vector2 direction = playerRenderer.transform.right;
                if (powerUpNum == 1 && powerUpCooldowns[1] <= 0)
                {
                    Debug.Log("Gunner");
                    //StartCoroutine(TogglePowerUp(0.5f, "PU1"));
                    powerUpCooldowns[1] = 0.2f;
                    //myPowerUp.GetComponent<AudioSource>().Play();
                    cannon.Shoot(direction);
                }
                else if (powerUpNum == 2 && powerUpCooldowns[2] <= 0)
                {
                    Debug.Log("Tank");
                    //StartCoroutine(TogglePowerUp(4f, "PU2"));
                    powerUpCooldowns[2] = 2;
                    //myPowerUp.GetComponent<AudioSource>().Play();
                    cannon.Shoot(direction);
                }
                else if (powerUpNum == 0 && powerUpCooldowns[0] <= 0)
                {
                    Debug.Log("Rookie");
                    powerUpCooldowns[0] = 4;
                    cannon.Shoot(direction);
                }
            }
        }
    }

    [PunRPC]
    public void ChangeCannon()
    {
        playerRenderer.sprite = powerUpSprites[powerUpNum - 1];
    }

    public IEnumerator TogglePowerUp(float duration, string powerUpName)
    {
        view.RPC("ActivateTriggerRPC", RpcTarget.All, powerUpName);
        yield return new WaitForSeconds(duration);
        view.RPC("DeactivateTriggerRPC", RpcTarget.All, powerUpName);
    }

    [PunRPC]
    public void ActivateTriggerRPC(string powerUpName)
    {
        //Set the trigger to active for all clients
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        myPowerUp.SetActive(true);
        Debug.Log("Trigger activated on all clients");
    }

    [PunRPC]
    public void DeactivateTriggerRPC(string powerUpName)
    {
        //Set the trigger to inactive for all clients
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        myPowerUp.SetActive(false);
        Debug.Log("Trigger activated on all clients");
    }

    [PunRPC]
    public void FlipItemRPC(string powerUpName, float hMov)
    {
        displacement = 1.666667f;
        myPowerUp = GetChildByTag(GetComponent<Transform>(), powerUpName);
        if (hMov > 0)
        {
            myPowerUp.GetComponent<SpriteRenderer>().flipX = false;
            myPowerUp.transform.position = new Vector2(transform.position.x + displacement, transform.position.y);
        }
        else if (hMov < 0)
        {
            myPowerUp.GetComponent<SpriteRenderer>().flipX = true;
            myPowerUp.transform.position = new Vector2(transform.position.x - displacement, transform.position.y);
        }
    }

    private static GameObject GetChildByTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child.gameObject;
            }
        }
        return null;
    }
}
