using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{
    public GameObject BGM, SFX;
    Slider musicSlider;
    Slider sfxSlider;

    // Start is called before the first frame update
    void Start()
    {
        musicSlider = gameObject.GetComponentsInChildren<Slider>()[0];
        sfxSlider = gameObject.GetComponentsInChildren<Slider>()[1];
        if (musicSlider.name != "MusicSlider")
        {
            Slider temp = musicSlider;
            musicSlider = sfxSlider;
            sfxSlider = musicSlider;
        }

        BGM = GameObject.FindGameObjectWithTag("BGM");
        SFX = GameObject.FindGameObjectWithTag("SFX");

    }

    public void changeMusicVolume()
    {
        BGM.GetComponent<AudioSource>().volume = musicSlider.value;
    }

    public void changeSFXVolume()
    {
        SFX.GetComponent<AudioSource>().volume = sfxSlider.value;
    }
}
