using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using MadPixel;

public class CollectAllDialog : DialogBase {
    
    public void CollectAll() {
        Hide().Forget();
        CollectResources(1);
    }

    public void CollectAllWithAds() {
        Hide().Forget();
        
        
       /* if(AdsManager.Ready())
            ZhukovskyAnalyticsManager.Instance.SendCustomEvent("video_ads_available", new Dictionary<string, object> {
                { "ad_type","rewarded"  },
                { "placement","get_double_afk_resources"  },
                { "result","canceled"  },
                { "connection",MainManager.Instance._hasInternetConnection.ToString()  }
            }, true);
        else 
            ZhukovskyAnalyticsManager.Instance.SendCustomEvent("video_ads_available", new Dictionary<string, object> {
                { "ad_type","rewarded"  },
                { "placement","get_double_afk_resources"  },
                { "result","canceled"  },
                { "connection",MainManager.Instance._hasInternetConnection.ToString()  }
            }, true);*/
        
        ZhukovskyAdsManager.Instance.AdsProvider.ShowRewardedAd(AdsIds.AdsTypesIds[AdsTypes.DoubleAfkResources], CollectAllWithMultiplier,
            FailedAd);
    }

    private void CollectAllWithMultiplier() {
        CollectResources(MetaFieldManager.Instance.MainMetaConfig.CollectWithAdsMultiplier);
      /*  ZhukovskyAnalyticsManager.Instance.SendCustomEvent("video_ads_watch", new Dictionary<string, object> {
            { "ad_type","rewarded"  },
            { "placement","get_double_afk_resources"  },
            { "result","watched"  },
            { "connection",MainManager.Instance._hasInternetConnection.ToString()  }
        }, true);
        */
    }

    private void AdAvaliable() {
        
    }
    private void AdNotAvaliable() {
        
    }
    private void FailedAd() {
    /*    ZhukovskyAnalyticsManager.Instance.SendCustomEvent("video_ads_watch", new Dictionary<string, object> {
            { "ad_type","rewarded"  },
            { "placement","get_double_afk_resources"  },
            { "result","canceled"  },
            { "connection",MainManager.Instance._hasInternetConnection.ToString()  }
        }, true);*/
    }

    private void CollectResources(float multiplier) {
        MetaFieldManager.Instance.CollectResourcesFromAllMarks(multiplier);
    }
}