using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;

    void Start()
    {
        //Defaults the volume to max, or else runs the loading phase when an update to the slider is detected
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }

        else
        {
            Load();
        }
    }

    public void ChangeVolume()
    {
        //Sets the volume of the audio the user hears to the value on the slider
        AudioListener.volume = volumeSlider.value;
        Save();
    }

    private void Load()
    {
        //Gets the current value for an updated volume slider
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }

    private void Save()
    {
        //Changes every volume slider to the same value, when one is updated they all are
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }
}
