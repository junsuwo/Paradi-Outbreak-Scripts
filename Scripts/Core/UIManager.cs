using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class UIManager : MonoBehaviour, IGameSystem
{
    [Header("Team Perks UI")]
    public GameObject teamPerksPnl;
    public Button[] teamPerksBtn;

    [Header("Personal Perks UI")]
    public GameObject personalPerksPnl;
    public Button[] personalPerksBtn;

    [Header("Shop UI")]
    public GameObject shopPanel;
    [SerializeField] TMP_Text personalCoinText;

    [Header("Inventory UI")]
    public GameObject inventoryPanel;

    [Header("HUD Root (인게임 UI)")]
    public GameObject hudRoot; // 스탯창, 퀵슬롯, 미니맵, 팀바 등이 들어있는 상위 오브젝트


    [Header("Game Flow Settings")]
    public string lobbySceneName = "Lobby";

    [Header("All Panels (등록 필수)")]
    public GameObject[] uiPanels;

    private PerksManager perksManager;
    private CameraFollow cameraFollow;
    public static bool IsUIOpen { get; private set; } = false;

    [Header("투표 확인")]
    public Text reviveVoteText;
    public Text buffVoteText;
    public Text wallVoteText;
    public Text timerText;


    [Header("Game Flow UI")]
    public GameObject waveClearPanel; //웨이브 클리어 화면
    public GameObject waveStartPanel; //웨이브 스타트 화면
    public GameObject gameClearPanel; // 게임 클리어 화면
    public GameObject gameOverPanel; // 게임 오버 화면
    public GameObject optionPanel; // ESC 옵션창


    [Header("Wave Texts")]
    public TMP_Text ingameWaveText; // Hud쪽 wave x/ y"
    //public TMP_Text waveClearWaveText; // 웨이브 클리어창 안 텍스트
    //public TMP_Text gameOverWaveText; // 게임 오버창 안 텍스트
    //public TMP_Text gameClearWaveText; // 게임 클리어창 안 텍스트


    private PerksVoteManager voteManager;
    private bool waitingPersonalPerks = false;

    // 팀 특전 아이콘 경로 (파일명과 정확히 일치)
    private readonly Dictionary<TeamPerksType, string> teamIconPath = new()
    {
        { TeamPerksType.ReviveAll,    "UI/Perks/SurveyCorps"}, // 대표 아이콘 매핑
        { TeamPerksType.TeamStatBuff, "UI/Perks/StationaryGuard" },
        { TeamPerksType.WallEnforce,  "UI/Perks/militarypoliceforce"},
    };

    // 개인 정수 아이콘 경로 (파일명과 정확히 일치)
    private readonly Dictionary<EssenceType, string> essenceIconPath = new()
    {
        { EssenceType.ArmoredEssence,   "UI/Perks/Armored Titan"   },
        { EssenceType.AttackEssence,    "UI/Perks/Attack Titan"    },
        { EssenceType.BeastEssence,     "UI/Perks/Beast Titan"     },
        { EssenceType.CartEssence,      "UI/Perks/Cart Titan"      },
        { EssenceType.ColossalEssence,  "UI/Perks/Colossus Titan"  }, // 파일명: Colossus
        { EssenceType.FemaleEssence,    "UI/Perks/Female Titan"    },
        { EssenceType.JawEssence,       "UI/Perks/Jaw Titan"       },
        { EssenceType.WarHammerEssence, "UI/Perks/Warhammer Titan" },
        { EssenceType.TheFoundingEssence, "UI/Perks/Founding Titan" }
        // TheFoundingEssence 아이콘이 아직 폴더에 없으면 경고만 출력됨
    };

    void Awake()
    {
        foreach (var p in uiPanels)
            if (p != null) p.SetActive(false);

        if (hudRoot != null)
            hudRoot.SetActive(true);

        IsUIOpen = false;
        Debug.Log("[UIManager] Awake 초기화 완료: 모든 패널 비활성화");
    }

    public void Init()
    {
        perksManager = GameManager.Instance.perksManager;
        //voteManager = FindObjectOfType<PerksVoteManager>(true);
        cameraFollow = FindObjectOfType<CameraFollow>();

        foreach (var p in uiPanels)
            if (p != null) p.SetActive(false);
    }

    public void OpenUI(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(true);
        UpdateUIState();
    }
    public void CloseUI(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(false);
        UpdateUIState();
    }

    void UpdateUIState()
    {
        IsUIOpen = AnyUIOpen();

        // 어떤 UI라도 하나 켜져 있으면 HUD 끄기
        if (hudRoot != null)
            hudRoot.SetActive(!IsUIOpen);

        if (IsUIOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.TryReconnectTarget();
    }

    bool AnyUIOpen()
    {
        foreach (var panel in uiPanels)
        {
            if (panel != null && panel.activeSelf)
                return true;
        }
        return false;
    }

    public void ShowTeamPerksChoices()
    {
        OpenUI(teamPerksPnl);

        if (voteManager == null)
        {
            voteManager = FindObjectOfType<PerksVoteManager>();
            if (voteManager == null)
            {
                Debug.LogError("[UI] voteManager를 찾을 수 없습니다");
                return;
            }
            else
                Debug.Log("[UI] voteManager 연결 성공");
        }

        foreach (var btn in teamPerksBtn) btn.onClick.RemoveAllListeners();

        // 아이콘
        SetButtonIcon(teamPerksBtn[0], teamIconPath[TeamPerksType.ReviveAll]);
        SetButtonIcon(teamPerksBtn[1], teamIconPath[TeamPerksType.TeamStatBuff]);
        SetButtonIcon(teamPerksBtn[2], teamIconPath[TeamPerksType.WallEnforce]);

        teamPerksBtn[0].onClick.AddListener(() => OnSelectTeamPerks(TeamPerksType.ReviveAll));
        teamPerksBtn[1].onClick.AddListener(() => OnSelectTeamPerks(TeamPerksType.TeamStatBuff));
        teamPerksBtn[2].onClick.AddListener(() => OnSelectTeamPerks(TeamPerksType.WallEnforce));

        UpdateVoteUI(0, 0, 0);
        if (voteManager)
            UpdateVoteTimer((int)voteManager.voteDuration);
    }

    void OnSelectTeamPerks(TeamPerksType type)
    {

        if (voteManager == null)
        {
            Debug.LogError("[UI] voteManager가 null입니다. SubmitVote 실행 불가");
            return;
        }

        Debug.Log($"[UI] 투표 버튼 클릭됨 -> {type}");
        voteManager.SubmitVote(type);

    }

    public void UpdateVoteUI(int revive, int buff, int wall)
    {
        Debug.Log($"[UI] 투표 UI 업데이트 -> {revive}/{buff}/{wall}");
        reviveVoteText.text = $"투표: {revive}";
        buffVoteText.text = $"투표: {buff}";
        wallVoteText.text = $"투표: {wall}";
    }

    public void UpdateVoteTimer(int remaining)
    {
        if (timerText != null)
            timerText.text = $"남은 시간: {remaining}초";
    }

    public void ShowPersonalPerksChoices()
    {
        var stats = GetLocalPlayerStats();
        if (stats == null)
        {
            if (!waitingPersonalPerks)
                StartCoroutine(WaitAndShowPersonalPerks());
            return;
        }

        OpenUI(personalPerksPnl);

        foreach (var btn in personalPerksBtn) btn.onClick.RemoveAllListeners();

        var picks = perksManager.GetRandomPersonalPerks();

        for (int i = 0; i < personalPerksBtn.Length && i < picks.Count; i++)
        {
            var picked = picks[i];

            if (essenceIconPath.TryGetValue(picked, out var path))
                SetButtonIcon(personalPerksBtn[i], path);

            // 캡처 변수 주의
            var captured = picked;
            personalPerksBtn[i].onClick.AddListener(() => OnSelectPersonalPerks(captured));
        }
        if (reviveVoteText == null || buffVoteText == null || wallVoteText == null)
            Debug.LogError("[UIManager] 투표 텍스트 연결 누락!");
    }

    // 현재 웨이브 / 췌대 웨이브 UI 갱신
    public void UpdateWaveUI(int current, int max)
    {
        string msg = $"WAVE {current} / {max}";

        if (ingameWaveText) ingameWaveText.text = msg;
        //if (waveClearWaveText) waveClearWaveText.text = msg;
        //if (gameOverWaveText) gameOverWaveText.text = msg;
        //if (gameClearWaveText) gameClearWaveText.text = msg;
    }
    IEnumerator WaitAndShowPersonalPerks()
    {
        waitingPersonalPerks = true;
        float t = 0f;
        while (GetLocalPlayerStats() == null && t < 3f)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        waitingPersonalPerks = false;

        if (GetLocalPlayerStats() == null)
        {
            Debug.LogWarning("[UI] PlayerStatsRuntime 찾을 수 없음");
            yield break;
        }
        ShowPersonalPerksChoices();
    }
    void OnSelectPersonalPerks(EssenceType type)
    {
        CloseUI(personalPerksPnl);
        perksManager.ApplyPersonalPerks(type); // 로컬 플레이어 자동 적용 오버로드 사용
        Debug.Log("개인 특전 선택");
        ShowShopUI();
    }

    public void ShowShopUI() => OpenUI(shopPanel);
    public void CloseShopUI() => CloseUI(shopPanel);

    public void UpdateTeamResourceUI(int value)
    {
        Debug.Log($"[UI] 팀 재화 : {value}");
    }

    public void UpdatePersonalResourceUI(string nickname, int value)
    {
        if (PhotonNetwork.player.NickName != nickname)
        return;

        Debug.Log($"[UI] 개인재화 : {value}");

        if (personalCoinText != null)
            personalCoinText.text = $"COIN {value}";
    }

    private void SetButtonIcon(Button button, string resPath)
    {
        if (button == null)
        {
            Debug.LogWarning("[UI] Button null");
            return;
        }

        // 버튼 하위의 첫 번째 Image 사용 (필요하면 "Icon" 이름으로 정확히 찾게 수정 가능)
        Image target = button.GetComponentInChildren<Image>(includeInactive: true);
        Sprite sp = Resources.Load<Sprite>(resPath);

        if (target != null && sp != null)
        {
            target.sprite = sp;
            target.preserveAspect = true;
            target.color = Color.white;
        }
        else
        {
            Debug.LogWarning($"[UI] 아이콘 로드 실패: {resPath} (target:{target != null}, sprite:{sp != null})");
        }
    }

    PlayerStats GetLocalPlayerStats()
    {
        foreach (var pc in FindObjectsOfType<PlayerController>())
        {
            var pv = pc.GetComponent<PhotonView>();
            if (pv != null && pv.isMine)
                return pc.GetComponent<PlayerStats>();
        }
        return null;
    }
    public void SetCamera(CameraFollow cam)
    {
        cameraFollow = cam;
    }

    
    
    public void ReleaseSystem()
    {
        foreach (var p in uiPanels)
            if (p != null) p.SetActive(false);

        if (hudRoot != null) hudRoot.SetActive(true);
        Debug.Log("[UIManager] 초기화 완료");
    }

    public void ShowWaveClearPanel()
    {
        if (waveClearPanel)
            OpenUI(waveClearPanel);
    }

    public void ShowGameClearPanel()
    {
        if (gameClearPanel)
        {
            //  부모가 비활성화되어 있으면 같이 켜기
            var parent = gameClearPanel.transform.parent;
            if (parent != null && !parent.gameObject.activeSelf)
                parent.gameObject.SetActive(true);

            OpenUI(gameClearPanel);
            Debug.Log("[UIManager] GameClearPanel 표시 완료");
        }
        else
        {
            Debug.LogError("[UIManager] gameClearPanel이 연결되어 있지 않습니다!");
        }
    }


    public void ShowGameOverPanel()
    {
        if (gameOverPanel == null)
        {
            Debug.LogWarning("[UIManager] gameOverPanel이 없습니다.");
            return;
        }

        // UIManager를 통해 열기 → UpdateUIState() 호출 → HUD 자동 숨김 + 마우스 활성화
        OpenUI(gameOverPanel);

    }
    public void ShowWaveStart()
    {
        StartCoroutine(ShowTempPanel(waveStartPanel, 2f));
    }

    public void ShowWaveClear()
    {
        StartCoroutine(ShowTempPanel(waveClearPanel, 2f));
    }

    IEnumerator ShowTempPanel(GameObject panel,float duration)
    {
        if (!panel) yield break;

        panel.SetActive(true);
        yield return new WaitForSeconds(duration);
        panel.SetActive(false);
    }

    public void ToggleOptionPanel()
    {
        if (!optionPanel) return;

        bool willOpen = !optionPanel.activeSelf;

        if (willOpen)
            OpenUI(optionPanel);
        else
            CloseUI(optionPanel);

        // 시간 멈추고 싶으면 여기서 조절
        Time.timeScale = willOpen ? 0f : 1f;
    }
    // 옵션창의 "로비로" 버튼에 연결
    public void OnClickGoLobbyFromOption()
    {
        Time.timeScale = 1f;

        // 1️ 시스템 해제
        if (GameManager.Instance != null)
            GameManager.Instance.ReleaseAllSystems();

        // 2️ 커스텀 프로퍼티 초기화
        if (PhotonNetwork.player != null)
        {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash["SelectedTitan"] = null;
            hash["TeamIndex"] = null;
            hash["TeamSlot"] = null;
            PhotonNetwork.player.SetCustomProperties(hash);
        }

        // 3️ 방 나가기 → 로비 로드
        if (PhotonNetwork.inRoom)
            StartCoroutine(Co_LeaveRoomAndGoLobby());
        else
            SceneManager.LoadScene("Lobby");
    }

    IEnumerator Co_LeaveRoomAndGoLobby()
    {
        PhotonNetwork.LeaveRoom();
        while (PhotonNetwork.inRoom)
            yield return null;
        SceneManager.LoadScene("Lobby");
    }


    // 옵션창의 "게임 종료" 버튼에 연결
    public void OnClickQuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }


    public void ReturnToLobby()
    {
        Time.timeScale = 1f;   // 다시 정상 시간으로 돌려놓고

        // 멀티플레이 기준: 포톤 방에서 나가지 않고 바로 로비 씬으로
        PhotonNetwork.LoadLevel(lobbySceneName);

        // 싱글용이라면 대신:
        // SceneManager.LoadScene(lobbySceneName);
    }


    public void OnClickGoLobbyFromClear()
    {
        // 버튼에서는 코루틴만 시작
        StartCoroutine(Co_GoLobbyFromClear());
    }

    //  실제 처리 로직
    IEnumerator Co_GoLobbyFromClear()
    {
        Time.timeScale = 1f;

        // 1) 인벤토리 DB 저장 먼저
        var invSync = FindObjectOfType<InventoryDBSync>();
        if (invSync != null)
        {
            Debug.Log("[UIManager] 인벤토리 저장 시작");
            yield return StartCoroutine(invSync.SaveInventoryCoroutine());
            Debug.Log("[UIManager] 인벤토리 저장 완료");
        }
        else
        {
            Debug.LogWarning("[UIManager] InventoryDBSync를 찾지 못했습니다. 저장 생략");
        }

        // 2) 시스템 해제
        if (GameManager.Instance != null)
            GameManager.Instance.ReleaseAllSystems();

        // 3) 커스텀 프로퍼티 초기화
        if (PhotonNetwork.player != null)
        {
            Hashtable hash = new Hashtable();
            hash["SelectedTitan"] = null;
            PhotonNetwork.player.SetCustomProperties(hash);
        }

        // 4) 방 나가기 → 로비 이동
        if (PhotonNetwork.inRoom)
        {
            yield return StartCoroutine(Co_LeaveRoomAndGoLobby());
        }
        else
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }



    public void UpdateSystem()
    {
        if (voteManager == null)
        {
            voteManager = FindObjectOfType<PerksVoteManager>();
            if (voteManager != null)
            {
                Debug.Log("[UIManager] PerksVoteManager 연결 완료");
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (inventoryPanel == null)
            {
                Debug.LogWarning("[UI] InventoryPanel이 연결되지 않음");
                return;
            }

            bool isActive = inventoryPanel.activeSelf;

            // 인벤토리 열고 닫기
            if (isActive)
                CloseUI(inventoryPanel);
            else
                OpenUI(inventoryPanel);

            // HUD 토글 추가
            //if (hudRoot != null)
            //    hudRoot.SetActive(isActive);  // 인벤토리가 열리면 HUD 비활성, 닫히면 다시 활성
        }
        //  ESC 옵션창 토글 추가
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptionPanel();
        }


    }

    // 게임 오버창의 "로비로" 버튼에 연결
    public void OnClickGoLobbyFromGameOver()
    {
        StartCoroutine(Co_GoLobbyFromGameOver());
    }

    IEnumerator Co_GoLobbyFromGameOver()
    {
        Time.timeScale = 1f;

        // 1) 시스템 해제
        if (GameManager.Instance != null)
            GameManager.Instance.ReleaseAllSystems();

        // 2) 커스텀 프로퍼티 초기화 (필요한 만큼만)
        if (PhotonNetwork.player != null)
        {
            Hashtable hash = new Hashtable();
            hash["SelectedTitan"] = null;
            PhotonNetwork.player.SetCustomProperties(hash);
        }

        // 3) 방 나가기 → 로비 이동 (이미 있는 코루틴 재사용)
        if (PhotonNetwork.inRoom)
        {
            yield return StartCoroutine(Co_LeaveRoomAndGoLobby());
        }
        else
        {
            SceneManager.LoadScene("LobbyScene");
        }
    }
}
