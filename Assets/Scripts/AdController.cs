using UnityEngine;
using UnityEngine.Advertisements;

public class AdController : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdController Instance;
    [SerializeField]
    private bool testMode;

    [Header("Android")]
    [SerializeField]
    private string _androidGameId;
    [SerializeField]
    private string _androidAdUnitId_Banner = "Banner_Android";
    [SerializeField]
    private string _androidAdUnitId_Interstitial = "Interstitial_Android";
    [SerializeField]
    private string _androidAdUnitId_Rewarded = "Rewarded_Android";

    private string _gameId;
    private string _adUnitId_Banner;
    private string _adUnitId_Interstitial;
    private string _adUnitId_Rewarded;


    [HideInInspector]
    public bool rewardIsReady;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeAds();
    }

    private void InitializeAds()
    {
        _gameId = _androidGameId;
        _adUnitId_Banner = _androidAdUnitId_Banner;
        _adUnitId_Interstitial = _androidAdUnitId_Interstitial;
        _adUnitId_Rewarded = _androidAdUnitId_Rewarded;

        Advertisement.Initialize(_gameId, testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        // Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        rewardIsReady = false;
        // LoadBanner();
        LoadInterstitial();
        LoadRewarded();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    private void LoadBanner()
    {
        if (ES3.Load<bool>("noAds", false))
        {
            return;
        }
        
        var options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.Load(_adUnitId_Banner, options);
    }

    private void LoadInterstitial()
    {
        if (ES3.Load<bool>("noAds", false))
        {
            return;
        }
        Advertisement.Load(_adUnitId_Interstitial, this);
    }

    private void LoadRewarded()
    {
        Advertisement.Load(_adUnitId_Rewarded, this);
    }

    private void OnBannerLoaded()
    {
        ShowBannerAd();
    }

    private void OnBannerError(string message)
    {
    }

    public void ShowBannerAd()
    {
        if (ES3.Load<bool>("noAds", false))
        {
            return;
        }
        Advertisement.Banner.Show(_adUnitId_Banner);
    }

    public void ShowInterstitialAd()
    {
        if (ES3.Load<bool>("noAds", false))
        {
            return;
        }
        Advertisement.Show(_adUnitId_Interstitial, this);
    }

    public void ShowRewardedAd()
    {
        Advertisement.Show(_adUnitId_Rewarded, this);
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide(false);
    }


    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId == _adUnitId_Rewarded)
        {
            rewardIsReady = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        if (placementId == _adUnitId_Interstitial)
        {
            LoadInterstitial();
        }
        else if (placementId == _adUnitId_Rewarded)
        {
            rewardIsReady = false;
            LoadRewarded();
        }
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        if (placementId == _adUnitId_Interstitial)
        {
            LoadInterstitial();
        }
        else if (placementId == _adUnitId_Rewarded)
        {
            rewardIsReady = false;
            LoadRewarded();
        }
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        if (placementId == _adUnitId_Interstitial)
        {
            LoadInterstitial();
        }
        else if (placementId == _adUnitId_Rewarded)
        {
            rewardIsReady = false;
            LoadRewarded();
        }
    }

    public void OnUnityAdsShowClick(string placementId)
    {
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (placementId.Equals(_adUnitId_Rewarded) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");
            UndoSystem.Instance.PerformUndo();
            rewardIsReady = false;
            Advertisement.Load(_adUnitId_Rewarded, this);
        }
    }
}