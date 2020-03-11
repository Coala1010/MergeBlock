//#define DEV_TEST_MODE
using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    public static AppManager Instance { set; get; }

    public GameObject shopPanelObj;

    public bool bShowAds;
    public bool bMusic, bSound, bNotification; // Game Settings

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        FB.Init();
        DeleteAllPlayerPrefs();
        bMusic = PlayerPrefs.GetInt("Bgm", 1) == 1;
        bSound = PlayerPrefs.GetInt("Sound", 1) == 1;
        bShowAds = PlayerPrefs.GetInt("ShowAds", 1) == 1;
        PlayBgm();
        SoundManager.Instance.SetBgmEnable(bMusic);
        if (bShowAds)
            StartCoroutine(IsBoughtNoAds());
    }

    private void PlayBgm()
    {
        SoundManager.Instance.PlayBGM(0);
    }

    public void RemoveAds()
    {
        bShowAds = false;
        PlayerPrefs.SetInt("ShowAds", 0);
        shopPanelObj.GetComponent<ShopPanelMgr>().InitUI();
    }

    private IEnumerator IsBoughtNoAds()
    {
        while (!IAPManager.Instance.IsInitialized())
            yield return null;

        bShowAds = !IAPManager.Instance.isBoughtNoAds();
        if (!bShowAds)
            PlayerPrefs.SetInt("ShowAds", 0);
    }

    public void DeleteAllPlayerPrefs()
    {
#if DEV_TEST_MODE
        PlayerPrefs.DeleteAll();
#endif
    }

    public void Record_ResumeGame_AppEvent(string eventStr, string strReason)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.PARAM_1, strReason);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Reason", strReason);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_ResumeGame_AppEvent(string eventStr, int nGameNumber)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.PARAM_1, "Reached at " + nGameNumber);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Reached at", nGameNumber);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_ReachedBigNumber_AppEvent(string eventStr, int nGameNumber)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.PARAM_1, "Reached at " + nGameNumber);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Reached at", nGameNumber);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_StarHelp_AppEvent(string eventStr, int nStarHelp, int nGameCoin, int nGameComplete20)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.QUANTITY, "Current starhelp: " + nStarHelp);
        gameEvent.Add(AFInAppEvents.PARAM_1, "game coin: " + nGameCoin);
        gameEvent.Add(AFInAppEvents.PARAM_2, "game complete20: " + nGameComplete20);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Current starhelp", nStarHelp);
        fb_gameEvent.Add("game coin", nGameCoin);
        fb_gameEvent.Add("game complete20", nGameComplete20);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_GetCoin_AppEvent(string eventStr, int nCurrentCoin, int nAmount)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.QUANTITY, "Current coin: " + nCurrentCoin);
        gameEvent.Add(AFInAppEvents.PARAM_1, "Get (new) coin: " + nAmount);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Current coin", nCurrentCoin);
        fb_gameEvent.Add("Get(new) coin", nAmount);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_GameStart_AppEvent(string eventStr, int nStartLevel, int nAmount)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.LEVEL, "Start Level " + nStartLevel);
        gameEvent.Add(AFInAppEvents.QUANTITY, "Current coin: " + nAmount);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("af_"+eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Start Level", nStartLevel);
        fb_gameEvent.Add("Current coin", nAmount);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_GameRestart_AppEvent(string eventStr, int nStartLevel, int nAmount)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.LEVEL, "Start Level " + nStartLevel);
        gameEvent.Add(AFInAppEvents.PARAM_1, "Current coin: " + nAmount);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Start Level", nStartLevel);
        fb_gameEvent.Add("Current coin", nAmount);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_GameTutorial_AppEvent(string eventStr)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_GameOver_AppEvent(string eventStr, int nStartLevel, int nComplete20, int nGameCoin, int nGameScore, int nBestScore)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.LEVEL, "Start Level: " + nStartLevel);
        gameEvent.Add(AFInAppEvents.PARAM_1, "Complete20: " + nComplete20);
        gameEvent.Add(AFInAppEvents.PARAM_2, "GameCoin: " + nGameCoin);
        gameEvent.Add(AFInAppEvents.QUANTITY, "Total: " + (nComplete20 + nGameCoin));
        gameEvent.Add(AFInAppEvents.PARAM_3, "GameScore: " + nGameScore);
        gameEvent.Add(AFInAppEvents.PARAM_4, "BestScore: " + nBestScore);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Start Level", nStartLevel);
        fb_gameEvent.Add("Complete20", nComplete20);
        fb_gameEvent.Add("GameCoin", nGameCoin);
        fb_gameEvent.Add("Total", (nComplete20 + nGameCoin));
        fb_gameEvent.Add("GameScore", nGameScore);
        fb_gameEvent.Add("BestScore", nBestScore);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_ShowAds_AppEvent(string eventStr, string adType, string adTiming)
    {
        Dictionary<string, string> gameEvent = new Dictionary<string, string>();
        gameEvent.Add(AFInAppEvents.PARAM_1, adType);
        gameEvent.Add(AFInAppEvents.PARAM_2, adTiming);
        gameEvent.Add(AFInAppEvents.CONTENT_TYPE, eventStr);
        AppsFlyer.trackRichEvent("mb_" + eventStr, gameEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Ads Type", adType);
        fb_gameEvent.Add("Ads Timing", adTiming);
        fb_gameEvent.Add("Event Name", eventStr);
        FB.LogAppEvent("fb_" + eventStr, null, fb_gameEvent);
    }

    public void Record_Purchase_AppEvent(string eventStr, float price, int amount)
    {
        Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
        purchaseEvent.Add(AFInAppEvents.CURRENCY, "USD");
        purchaseEvent.Add(AFInAppEvents.REVENUE, price.ToString());
        purchaseEvent.Add(AFInAppEvents.QUANTITY, "Bought " + amount + " coins");
        purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "purchase_" + eventStr);
        AppsFlyer.trackRichEvent("mb_purchase", purchaseEvent);

        Dictionary<string, object> fb_gameEvent = new Dictionary<string, object>();
        fb_gameEvent.Add("Price", price + "$");
        fb_gameEvent.Add("Quantity", "Bought " + amount + " coins");
        fb_gameEvent.Add("Event Name", "purchase_" + eventStr);
        FB.LogAppEvent("fb_purchase", null, fb_gameEvent);
    }
}
