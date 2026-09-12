using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class CashGrabbers : MonoBehaviour
{
    private int points;
    [SerializeField] private AudioSource coinCollect;

    public GameObject scorePrefab;
    public Vector3[] multiSpawnPos = new Vector3[4];

    [SerializeField] private Texture2D customCursor;
    private Color[] playerColours = { Color.red, Color.blue, Color.green, Color.yellow };
    [SerializeField] private GameObject cursorPrefab;

    private int streak;
    [SerializeField] private Animator streakAnim;

    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        if (view.IsMine)
        {
            GameObject myScore = PhotonNetwork.Instantiate(scorePrefab.name, multiSpawnPos[PhotonNetwork.LocalPlayer.ActorNumber - 1], Quaternion.identity);
            view.RPC("InitialiseScore", RpcTarget.All, myScore.GetPhotonView().ViewID);
        }

        Vector2 hotspot;
        hotspot = new Vector2(customCursor.width / 2, customCursor.height / 2);
        for (int i = 0; i < customCursor.width; i++)
        {
            for (int j = 0; j < customCursor.height; j++)
            {
                Color pixelColor = customCursor.GetPixel(i, j);
                customCursor.SetPixel(i, j, pixelColor * playerColours[(int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerColour"]]);
            }
        }
        Cursor.SetCursor(customCursor, hotspot, CursorMode.Auto);

        //Only instantiate the cursor if it's not the local player
        if (view.IsMine)
        {
            GameObject myCursor = PhotonNetwork.Instantiate(cursorPrefab.name, new Vector3(0,0,10), Quaternion.identity);
            view.RPC("SetCursorColour", RpcTarget.AllBuffered, myCursor.GetPhotonView().ViewID, (int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerColour"]);
            //myCursor.GetComponent<SpriteRenderer>().color = playerColours[(int)PhotonNetwork.LocalPlayer.CustomProperties["PlayerColour"]];
            myCursor.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    void DisableOtherCameras()
    {
        Camera[] allCameras = Camera.allCameras;

        foreach (Camera cam in allCameras)
        {
            bool isMainCamera = cam.CompareTag("MainCamera");
            Camera childcam = GetComponentInChildren<Camera>();

            if (!isMainCamera && cam != childcam)
            {
                cam.enabled = false;
            }
        }
    }


    [PunRPC]
    public void SetCursorColour(int viewID, int colourIndex)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        targetPhotonView.gameObject.GetComponent<SpriteRenderer>().color = playerColours[colourIndex];
    }

    [PunRPC]
    public void InitialiseScore(int viewID)
    {
        Debug.Log(PhotonView.Find(viewID).Owner.ActorNumber);
        PlayerSetup.FindComponentInChildren<TextMeshProUGUI>(PhotonView.Find(viewID).gameObject, "ScoreName").name = "ScoreName" + PhotonView.Find(viewID).Owner.ActorNumber;
    }

    // Update is called once per frame
    void Update()
    {
        if (!view.IsMine)
        {
            //PowerUps.HideUIForRemotePlayers(gameObject);
        }

        if (view.IsMine)
        {
            DisableOtherCameras();
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                HandleClick();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D coin)
    {
        if (coin.CompareTag("Coin") && (int)PhotonNetwork.LocalPlayer.CustomProperties["PowerUp"] == 3)
        {
            Debug.Log("Tax Collected");
            points += coin.gameObject.GetComponent<Coin>().coinPoints;
            PhotonView coinView = coin.GetComponent<PhotonView>();
            if (coinView != null)
            {
                view.RPC("RequestDestroyCoin", RpcTarget.MasterClient, coinView.ViewID);
            }
            view.RPC("ShowPoints", RpcTarget.All, points, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    void HandleClick()
    {
        Debug.Log("Click");
        Vector2 mousePosition = GetComponentInChildren<Camera>().ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Coin"))
        {
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties["PowerUp"] == 1)
            {
                streak++;
            }
            if (streak >= 5)
            {
                streakAnim.SetTrigger("Streaking");
                points += 5;
                streak = 0;
            }
            coinCollect.Play();
            points += hit.collider.gameObject.GetComponent<Coin>().coinPoints;
            PhotonView coinView = hit.collider.GetComponent<PhotonView>();
            if (coinView != null)
            {
                view.RPC("RequestDestroyCoin", RpcTarget.MasterClient, coinView.ViewID);
            }
            //PhotonNetwork.Destroy(hit.collider.gameObject);
            view.RPC("ShowPoints", RpcTarget.All, points, PhotonNetwork.LocalPlayer.ActorNumber);
            Debug.Log("Coin clicked by player: " + view.Owner.NickName);

            //hit.collider.gameObject.GetComponent<Coin>().OnClicked(view.Owner);
        }
        else
        {
            streak = 0;
        }
    }

    // On all clients
    [PunRPC]
    public void RequestDestroyCoin(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView view = PhotonView.Find(viewID);
            if (view != null)
            {
                PhotonNetwork.Destroy(view.gameObject);
            }
        }
    }


    [PunRPC]
    public void ShowPoints(int playePoints, int actorNum)
    {
        //PhotonView targetPhotonView = PhotonView.Find(viewID);
        //string ownerName = targetPhotonView.Owner.NickName;
        //PlayerSetup.FindComponentInChildren<TextMeshProUGUI>(targetPhotonView.gameObject, "Name").text = ownerName + ": " + points;
        Debug.Log(actorNum + " " + playePoints);
        GameObject.Find("ScoreName" + actorNum).GetComponent<TextMeshProUGUI>().text = playePoints.ToString();
    }
}
