using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{

    private AudioSource audioSource;
    public AudioClip showdownGunSounds;
    public AudioClip GunShotSound;
    void Start()
  {
    audioSource = GetComponent<AudioSource>();
    PlayMusic();
  }
  
  public void PlayMusic()
  {
    if (!audioSource.isPlaying)
    {
      audioSource.Play();
    }
  }
  
  public void StopMusic()
  {
    if (audioSource.isPlaying)
    {
      audioSource.Stop();
    }
  }

  public void PlayGunShot()
  {
    StopMusic();
    audioSource.PlayOneShot(showdownGunSounds);
  }
}
