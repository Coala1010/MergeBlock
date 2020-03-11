using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanelMgr : MonoBehaviour
{
    public GameObject musicBtnObj;
    public GameObject soundBtnObj;
    public GameObject notificationBtnObj;

    bool bBlockTouch;

    private void OnEnable()
    {
        InitUI();
    }

    public void onBackBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(ExitWindow());
    }

    IEnumerator ExitWindow()
    {
        bBlockTouch = true;
        transform.GetComponent<Animator>().Play("CloseWinAnim");
        yield return new WaitForSeconds(0.2f);
        bBlockTouch = false;
        gameObject.SetActive(false);
        gameObject.transform.localScale = Vector3.one;
    }

    public void onMusicBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        AppManager.Instance.bMusic = !AppManager.Instance.bMusic;
        SoundManager.Instance.SetBgmEnable(AppManager.Instance.bMusic);
        PlayerPrefs.SetInt("Bgm", AppManager.Instance.bMusic ? 1 : 0);
        InitUI();
    }

    public void onSoundBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        AppManager.Instance.bSound = !AppManager.Instance.bSound;
        PlayerPrefs.SetInt("Sound", AppManager.Instance.bSound ? 1 : 0);
        InitUI();
    }

    public void onNotificationBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        AppManager.Instance.bNotification = !AppManager.Instance.bNotification;
        InitUI();
    }

    public void onContactBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        // Application.OpenURL("http://help.bitmango.com/faq/?adid=3298665b-8dec-4e98-8358-c1f8a6e6fc74&appid=mergeblockstarfinder&app_version=1.3.8&os=Android&pubid=bitmango");
    }

    public void onRateBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
    }

    public void onTutorialBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(ExitWindow());
        MainPanelMgr.Instance.isTutorial = true;
        MainPanelMgr.Instance.nGamePlayStatus = PlayerPrefs.GetInt("GamePlayStatus", 0);
        MainPanelMgr.Instance.GotoGamePlayPanel();
        AppManager.Instance.Record_GameTutorial_AppEvent("Tutorial_started");
    }

    void InitUI()
    {
        string str;
        str = AppManager.Instance.bMusic ? "on" : "off";
        musicBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/Setting/Music_" + str);
        str = AppManager.Instance.bSound ? "on" : "off";
        soundBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/Setting/Sound_" + str);
        str = AppManager.Instance.bNotification ? "on" : "off";
        notificationBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/Setting/Notification_" + str);
    }
}
