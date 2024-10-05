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
    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
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
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                HandleClick();
            }
        }
    }

    void HandleClick()
    {
        Debug.Log("Click");
        Vector2 mousePosition = GetComponentInChildren<Camera>().ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Coin"))
        {
            coinCollect.Play();
            points += hit.collider.gameObject.GetComponent<Coin>().coinPoints;
            Destroy(hit.collider.gameObject);
            view.RPC("ShowPoints", RpcTarget.All, points, view.ViewID);
            Debug.Log("Coin clicked by player: " + view.Owner.NickName);

            //hit.collider.gameObject.GetComponent<Coin>().OnClicked(view.Owner);
        }
    }

    [PunRPC]
    public void ShowPoints(int points, int viewID)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        string ownerName = targetPhotonView.Owner.NickName;
        PlayerSetup.FindComponentInChildren<TextMeshProUGUI>(targetPhotonView.gameObject, "Name").text = ownerName + ": " + points;
    }
}
