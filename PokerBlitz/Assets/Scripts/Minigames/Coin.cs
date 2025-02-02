using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Coin : MonoBehaviour
{
    private float coinSpeed = 5f;
    internal int coinPoints;
    private bool isRed;
    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            float redChance = Random.Range(0, 100);
            if (redChance < 5)
            {
                isRed = true;
            }

            view.RPC("IsCoinRed", RpcTarget.All, view.ViewID, isRed);
        }
    }

    [PunRPC]
    public void IsCoinRed(int viewID, bool red)
    {
        PhotonView targetPhotonView = PhotonView.Find(viewID);
        Coin coinComponent = targetPhotonView.gameObject.GetComponent<Coin>();
        if (red)
        {
            coinComponent.coinPoints = 5;
            targetPhotonView.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            coinComponent.coinPoints = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.down * coinSpeed * Time.deltaTime);
    }
}
