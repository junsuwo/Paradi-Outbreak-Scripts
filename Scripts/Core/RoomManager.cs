using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class RoomManager : Photon.MonoBehaviour
{
    [Header("UI References")]
    public Transform playerListContent;      // PlayerConnectPanel의 Content
    public GameObject playerSlotPrefab;      // PlayerSlot Prefab 
    public Button readyButton;               // Ready 버튼
    public Button startButton;               // Start 버튼

    //각 플레이어의 슬롯과 준비 상태 관리용 Dictionary
    private Dictionary<int, GameObject> playerSlotDict = new Dictionary<int, GameObject>();
    private Dictionary<int, bool> readyStates = new Dictionary<int, bool>();

    //로컬 플레이어의 준비 상태 false
    private bool isReady = false;


    void Start()
    {
        // 버튼 연결
        readyButton.onClick.AddListener(OnClickReady);
        startButton.onClick.AddListener(OnClickStartGame);

        //Ready,Start 버튼 세팅
        SetupButtons();

        // 기존 플레이어 리스트 초기화
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            AddPlayerSlot(player);
            readyStates[player.ID] = false;
        }

        // 초기 상태 텍스트 갱신
        UpdatePlayerStateTexts();
    }

    //버튼 세팅 함수(마스터/로컬 플레이어)
    void SetupButtons()
    {
        // Master/일반 유저에 따라 버튼 보이기
        if (PhotonNetwork.isMasterClient)
        {
            //마스터는 Ready 버튼 숨기고 Strat 버튼 표시
            readyButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            startButton.interactable = true;
        }
        else
        {
            //일반 플레이어는 Ready 버튼만 보이게
            readyButton.gameObject.SetActive(true);
            startButton.gameObject.SetActive(false);
        }
    }

    //마스터 교체 콜백
    public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        Debug.Log("새로운 마스터: " + newMasterClient.NickName);

        //마스터 변경 시 전체 상태 텍스트 다시 갱신
        UpdatePlayerStateTexts();

        //버튼 표시 다시 세팅(마스터/로컬 구분)
        SetupButtons();

        //내가 새 마스터가 되었을 경우
        if (PhotonNetwork.player == newMasterClient)
        {
            //Start 버튼 초기화
            startButton.interactable = false;

            //혹시 Ready 상태였다면 해제
            isReady = false;
            if (readyStates.ContainsKey(PhotonNetwork.player.ID))
                readyStates[PhotonNetwork.player.ID] = false;

            //모든 클라이언트에 내 Ready 해제 상태 전송
            photonView.RPC("SyncReadyState", PhotonTargets.All, PhotonNetwork.player.ID, false);
        }
    }

    // 플레이어 입장 콜백
    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        //새 플레이어 슬롯 추가
        AddPlayerSlot(newPlayer);
        readyStates[newPlayer.ID] = false;
        UpdatePlayerStateTexts();
    }

    //플레이어 퇴장 콜백
    public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        // 나간 플레이어의 슬롯과 Ready 상태 제거
        if (playerSlotDict.ContainsKey(otherPlayer.ID))
        {
            Destroy(playerSlotDict[otherPlayer.ID]);
            playerSlotDict.Remove(otherPlayer.ID);
            readyStates.Remove(otherPlayer.ID);
        }
        UpdatePlayerStateTexts();
    }

    // 플레이어 슬롯 추가
    void AddPlayerSlot(PhotonPlayer player)
    {
        // 부모에 붙이면서 월드 위치 유지하지 않음 → 로컬(레이아웃) 기준으로 바로 정렬됨
        GameObject slot = Instantiate(playerSlotPrefab, playerListContent, false);

        // 슬롯 내부 Text 세팅 (프리팹 구조: [0]=NameText, [1]=StateText)
        TMP_Text[] texts = slot.GetComponentsInChildren<TMP_Text>();
        texts[0].text = player.NickName;
        texts[1].text = "";

        // 딕셔너리 등록
        playerSlotDict[player.ID] = slot;

        // 레이아웃 강제 갱신 (즉시 반영)
        LayoutRebuilder.ForceRebuildLayoutImmediate(playerListContent.GetComponent<RectTransform>());
    }

    // Ready 버튼 클릭
    void OnClickReady()
    {
        isReady = !isReady;
        // 버튼 텍스트 변경 Unready <-> Ready
        TMP_Text btnText = readyButton.GetComponentInChildren<TMP_Text>();
        btnText.text = isReady ? "Unready" : "Ready";

        //모든 클라이언트에 내 Ready 상태 전송
        photonView.RPC("SyncReadyState", PhotonTargets.All, PhotonNetwork.player.ID, isReady);
    }

    //모든 클라이언트에 Ready 상태 동기화
    [PunRPC]
    void SyncReadyState(int playerID, bool ready)
    {
        //해당 플레이어의 Ready 상태 갱신
        readyStates[playerID] = ready;

        //UI 텍스트 업데이트
        UpdatePlayerStateTexts();

        //마스터면 전체 Ready 상태 체크 
        if (PhotonNetwork.isMasterClient)
        {
            CheckAllReady();
        }
    }

    //각 플레이어 슬롯의 상태 텍스트 업데이트
    void UpdatePlayerStateTexts()
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            if (!playerSlotDict.ContainsKey(player.ID)) continue;

            TMP_Text[] texts = playerSlotDict[player.ID].GetComponentsInChildren<TMP_Text>();
            TMP_Text nameText = texts[0];
            TMP_Text stateText = texts[1];

            if (player.IsMasterClient)
            {
                //마스터 표시
                stateText.text = "<color=#FFD700>Master</color>";
            }
            else if (readyStates.ContainsKey(player.ID) && readyStates[player.ID])
            {
                //Ready 표시
                stateText.text = "<color=#00FF00>Ready</color>";
            }
            else
            {
                //아무것도 표시 안함
                stateText.text = "";
            }
        }
    }

    //전체 Ready 상태 체크(마스터 전용)
    void CheckAllReady()
    {
        // 마스터는 항상 true
        foreach (PhotonPlayer p in PhotonNetwork.playerList)
        {
            if (p.IsMasterClient) continue;
            if (!readyStates.ContainsKey(p.ID) || !readyStates[p.ID])
            {
                startButton.interactable = false;

                Image img = startButton.GetComponent<Image>();
                Color color = img.color;
                color.a = 0.4f;
                img.color = color;

                return;
            }
        }
        // 전원 준비 완료
        startButton.interactable = true;
        Image bright = startButton.GetComponent<Image>();
        Color C = bright.color;
        //전원 준비 완료시 완전 불투명으로 
        C.a = 1f;
        bright.color = C;
    }

    // 게임 시작
    void OnClickStartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            ExitGames.Client.Photon.Hashtable newProps = new ExitGames.Client.Photon.Hashtable();
            newProps["isPlaying"] = true;
            PhotonNetwork.room.SetCustomProperties(newProps);

            photonView.RPC("StartGame", PhotonTargets.All);
        }
    }

    [PunRPC]
    void StartGame()
    {
        SceneManager.LoadScene("CharacterSelect");
    }
}
