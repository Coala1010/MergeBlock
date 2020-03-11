using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPanelMgr : MonoBehaviour
{
    public static MainPanelMgr Instance { set; get; }
    public GameObject gameplayPanelObj;
    public GameObject settingPanelObj;
    public GameObject shopPanelObj;
    public GameObject slidePanelObj;
    public GameObject colorBgObj;
    public GameObject startLevelObj;
    public GameObject playBtnObj;
    public GameObject settingBtnObj;
    public GameObject cartBtnObj;
    public GameObject coinObj;
    public GameObject starLogoObj1;
    public GameObject starLogoObj2;
    public GameObject warnTextObj;

    public int nLevelUnlocked = 0;
    public int nStartLevel = 0;
    public int nCoin;
    public int nBestScore;

    public bool isTutorial;
    bool bBlockTouch;
    public int nGamePlayStatus;

    private void Awake()
    {
        Instance = this;
        nCoin = PlayerPrefs.GetInt("Coin", 0);
        nBestScore = PlayerPrefs.GetInt("BestScore", 0);
        nStartLevel = PlayerPrefs.GetInt("StartLevel", 0);
        nLevelUnlocked = PlayerPrefs.GetInt("UnlockLevel", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        SendRetentionEventsIfExist();

        nGamePlayStatus = PlayerPrefs.GetInt("GamePlayStatus", 0);
        PlayerPrefs.SetInt("GamePlayStatus", 0);
        if (nGamePlayStatus > 0)
        {
            // Debug.Log("GamePlayStatus " + nGamePlayStatus);
            if (nGamePlayStatus == 1)
                isTutorial = true;
            else if (nGamePlayStatus == 2)
                isTutorial = false;
            ResumeGamePlayPanel();
        }
        else
            StartCoroutine(CloseSlide());
    }

    void SendRetentionEventsIfExist()
    {
        string installDateData = PlayerPrefs.GetString("install_date", string.Empty);

        if (string.IsNullOrEmpty(installDateData))
        {
            System.DateTime dateNow = System.DateTime.UtcNow;
            installDateData = dateNow.ToShortDateString();
            PlayerPrefs.SetString("install_date", installDateData);
        }
        else
        {
            System.DateTime installDate = System.DateTime.ParseExact(installDateData, "MM/dd/yyyy", null);

            bool retentionD3 = PlayerPrefs.GetInt("d3_sent", 0) == 1;
            bool retentionD10 = PlayerPrefs.GetInt("d10_sent", 0) == 1;

            int dayDelta = 0;
            System.DateTime dateNow = System.DateTime.UtcNow;
            //dateNow = new System.DateTime(2021, 1, 1); //For test
            int deltaDays = dateNow.Subtract(installDate).Days;

            if (!retentionD10 && deltaDays >= 10)
            {
                PlayerPrefs.SetInt("d10_sent", 1);
                dayDelta = 10;
            }
            else if (!retentionD3 && deltaDays >= 3 && deltaDays < 10)
            {
                PlayerPrefs.SetInt("d3_sent", 1);
                dayDelta = 3;
            }
            if (dayDelta != 0)
            {
                Facebook.Unity.FB.LogAppEvent($"{dayDelta}_retention", null);
                Dictionary<string, string> richData = new Dictionary<string, string>() { { "RetentionDay", $"{dayDelta}_retention" } };
#if UNITY_ANDROID
                richData.Add("Platform", "Android");
#elif UNITY_IOS
                richData.Add("Platform", "iOS");
#endif
                AppsFlyer.trackRichEvent($"{dayDelta}_retention", richData);

                Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

                    Firebase.Analytics.FirebaseAnalytics.LogEvent($"d_{dayDelta}");
                });

            }
        }
    }

    private void OnEnable()
    {
        warnTextObj.SetActive(false);
        coinObj.GetComponent<UILabel>().text = nCoin.ToString();
        Init_UI();
    }

    public void onPlayBtnClicked()
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        if (nStartLevel > nLevelUnlocked)
            return;
        if (nCoin < 2 * nStartLevel)
        {
            Debug.Log("Insufficient Coins");
            shopPanelObj.SetActive(true);
        }
        else
        {
            nCoin -= 2 * nStartLevel;
            isTutorial = false;
            nGamePlayStatus = PlayerPrefs.GetInt("GamePlayStatus", 0);
            AppManager.Instance.Record_GameStart_AppEvent("game_started", nStartLevel + 1, nCoin);
            GotoGamePlayPanel();
        }
    }

    void ResumeGamePlayPanel()
    {
        StartCoroutine(_ResumeGamePlayPanel(gameplayPanelObj));
    }

    IEnumerator _ResumeGamePlayPanel(GameObject targetPanel)
    {
        targetPanel.SetActive(true);
        gameObject.GetComponent<UIPanel>().alpha = 0;

        slidePanelObj.SetActive(true);
        slidePanelObj.GetComponent<Animator>().Play("CloseSlide");

        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        gameObject.GetComponent<UIPanel>().alpha = 1;
        gameObject.SetActive(false);
        slidePanelObj.SetActive(false);
    }

    public void GotoGamePlayPanel()
    {
        StartCoroutine(SwitchToOtherPanel(gameplayPanelObj));
    }

    IEnumerator SwitchToOtherPanel(GameObject targetPanel)
    {
        slidePanelObj.SetActive(true);
        slidePanelObj.GetComponent<Animator>().Play("OpenSlide");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();

        targetPanel.SetActive(true);
        gameObject.GetComponent<UIPanel>().alpha = 0;
        slidePanelObj.GetComponent<Animator>().Play("CloseSlide");

        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        gameObject.GetComponent<UIPanel>().alpha = 1;
        gameObject.SetActive(false);
        slidePanelObj.SetActive(false);
    }

    IEnumerator CloseSlide()
    {
        slidePanelObj.SetActive(true);
        slidePanelObj.GetComponent<Animator>().Play("CloseSlide");
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        slidePanelObj.SetActive(false);
    }

    public void onStartLevelBtnClicked(GameObject obj)
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        int nObj = int.Parse(obj.name);
        if (nObj == nStartLevel)
            return;
        if (nObj > nLevelUnlocked)
        {
            StartCoroutine(PlayWarnText(nObj));
            return;
        }
        starLogoObj1.GetComponent<Animator>().Play(0);
        starLogoObj2.GetComponent<Animator>().Play(0);
        nStartLevel = nObj;
        PlayerPrefs.SetInt("StartLevel", nStartLevel);
        Init_UI();
    }

    public void onSettingBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(YouClickedBtn());
        StartCoroutine(OpenSettingPanel());
    }

    IEnumerator OpenSettingPanel()
    {
        settingPanelObj.SetActive(true);
        settingPanelObj.GetComponent<Animator>().Play("OpenWinAnim");
        yield return new WaitForSeconds(0.17f);
        settingPanelObj.transform.GetChild(0).gameObject.SetActive(false);
        settingPanelObj.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void onCartBtnClicked()
    {
        if (bBlockTouch)
            return;
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);
        StartCoroutine(YouClickedBtn());
        shopPanelObj.SetActive(true);
        shopPanelObj.GetComponent<Animator>().Play("OpenWinAnim");
    }

    void Init_UI()
    {
        starLogoObj1.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/Number" + ((nStartLevel + 1) * 5) + "_1");
        starLogoObj2.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/Number" + ((nStartLevel + 1) * 5) + "_2");
        settingBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/SettingBtn_" + nStartLevel);
        cartBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/CartBtn_" + nStartLevel);
        colorBgObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/Back" + nStartLevel);
        startLevelObj.transform.GetChild(1).gameObject.transform.GetChild(0).gameObject.SetActive(nLevelUnlocked < 1);
        startLevelObj.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.SetActive(nLevelUnlocked < 2);
        playBtnObj.transform.GetChild(0).gameObject.SetActive(nStartLevel > 0);
        playBtnObj.transform.GetChild(0).gameObject.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/CoinBg" + nStartLevel);
        if (nStartLevel > 0)
            playBtnObj.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(62, 5, 0);
        else
            playBtnObj.transform.GetChild(1).gameObject.transform.localPosition = new Vector3(0, 5, 0);
        playBtnObj.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = (-2 * nStartLevel).ToString();
        playBtnObj.GetComponent<UITexture>().mainTexture = Resources.Load<Texture>("Images/MainPage/PlayBtnBg" + nStartLevel);
        ShowStartLevelButtonSelectEffect();
    }

    void ShowStartLevelButtonSelectEffect()
    {
        startLevelObj.transform.GetChild(0).gameObject.transform.GetChild(1).gameObject.SetActive(0 == nStartLevel);
        startLevelObj.transform.GetChild(1).gameObject.transform.GetChild(1).gameObject.SetActive(1 == nStartLevel);
        startLevelObj.transform.GetChild(2).gameObject.transform.GetChild(1).gameObject.SetActive(2 == nStartLevel);
    }

    public void AddCoin(int amount)
    {
        nCoin += amount;
        PlayerPrefs.SetInt("Coin", nCoin);
        coinObj.GetComponent<UILabel>().text = nCoin.ToString();
        shopPanelObj.GetComponent<ShopPanelMgr>().InitUI();
    }

    IEnumerator PlayWarnText(int mode)
    {
        warnTextObj.SetActive(false);
        yield return new WaitForEndOfFrame();
        warnTextObj.SetActive(true);
        warnTextObj.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = "Reach " + (mode + 1) * 5 + " Unlock";
        warnTextObj.GetComponent<Animator>().Play(0);
    }

    IEnumerator YouClickedBtn()
    {
        bBlockTouch = true;
        yield return new WaitForSeconds(0.2f);
        bBlockTouch = false;
    }
}
