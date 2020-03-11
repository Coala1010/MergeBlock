using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppsFlyerTrackerCallbacks : MonoBehaviour
{
    // const string appsFlyerDevKey = "RsawgcqZtGbNorw8pZ2SCT"; // developer's test app devKey
    const string appsFlyerDevKey = "ewVfXy4eavTcRaRzrsKWAA"; // Vladimir's devKey

    // Start is called before the first frame update
    void Start()
    {
        /*To get conversion data for Android, add this listener to the init() method*/
        AppsFlyer.init(appsFlyerDevKey, "AppsFlyerTrackerCallbacks");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
