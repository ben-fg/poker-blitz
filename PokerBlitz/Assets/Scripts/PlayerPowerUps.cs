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
    private float startCountdown = 5;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private AudioSource music;

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

        selectionEnd = true;
    }

    void Update()
    {
        if (startCountdown > 0)
        {
            countdownText.text = (startCountdown + 0.5).ToString("0");
            startCountdown -= Time.deltaTime;
        }
        else if (startCountdown > -10)
        {
            countdownText.gameObject.SetActive(false);
            music.Play();
            PlayerMovement.isFrozen = false;
            startCountdown = -20;
        }

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
                if (!PlayerMovement.isFrozen && view.IsMine && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)))
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
                    view.RPC("ChangeCannon", RpcTarget.All, view.ViewID, powerUpNum);

                    cannon = GetComponent<CannonShoot>();
                    playerRb = GetComponent<Rigidbody2D>();
                    if (powerUpNum == 1)
                    {
                        view.RPC("SetCannonPropertiesRPC", RpcTarget.AllBuffered, view.ViewID, 15f, 25, view.ViewID, "Gunner");
                        //cannon.SetCannonProperties(15, 25, PhotonNetwork.LocalPlayer.ActorNumber, "Gunner");
                        Debug.Log("Gunner owner: " + PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    else if (powerUpNum == 2)
                    {
                        view.RPC("SetCannonPropertiesRPC", RpcTarget.AllBuffered, view.ViewID, 10f, 100, view.ViewID, "Tank");
                        //cannon.SetCannonProperties(10, 100, PhotonNetwork.LocalPlayer.ActorNumber, "Tank");
                        GetComponent<CircleCollider2D>().radius = 1;
                        Debug.Log("Tank owner: " + PhotonNetwork.LocalPlayer.ActorNumber);
                    }
                    else if (powerUpNum == 3)
                    {
                        playerMovement.SetSpeed(10);
                    }
                    else if (powerUpNum == 0)
                    {
                        view.RPC("SetCannonPropertiesRPC", RpcTarget.AllBuffered, view.ViewID, 12.5f, 50, view.ViewID, "Rookie");
                        //cannon.SetCannonProperties(12.5f, 50, PhotonNetwork.LocalPlayer.ActorNumber, "Rookie");
                    }
                }

                selectionEnd = false;
            }

            if (powerUpNum != 3)
            {
                //Counts down a cooldown (fire rate)
                if (powerUpCooldowns[powerUpNum] > 0)
                {
                    powerUpCooldowns[powerUpNum] -= Time.deltaTime;
                }
            }

            //Allows the player to shoot
            if (!PlayerMovement.isFrozen && view.IsMine && Input.GetKey(KeyCode.Mouse0))
            {
                //Vector2 direction = new Vector2(Mathf.Sin(Mathf.Abs(playerRenderer.transform.rotation.z) + 90), Mathf.Cos(Mathf.Abs(playerRenderer.transform.rotation.z) + 90));
                Vector2 direction = playerRenderer.transform.right;
                if (powerUpNum == 1 && powerUpCooldowns[1] <= 0)
                {
                    //Debug.Log("Gunner");
                    //StartCoroutine(TogglePowerUp(0.5f, "PU1"));
                    powerUpCooldowns[1] = 0.2f;
                    //myPowerUp.GetComponent<AudioSource>().Play();
                    cannon.Shoot(direction);
                }
                else if (powerUpNum == 2 && powerUpCooldowns[2] <= 0)
                {
                    //Debug.Log("Tank");
                    //StartCoroutine(TogglePowerUp(4f, "PU2"));
                    powerUpCooldowns[2] = 2;
                    //myPowerUp.GetComponent<AudioSource>().Play();
                    cannon.Shoot(direction);
                }
                else if (powerUpNum == 0 && powerUpCooldowns[0] <= 0)
                {
                    //Debug.Log("Rookie");
                    powerUpCooldowns[0] = 0.75f;
                    cannon.Shoot(direction);
                }
            }
        }
    }

    [PunRPC]
    public void ChangeCannon(int viewID, int powerUpNum)
    {
        if (powerUpNum != 0)
        {
            PhotonView targetPhotonView = PhotonView.Find(viewID);
            SpriteRenderer cannonRen = PlayerSetup.FindComponentInChildren<SpriteRenderer>(targetPhotonView.gameObject, "");
            cannonRen.sprite = powerUpSprites[powerUpNum - 1];
        }
    }

    [PunRPC]
    public void SetCannonPropertiesRPC(int viewID, float speed, int damage, int owner, string type)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        targetPhotonView.gameObject.GetComponent<CannonShoot>().SetCannonProperties(speed, damage, owner, type);
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
