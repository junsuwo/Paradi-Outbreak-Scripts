using UnityEngine;

public class ButtonClickManager : MonoBehaviour
{
    public static ButtonClickManager Instance;

    [Header("버튼 클릭 사운드")]
    public AudioSource sfxSource;
    public AudioClip clickClip;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void PlayClick()
    {
        if (clickClip == null || sfxSource == null) return;

        sfxSource.PlayOneShot(clickClip);
    }
}
