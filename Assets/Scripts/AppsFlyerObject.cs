using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AppsFlyerObject : MonoBehaviour
{
    // const string appsFlyerDevKey = "RsawgcqZtGbNorw8pZ2SCT"; // developer's test app devKey
    const string appsFlyerDevKey = "ewVfXy4eavTcRaRzrsKWAA"; // Vladimir's devKey

    void Start()
    {
        /* Mandatory - set your AppsFlyer’s Developer key. */
        AppsFlyer.setAppsFlyerKey(appsFlyerDevKey);
        /* For detailed logging */
        /* AppsFlyer.setIsDebug (true); */
#if UNITY_IOS
      /* Mandatory - set your apple app ID
      NOTE: You should enter the number only and not the "ID" prefix */
      AppsFlyer.setAppID ("YOUR_APP_ID_HERE");
      AppsFlyer.getConversionData();
      AppsFlyer.trackAppLaunch ();
#elif UNITY_ANDROID
        /* For getting the conversion data in Android, you need to add the "AppsFlyerTrackerCallbacks" listener.*/
        AppsFlyer.init(appsFlyerDevKey, "AppsFlyerTrackerCallbacks");
#endif
    }
}
