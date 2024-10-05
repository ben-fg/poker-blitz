using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnCoins : MonoBehaviour
{
    private float delay = 0.3f;
    private float duration = 54;
    [SerializeField] private GameObject coin;
    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (duration >= 0)
            {
                duration -= Time.deltaTime;
                if (delay >= 0)
                {
                    delay -= Time.deltaTime;
                }
                else
                {
                    //view.RPC("DropCoin", RpcTarget.All, Random.Range(-8f, 8f));
                    DropCoin(Random.Range(-8f, 8f));
                    delay = 0.15f;
                }
            }
        }
    }

    //[PunRPC]
    public void DropCoin(float xPos)
    {
        GameObject currentCoin = PhotonNetwork.Instantiate(coin.name, new Vector3(xPos, 8.5f, 10), Quaternion.identity);
        currentCoin.GetComponent<Coin>().coinPoints = 1;
    }
}
