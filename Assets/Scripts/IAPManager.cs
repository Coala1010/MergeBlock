using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { set; get; }

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    public string COIN_200 = "coin200";
    public string COIN_750 = "coin750";
    public string COIN_1200 = "coin1200";
    public string COIN_2550 = "coin2550";
    public string COIN_5500 = "coin5500";
    public string COIN_14500 = "coin14500";
    public string NO_ADS = "noads";

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add a product to sell / restore by way of its identifier, associating the general identifier
        // with its store-specific identifiers.
        builder.AddProduct(COIN_200, ProductType.Consumable);
        builder.AddProduct(COIN_750, ProductType.Consumable);
        builder.AddProduct(COIN_1200, ProductType.Consumable);
        builder.AddProduct(COIN_2550, ProductType.Consumable);
        builder.AddProduct(COIN_5500, ProductType.Consumable);
        builder.AddProduct(COIN_14500, ProductType.Consumable);
        // Continue adding the non-consumable product.
        builder.AddProduct(NO_ADS, ProductType.NonConsumable);

        // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
        // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
        UnityPurchasing.Initialize(this, builder);
    }


    public bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void BuyCoin200()
    {
        BuyProductID(COIN_200);
    }

    public void BuyCoin750()
    {
        BuyProductID(COIN_750);
    }

    public void BuyCoin1200()
    {
        BuyProductID(COIN_1200);
    }

    public void BuyCoin2550()
    {
        BuyProductID(COIN_2550);
    }

    public void BuyCoin5500()
    {
        BuyProductID(COIN_5500);
    }

    public void BuyCoin14500()
    {
        BuyProductID(COIN_14500);
    }

    public void BuyNoAds()
    {
        BuyProductID(NO_ADS);
    }

    public string GetProducePriceFromStore(string id)
    {
        if (m_StoreController != null && m_StoreController.products != null)
            return m_StoreController.products.WithID(id).metadata.localizedPriceString;
        else
            return "";
    }

    private void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);

            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
    // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
            // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) => {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                // no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }


    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, COIN_200, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(200);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_200, 0.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, COIN_750, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(750);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_750, 2.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, COIN_1200, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(1200);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_1200, 4.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, COIN_2550, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(2550);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_2550, 9.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, COIN_5500, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(5500);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_5500, 19.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, COIN_14500, StringComparison.Ordinal))
        {
            MainPanelMgr.Instance.AddCoin(14500);
            AppManager.Instance.Record_Purchase_AppEvent(COIN_14500, 39.99f, 200);
        }
        else if (String.Equals(args.purchasedProduct.definition.id, NO_ADS, StringComparison.Ordinal))
        {
            AppManager.Instance.RemoveAds();
            AppManager.Instance.Record_Purchase_AppEvent(NO_ADS, 3.99f, 1);
        }
        // Or ... an unknown product has been purchased by this user. Fill in additional products here....
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
        }

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }

    public bool isBoughtNoAds()
    {
        Product product = m_StoreController.products.WithID(NO_ADS);
        if (product != null && product.hasReceipt)
        {
            // Owned Non Consumables and Subscriptions should always have receipts.
            // So here the Non Consumable product has already been bought.
            return true;
        }
        return false;
    }
}
