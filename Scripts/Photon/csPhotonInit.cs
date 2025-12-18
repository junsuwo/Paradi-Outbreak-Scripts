using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class csPhotonInit : MonoBehaviour
{
    public string version = "Ver 0.1.0";

    public PhotonLogLevel LogLevel = PhotonLogLevel.Full;

    public InputField roomName;

    public GameObject scrollContents;

    public GameObject roomItem;

    public Transform playerPos;

    public InputNickName nickname;


    void Awake()
    {
        nickname = GetComponent<InputNickName>();

        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings(version);

            PhotonNetwork.logLevel = LogLevel;

            //PhotonNetwork.playerName = "GUEST " + Random.Range(1, 9999);
        }

        roomName.text = "ROOM_" + Random.Range(0, 999).ToString("000");

        scrollContents.GetComponent<RectTransform>().pivot = new Vector2(0.0f, 1.0f);
    }

    void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby !!!");

        
    }


}
