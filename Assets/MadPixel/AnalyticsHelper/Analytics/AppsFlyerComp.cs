using AppsFlyerSDK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerConnector;
using MadPixel;
using System.Globalization;
using UnityEngine.Serialization;

namespace MadPixelAnalytics {
    public class AppsFlyerComp : MonoBehaviour {
        #region Fields

        [FormerlySerializedAs("bUsePurchaseConnector")]
        [SerializeField]
        private bool m_usePurchaseConnector;

        [FormerlySerializedAs("monetizaionPubKey")]
        [SerializeField, HideInInspector]
        private string m_monetizationPublicKey;

        [Space]
        [Header("Turn Debug OFF for production builds")]
        [SerializeField]
        private bool m_debugMode;

        public bool UseInappConnector => m_usePurchaseConnector;

        #endregion

        #region Init

        public void Init() {
            AppsFlyer.setIsDebug(m_debugMode);

#if UNITY_ANDROID
            AppsFlyer.initSDK(MadPixelCustomSettings.APPSFLYER_SDK_KEY, null, this);
#else
            MadPixelCustomSettings customSettings = AdsManager.LoadMadPixelCustomSettings();
            if (customSettings != null && !string.IsNullOrEmpty(customSettings.appsFlyerID_ios)) {
                AppsFlyer.initSDK(MadPixelCustomSettings.APPSFLYER_SDK_KEY, customSettings.appsFlyerID_ios, this);
            }
            else {
                Debug.LogError($"Can not find IOS APP ID for appsflyer ios!");
            }
#endif
            AppsFlyer.enableTCFDataCollection(true);

            // Purchase connector implementation 
            if (m_usePurchaseConnector) {
                AppsFlyerPurchaseConnector.init(this, AppsFlyerConnector.Store.GOOGLE);
                AppsFlyerPurchaseConnector.setIsSandbox(false);
                AppsFlyerPurchaseConnector.setAutoLogPurchaseRevenue(
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions,
                    AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases);
                AppsFlyerPurchaseConnector.build();

                AppsFlyerPurchaseConnector.startObservingTransactions();
            }

            AppsFlyer.startSDK();

            IronSourceEvents.onImpressionDataReadyEvent += LogAdPurchase;
        }

        private void OnDestroy() {
            IronSourceEvents.onImpressionDataReadyEvent -= LogAdPurchase;
        }

        #endregion

        #region AppsFlyer's Inner Stuff

        public void didFinishValidateReceipt(string result) {
            Debug.Log($"Purchase {result}");
        }

        public void didFinishValidateReceiptWithError(string error) {
            Debug.Log($"Purchase {error}");
        }

        public void onConversionDataSuccess(string conversionData) {
            AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
            // add deferred deeplink logic here
        }

        public void onConversionDataFail(string error) {
            AppsFlyer.AFLog("onConversionDataFail", error);
        }

