using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public class Laucher : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    RoomOptions roomOptions = new RoomOptions();

    void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = false;
        }


    public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = gameVersion;
            }
        }



    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master.");
        PhotonNetwork.JoinRandomRoom();

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected has been called with the reason : {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {

        Debug.Log("Failed to join a room... Creating one");

        PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 4});


    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room successfuly !");

        PhotonNetwork.LoadLevel("Game");
        // if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        // {
        // }
    }
}
