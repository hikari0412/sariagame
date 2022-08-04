using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SimpleUI : MonoBehaviour
{
    public AudioMixer audioMixerUI;
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void BGMChange(float value)
    {
        audioMixerUI.SetFloat("Vol_BGM", value);
    }
    public void MasterChange(float value)
    {
        audioMixerUI.SetFloat("Vol_Master", value);
    }
    public void SoundEffectChange(float value)
    {
        audioMixerUI.SetFloat("Vol_SoundEffect", value);
    }
    public void VoiceChange(float value)
    {
        audioMixerUI.SetFloat("Vol_Voice", value);
    }
}
