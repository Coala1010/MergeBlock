using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Facebook.Unity;

public class LoadingSceneMgr : MonoBehaviour
{
    [SerializeField]
    UILabel _loadingLabel;

    public GameObject loadingProgressObj;
    float loadingProgress = 0f;
    bool isLoading;
    string defaultLabelText;

    // Start is called before the first frame update
    void Start()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();

            isLoading = true;
        }
        else
        {
            defaultLabelText = _loadingLabel.text;
            _loadingLabel.text = "Initializing analytics..";

            //Handle FB.Init
            FB.Init(() =>
            {
                _loadingLabel.text = defaultLabelText;

                FB.ActivateApp();

                isLoading = true;
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLoading)
        {
            if (loadingProgress < 1.0f)
            {
                loadingProgress += (Time.deltaTime / 2);
                loadingProgressObj.GetComponent<UITexture>().fillAmount = loadingProgress;
            }
            else if (loadingProgress > 1.0f)
            {
                loadingProgress = 1.0f;
                loadingProgressObj.GetComponent<UITexture>().fillAmount = loadingProgress;
            }
            else
                SceneManager.LoadScene("MainScene");
        }
    }
}
