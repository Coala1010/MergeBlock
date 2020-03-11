using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPanelMgr : MonoBehaviour
{
    public static ShopPanelMgr Instance;
    public GameObject coinObj;
    public GameObject shopItemObj;

    bool bBlockTouch;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        StartCoroutine(LoadPriceRoutine());
    }

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

    public void onBuyBtnClicked(GameObject obj)
    {
        SoundManager.Instance.PlaySE(SoundManager.SE_BUTTON_CLICK);

        if (obj.name.Equals("1"))
            IAPManager.Instance.BuyCoin200();
        else if (obj.name.Equals("2"))
            IAPManager.Instance.BuyCoin750();
        else if (obj.name.Equals("3"))
            IAPManager.Instance.BuyCoin1200();
        else if (obj.name.Equals("4"))
            IAPManager.Instance.BuyCoin2550();
        else if (obj.name.Equals("5"))
            IAPManager.Instance.BuyCoin5500();
        else if (obj.name.Equals("6"))
            IAPManager.Instance.BuyCoin14500();
        else if (obj.name.Equals("7"))
            IAPManager.Instance.BuyNoAds();
    }

    public void InitUI()
    {
        coinObj.GetComponent<UILabel>().text = MainPanelMgr.Instance.nCoin.ToString();
        shopItemObj.transform.GetChild(6).gameObject.SetActive(AppManager.Instance.bShowAds);
        shopItemObj.transform.GetChild(7).gameObject.SetActive(AppManager.Instance.bShowAds);
    }

    private IEnumerator LoadPriceRoutine()
    {
        while (!IAPManager.Instance.IsInitialized())
            yield return null;

        string[] strLoadedPrice = new string[7];
        strLoadedPrice[0] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_200);
        strLoadedPrice[1] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_750);
        strLoadedPrice[2] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_1200);
        strLoadedPrice[3] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_2550);
        strLoadedPrice[4] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_5500);
        strLoadedPrice[5] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.COIN_14500);
        strLoadedPrice[6] = IAPManager.Instance.GetProducePriceFromStore(IAPManager.Instance.NO_ADS);
        for (int i=0; i<7; i++)
        {
            shopItemObj.transform.GetChild(i).gameObject.transform.GetChild(2).gameObject.transform.GetChild(0).gameObject.GetComponent<UILabel>().text = strLoadedPrice[i];
        }
    }
}
