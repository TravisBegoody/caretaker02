using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer mixer;

    public Sound[] Sounds;

    public float ambienceMultiplier = 1f;
    public float sfxMultiplier;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject); //Avoids loading new managers
            return;
        }
        Instance = this;

        foreach (Sound s in Sounds)
        {
            GameObject temp = new GameObject(s.name);
            temp.transform.parent = this.gameObject.transform;

            s.source = temp.AddComponent<AudioSource>();
            s.source.playOnAwake = false;
            s.soundObject = temp;
            s.source.clip = s.clip;
            s.source.outputAudioMixerGroup = s.group;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = s.spatialblend;

            s.source.loop = s.loop;
        }
        DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        StartCoroutine(PlayRandomAmbience());
    }
    public void Play(string name,GameObject source)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
            return;
        Debug.Log("Playing " + s.name + " at request of " + source);
        s.source.Play();

        if(s.StopAfter)
        {
            StartCoroutine(PlayDuration(s));
        }
    }
    public void PlayAt(string name, Vector3 position)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.soundObject.transform.position = position;
        if (s.StopAfter)
        {
            StartCoroutine(PlayDuration(s));
        }
    }
    public void CheckPlay(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
            return;

        //If not already playing then it plays
        if (!s.source.isPlaying)
        {
            Debug.Log("Playing " + name + " Already Running = " + s.source.isPlaying);
            s.source.Play();
            if (s.StopAfter)
            {
                StartCoroutine(PlayDuration(s));
            }
        }
    }
    public void CheckPlayAt(string name, Vector3 position)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.soundObject.transform.position = position;
        if (!s.source.isPlaying)
        {
            s.source.Play();
            if (s.StopAfter)
            {
                StartCoroutine(PlayDuration(s));
            }
        }
    }
    public void PauseAll()
    {
        foreach (Sound s in Sounds)
        {
            s.source.Pause();
        }
    }
    public void ResumeAll()
    {
        foreach (Sound s in Sounds)
        {
            s.source.UnPause();
        }
    }
    public void Stop(string name)
    {
        Sound s = Array.Find(Sounds, sound => sound.name == name);
        if (s == null)
            return;
        s.source.Stop();
    }
    //Stops all running sounds
    public void StopAll()
    {
        foreach (Sound s in Sounds)
        {
            s.source.Stop();
        }
    }
    public void AmbienceVolumeChange()
    {
        //Log10 makes a value of -80 to 0 on a logarithmic scale
        mixer.SetFloat("AmbienceVol", Mathf.Log10(ambienceMultiplier) * 20);
    }
    public void SFXVolumeChange()
    {
        //Log10 makes a value of -80 to 0 on a logarithmic scale
        mixer.SetFloat("SFXVol", Mathf.Log10(ambienceMultiplier) * 20);
    }
    IEnumerator PlayDuration(Sound s)
    {
        yield return new WaitForSeconds(s.duration);
        s.source.Stop();
    }
    IEnumerator PlayRandomAmbience()
    {
        int index = UnityEngine.Random.Range(0, 4);
        yield return new WaitForSeconds(UnityEngine.Random.Range(25f, 50f));

        Sounds[index].soundObject.transform.position = UnityEngine.Random.insideUnitSphere * 100f;
        Sounds[index].source.Play();
        Debug.Log("Playing" + Sounds[index].source.name);
        StartCoroutine(PlayRandomAmbience());
    }
}
[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;
    public AudioMixerGroup group;

    [Range(0f, 1f)]
    public float volume;

    [Range(0.1f,3f)]
    public float pitch;
    [Range(0f, 1f)]
    public float spatialblend;
    //Stops after certain duration
    public bool StopAfter;
    public float duration;
    [HideInInspector]
    public AudioSource source;
    [HideInInspector]
    public GameObject soundObject;

    public bool loop;
}
