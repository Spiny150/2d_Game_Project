using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Manager : MonoBehaviourPunCallbacks
{

    GameObject player;
    CameraMovement cam;

    DungeonGenerator dg;
    

    int randomSeed;

    private void Awake() 
    {

        cam = GameObject.Find("Main Camera").GetComponent<CameraMovement>();
        dg = gameObject.GetComponent<DungeonGenerator>();      
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            cam.player = player;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            //randomSeed = ((int) System.DateTime.Now.Ticks);
            SetSeedAndGenerate(1);
        }
    }

    public override void OnPlayerEnteredRoom(Player remotePlayer) {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("SetSeedAndGenerate", remotePlayer, randomSeed);

        //player.transform.position = Vector2.zero;
    }

    [PunRPC]
    void SetSeedAndGenerate(int seed)
    {
        Random.InitState(seed);
        dg.RandomRooms();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            dg.ClearMap();
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            //dg.GenerateDungeon();
            dg.RandomRooms();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dg.GenerateMinimumSpanningTree();
        }


    }

}
