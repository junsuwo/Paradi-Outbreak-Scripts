using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmSource;  // BGM 재생용

    [Header("씬 기본 BGM")]
    public AudioClip sceneBGM;

    [Header("Wave 관련 BGM")]
    public AudioClip wave1ReadyAndPlay;
    public AudioClip wave2BGM;
    public AudioClip wave3BGM;
    public AudioClip wave4BGM;
    public AudioClip wave5BGM;
    public AudioClip betweenWaveBGM;

    [Header("게임 종료 BGM")]
    public AudioClip gameOverBGM;
    public AudioClip gameClearBGM;

    [Header("Mixer")]
    public AudioMixer mixer;

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 씬 변경 이벤트 등록
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Start()
    {
        ApplySavedVolumes();
    }

    public void ApplySavedVolumes()
    {
        float master = PlayerPrefs.GetFloat("vol_master", 1f);
        float bgm = PlayerPrefs.GetFloat("vol_bgm", 1f);
        float sfx = PlayerPrefs.GetFloat("vol_sfx", 1f);

        mixer.SetFloat("MasterVol", (master <= 0.0001f ? -80f : 20f * Mathf.Log10(master)));
        mixer.SetFloat("BGMVol", (bgm <= 0.0001f ? -80f : 20f * Mathf.Log10(bgm)));
        mixer.SetFloat("SFXVol", (sfx <= 0.0001f ? -80f : 20f * Mathf.Log10(sfx)));
    }

    // 씬 변경 시 자동 호출
    private void OnSceneChanged(Scene prev, Scene next)
    {
        string name = next.name;

        if(name == "Login" || name == "User_ID" || name == "Lobby" || name == "Room")
        {
            PlayBGM(sceneBGM);
        }
        else if(name == "CharacterSelect")
        {
            PlayBGM(wave1ReadyAndPlay);
        }

    }

    // =====================================================
    //  공통 BGM 재생 함수
    // =====================================================
    public void PlayBGM(AudioClip clip)
    {
        if(clip==null)return;
        if(bgmSource.clip==clip)return;

        bgmSource.clip=clip;
        bgmSource.Play();
    }

    // =====================================================
    // 웨이브용 BGM
    // =====================================================
    public void PlayWaveBGM(int wave)
    {
        switch (wave)
        {
            case 1:
                PlayBGM(wave1ReadyAndPlay);
                break;
            case 2:
                PlayBGM(wave2BGM);
                break;
            case 3:
                PlayBGM(wave3BGM);
                break;
            case 4:
                PlayBGM(wave4BGM);
                break;
            case 5:
                PlayBGM(wave5BGM);
                break;
        }
    }

    public void PlayBetweenWaveBGM()
    {
        PlayBGM(betweenWaveBGM);
    }

    public void PlayGameOverBGM()
    {
        PlayBGM(gameOverBGM);
    }

    public void PlayGameClearBGM()
    {
        PlayBGM(gameClearBGM);
    }
}
