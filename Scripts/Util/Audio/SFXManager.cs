using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;
    public AudioMixerGroup sfxGroup;

    [System.Serializable]
    public class SFX
    {
        public string key;
        public AudioClip clip;
        public float volume = 1f;
    }
    public List<SFX> list=new();
    Dictionary<string, SFX> map = new();

    AudioSource source;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup=sfxGroup;

        foreach(var s in list)
        map[s.key] = s;


    }
    
    public void PlaySFX(string key)
    {
        if(!map.ContainsKey(key)) return;
        var s = map[key];
        source.PlayOneShot(s.clip, s.volume);
    }
}
