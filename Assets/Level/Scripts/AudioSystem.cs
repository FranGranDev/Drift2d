using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSystem : MonoBehaviour
{
    public Sound[] sound;
    public Sound GetSound(string name)
    {
        for(int i = 0; i < sound.Length; i++)
        {
            if (sound[i].name == name)
                return sound[i];
        }
        return null;
    }

    private AudioSource Music;

    public void PlayMusic(string name, float Time)
    {
        if(Music == null)
            Music = gameObject.AddComponent<AudioSource>();
        Sound sound = GetSound(name);
        Music.volume = sound.Volume * GameData.MusicVolume;
        Music.pitch = sound.Pitch;
        Music.clip = sound.Audio;
        Music.loop = sound.loop;
        Music.Play();
        Music.time = Time;
    }
    public void StopMusic()
    {
        if (Music != null)
        {
            Music.Pause();
            GameData.MusicTime = Music.time;
        }
        
    }
    public void ResumeMusic()
    {
        if (Music != null)
        {
            Music.UnPause();
        }
    }
    public float GetMusicTime()
    {
        if (Music != null)
            return Music.time;
        else
            return 0f;
    }
}
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip Audio;
    [Range(0, 1f)]
    public float Volume;
    [Range(0.3f, 5f)]
    public float Pitch;
    public bool loop;
}
