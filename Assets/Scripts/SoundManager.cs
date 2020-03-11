using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { set; get; }

    public AudioSource bgmAudioSource;
    public AudioSource seAudioSource;
    public AudioClip[] bgmAudioClip;
    public AudioClip[] seAudioClip;

    private int m_BgmPlayIndex = -1;

    public static int SE_BUTTON_CLICK = 0;
    public static int SE_COIN = 1;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }

    public void PlayBGM(int nBgmIndex)
    {
        if (m_BgmPlayIndex == nBgmIndex)
            return;
        m_BgmPlayIndex = nBgmIndex;

        bgmAudioSource.Stop();
        bgmAudioSource.clip = bgmAudioClip[m_BgmPlayIndex];
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }

    public void SetBgmEnable(bool bEnable)
    {
        if (bEnable)
            bgmAudioSource.Play();
        else
            bgmAudioSource.Stop();
    }

    public void PlaySE(int nSeIndex)
    {
        if (!AppManager.Instance.bSound)
            return;
        seAudioSource.clip = seAudioClip[nSeIndex];
        seAudioSource.loop = false;
        seAudioSource.Play();
    }
}