        public void onAppOpenAttribution(string attributionData) {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error) {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        #endregion

        #region Events

        public void VerificateAndSendPurchase(MPReceipt receipt) {
            if (m_usePurchaseConnector) {
                return;
            }

            string currency = receipt.Product.metadata.isoCurrencyCode;
            float revenue = (float)receipt.Product.metadata.localizedPrice;
            string revenueString = revenue.ToString(CultureInfo.InvariantCulture);

#if UNITY_ANDROID
            if (string.IsNullOrEmpty(m_monetizationPublicKey)) {
                return;
            }

            AppsFlyer.validateAndSendInAppPurchase(m_monetizationPublicKey, receipt.Signature, receipt.Data, revenueString, currency, null,
                this);
#endif

#if UNITY_IOS
            AppsFlyer.validateAndSendInAppPurchase(receipt.SKU, revenueString,  currency,  receipt.Product.transactionID,  null,  this);
#endif
        }

        public void OnFirstInApp() {
            AppsFlyer.sendEvent("Unique_PU", null);
        }

        public void OnRewardedShown(string Placement) {
            Dictionary<string, string> rvfinishEvent = new Dictionary<string, string>();
            rvfinishEvent.Add("Placement", Placement);
            AppsFlyer.sendEvent("RV_finish", rvfinishEvent);
        }

        public void OnInterShown() {
            AppsFlyer.sendEvent("IT_finish", null);
        }

        public void VideoAdsAvaliable(string ad_type, string placement, string result, bool connection) {
#if UNITY_EDITOR
            Debug.Log($"video_ads_available{ad_type}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("ad_type", ad_type);
            Event.Add("placement", placement);
            Event.Add("result", result);
            Event.Add("connection", connection.ToString());
            AppsFlyer.sendEvent("video_ads_available", Event);
        }

        public void VideoAdsStarted(string ad_type, string placement, string result, bool connection) {
#if UNITY_EDITOR
            Debug.Log($"video_ads_started{ad_type}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("ad_type", ad_type);
            Event.Add("placement", placement);
            Event.Add("result", result);
            Event.Add("connection", connection.ToString());
            AppsFlyer.sendEvent("video_ads_started", Event);
        }

        public void VideoAdsWatch(string ad_type, string placement, string result, bool connection, int level_number, string level_name,
            int level_count, string level_diff) {
#if UNITY_EDITOR
            Debug.Log($"video_ads_watch{ad_type}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("ad_type", ad_type);
            Event.Add("placement", placement);
            Event.Add("result", result);
            Event.Add("connection", connection.ToString());
            Event.Add("level_number", level_number.ToString());
            Event.Add("level_name", level_name);
            Event.Add("level_count", level_count.ToString());
            Event.Add("level_diff", level_diff);
            AppsFlyer.sendEvent("video_ads_watch", Event);
        }

        public void PaymentSucceed(string inapp_id, string currency, float price, string inapp_type) {
#if UNITY_EDITOR
            Debug.Log($"payment_succeed{inapp_id}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("inapp_id", inapp_id);
            Event.Add("currency", currency);
            Event.Add("price", price.ToString());
            Event.Add("inapp_type", inapp_type);
            AppsFlyer.sendEvent("payment_succeed", Event);
        }

        public void RateUs(string show_reason, int rate_result) {
#if UNITY_EDITOR
            Debug.Log($"rate_us{show_reason}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("show_reason", show_reason);
            Event.Add("rate_result", rate_result.ToString());
            AppsFlyer.sendEvent("rate_us", Event);
        }

        public void LevelStart(int level_number, string level_name, int level_count, string level_diff) {
#if UNITY_EDITOR
            Debug.Log($"level_start{level_number}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("level_number", level_number.ToString());
            Event.Add("level_name", level_name);
            Event.Add("level_count", level_count.ToString());
            Event.Add("level_diff", level_diff);
            AppsFlyer.sendEvent("level_start", Event);
        }

        public void LevelFinish(int level_number, string level_name, int level_count, string level_diff, string result, int time, int progress,
            int _continue) {
#if UNITY_EDITOR
            Debug.Log($"level_finish {level_number}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("level_number", level_number.ToString());
            Event.Add("level_name", level_name);
            Event.Add("level_count", level_count.ToString());
            Event.Add("level_diff", level_diff);
            Event.Add("result", result);
            Event.Add("time", time.ToString());
            Event.Add("progress", progress.ToString());
            Event.Add("continue", _continue.ToString());
            AppsFlyer.sendEvent("level_finish", Event);
        }

        public void Tutorial(string step_name) {
#if UNITY_EDITOR
            Debug.Log($"tutorial{step_name}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("step_name", step_name);
            AppsFlyer.sendEvent("tutorial", Event);
        }

        /* public void MetaVillage(string step_name) {
 #if UNITY_EDITOR
             Debug.Log($"meta_village{step_name}");
 #endif
             Dictionary<string, string> Event = new Dictionary<string, string>();
             Event.Add("step_name", step_name);
             AppsFlyer.sendEvent("meta_village", Event);
         }*/

        public void BlockPlaced(string block_placed) {
#if UNITY_EDITOR
            Debug.Log($"block_placed{block_placed}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("block_placed", block_placed);
            AppsFlyer.sendEvent("block_placed", Event);
        }

        public void ZoneUnlocked(string zone_unlocked) {
#if UNITY_EDITOR
            Debug.Log($"zone_unlocked{zone_unlocked}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("zone_unlocked", zone_unlocked);
            AppsFlyer.sendEvent("zone_unlocked", Event);
        }

        public void BlockUpgrade(string block_upgrade) {
#if UNITY_EDITOR
            Debug.Log($"block_upgrade{block_upgrade}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("block_upgrade", block_upgrade);
            AppsFlyer.sendEvent("block_upgrade", Event);
        }

        public void BlockDelete(string block_delete) {
#if UNITY_EDITOR
            Debug.Log($"block_delete{block_delete}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("block_delete", block_delete);
            AppsFlyer.sendEvent("block_delete", Event);
        }

        public void ResourceCollect(string resource_collect) {
#if UNITY_EDITOR
            Debug.Log($"resource_collect{resource_collect}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("resource_collect", resource_collect);
            AppsFlyer.sendEvent("resource_collect", Event);
        }

        public void ShopOpen(int randomPiece) {
#if UNITY_EDITOR
            Debug.Log($"shop_open{randomPiece}");
#endif
            Dictionary<string, string> Event = new Dictionary<string, string>();
            Event.Add("Random piece", randomPiece.ToString());
            AppsFlyer.sendEvent("shop_open", Event);
        }

        #endregion

        #region AdRevenue

        public static void LogAdPurchase(IronSourceImpressionData a_impressionData) {
            if (a_impressionData == null || a_impressionData.revenue == null || a_impressionData.revenue.Value <= 0) {
                return;
            }

            Dictionary<string, string> additionalParams = new Dictionary<string, string>();
            additionalParams.Add("custom_AdUnitIdentifier", a_impressionData.mediationAdUnitId);
            additionalParams.Add(AdRevenueScheme.AD_TYPE, a_impressionData.adFormat);

            AFAdRevenueData logRevenue = new AFAdRevenueData(a_impressionData.adNetwork, MediationNetwork.IronSource, "USD",
                a_impressionData.revenue.Value);
            AppsFlyer.logAdRevenue(logRevenue, additionalParams);
        }

        #endregion
    }
}
