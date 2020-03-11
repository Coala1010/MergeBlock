using System;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;
using UnityEngine;

public class AdManager : MonoBehaviour, IRewardedVideoAdListener, IBannerAdListener
{
    public static AdManager Instance;
    int timesTriedToShowInterstitial = 0;

    // Use this for initialization
    void Start()
    {
        Instance = this;

        Application.targetFrameRate = 60;
        Screen.orientation = ScreenOrientation.Portrait;

#if UNITY_ANDROID
        string appKey = "23f1181b191f34a04f4a74840c09d116028a98ed3721de6b";
#elif UNITY_IOS || UNITY_IPHONE
        string appKey = "0a1d1b96b7fc5ebef29313c625edbce927474670367f23f5";
#endif
        // Appodeal.setTesting(true);
        Appodeal.disableLocationPermissionCheck();
        Appodeal.initialize(appKey, Appodeal.INTERSTITIAL | Appodeal.BANNER_BOTTOM | Appodeal.REWARDED_VIDEO);
        Appodeal.setBannerCallbacks(this);
        Appodeal.setRewardedVideoCallbacks(this);
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause && AppManager.Instance != null && AppManager.Instance.bShowAds)
        {
            ShowInterstitial();
            AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Interstitial", "When back to game");
        }
    }

    public void ShowBanner()
    {
        if (Appodeal.isLoaded(Appodeal.BANNER_BOTTOM))
            Appodeal.show(Appodeal.BANNER_BOTTOM);
    }

    public void HideBanner()
    {
        Appodeal.hide(Appodeal.BANNER);
    }

    public void ShowInterstitial(int nAdTiming = 0)
    {
        timesTriedToShowInterstitial++;
        if (Appodeal.isLoaded(Appodeal.INTERSTITIAL) && timesTriedToShowInterstitial >= 2)
        {
            timesTriedToShowInterstitial = 0;
            if (nAdTiming == 1) // When click pause btn
                AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Interstitial", "When click pause btn");
            else if (nAdTiming == 2) // When reached at number of 15 or 20
                AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Interstitial", "When reached at number of 15 or 20");
            else if (nAdTiming == 3) // Click home btn When game is over
                AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Interstitial", "Click home btn When game is over");
            Appodeal.show(Appodeal.INTERSTITIAL);
        }
    }

    public void ShowRewarded(int nAdTiming, int nCoin)
    {
        if (Appodeal.isLoaded(Appodeal.REWARDED_VIDEO))
        {
            if (nAdTiming == 1) // To get double coin
                AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Rewarded", "To get double coin of " + nCoin);
            else if (nAdTiming == 2) // To continue play game
                AppManager.Instance.Record_ShowAds_AppEvent("ShowAds", "Rewarded", "To continue game");
            Appodeal.show(Appodeal.REWARDED_VIDEO);
        }
    }

    public void onRewardedVideoLoaded(bool precache)
    {
        // throw new NotImplementedException();
    }

    public void onRewardedVideoFinished(double amount, string name)
    {
        // throw new NotImplementedException();
        if (GamePlayMgr.Instance.nRewardVideoStatus == 1)
            ; // GamePlayMgr.Instance.OpenDoubleCoinWin();
        if (GamePlayMgr.Instance.nRewardVideoStatus == 2)
        {
            GamePlayMgr.Instance.ContinueGameWithRemove4Rows();
            AppManager.Instance.Record_ResumeGame_AppEvent("Continue_Game", "by Ads Video");
        }
    }

    public void onRewardedVideoClosed(bool finished)
    {
        // throw new NotImplementedException();
    }

    public void onRewardedVideoExpired()
    {
        // throw new NotImplementedException();
    }

    public void onRewardedVideoClicked()
    {
        // throw new NotImplementedException();
    }

    public void onRewardedVideoFailedToLoad()
    {
    }

    public void onRewardedVideoShown()
    {
    }

    void IBannerAdListener.onBannerLoaded(int height, bool isPrecache)
    {
        ShowBanner();
    }

    void IBannerAdListener.onBannerFailedToLoad()
    {
    }

    void IBannerAdListener.onBannerShown()
    {
    }

    void IBannerAdListener.onBannerClicked()
    {
    }

    void IBannerAdListener.onBannerExpired()
    {
    }
}