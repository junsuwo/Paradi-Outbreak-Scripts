using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon;
using TMPro;

public class RoomChatManager : Photon.MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField inputChat;
    public GameObject chatMessagePrefab;
    public Transform chatContent;
    public ScrollRect scrollRect;
    public Text txtConnect;
    public TMP_Text txtRoomName;

    PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        PhotonNetwork.isMessageQueueRunning = true;
        UpdatePlayerCount();
        Debug.Log("[RoomChatManager] PhotonView ID: " + pv.viewID);
    }

    IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.inRoom);

        string msg = $"<color=#00ff00>[{PhotonNetwork.player.NickName}] 님이 입장하였습니다.</color>";
        pv.RPC("RPC_AddChatMessage", PhotonTargets.AllBuffered, msg);
        //yield return new WaitForSeconds(1f);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
        }
    }

    void UpdatePlayerCount()
    {
        Room currRoom = PhotonNetwork.room;
        txtConnect.text = $"{currRoom.PlayerCount}/{currRoom.MaxPlayers}";
        txtRoomName.text = $"{currRoom.name}";
    }

    void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        UpdatePlayerCount();
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer outPlayer)
    {
        UpdatePlayerCount();
    }

    public void SendChatMessage()
    {
        if (string.IsNullOrWhiteSpace(inputChat.text))
            return;

        string msg = $"<color=blue>[{PhotonNetwork.player.NickName}]</color>{inputChat.text}";
        pv.RPC("RPC_AddChatMessage", PhotonTargets.AllBuffered, msg);
        inputChat.text = "";
        inputChat.ActivateInputField();
    }

    [PunRPC]
    public void RPC_AddChatMessage(string msg)
    {
        // 채팅 프리팹 생성
        GameObject newMsg = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text msgText = newMsg.GetComponent<TMP_Text>();
        msgText.text = msg;

        // 스크롤 아래로 자동 이동
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // 퇴장 처리
    public void OnClickExitRoom()
    {
        string msg = $"<color=#ff0000>[{PhotonNetwork.player.NickName}] 님이 퇴장하였습니다.</color>";
        pv.RPC("RPC_AddChatMessage", PhotonTargets.AllBuffered, msg);
        PhotonNetwork.LeaveRoom();
    }

    void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }



}
