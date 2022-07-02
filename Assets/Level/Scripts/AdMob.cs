using GoogleMobileAds.Api;
using UnityEngine;
using System;
using System.Collections;

public class AdMob : MonoBehaviour
{
    public string AppId = "ca-app-pub-8698419787299114~9025445104";
    public string VideoId = "ca-app-pub-8698419787299114/4405850266";
    public string RewardedId = "ca-app-pub-8698419787299114/7389793178";
    public string BannerId = "ca-app-pub-8698419787299114/2290822811";

    public static bool ShowingAds;
    public static BannerView banner;
    public static InterstitialAd Video;
    public static RewardedAd Rewarded;

    public void Init()
    {
        //MobileAds.Initialize(initStatus => { });
    }

    public void ShowVideo()
    {
        StartCoroutine(ShowVideoForce());
    }
    private IEnumerator ShowVideoForce()
    {
        while(Video == null || !Video.IsLoaded())
        {
            yield return new WaitForFixedUpdate();
        }
        Video.Show();
        yield break;
    }

    public void ShowRewarded()
    {
        StartCoroutine(ShowRewardedForce());
    }
    private IEnumerator ShowRewardedForce()
    {
        while (Rewarded == null || !Rewarded.IsLoaded())
        {
            yield return new WaitForFixedUpdate();
        }
        Rewarded.Show();
        yield break;
    }

    public void LoadAds()
    {
        LoadVideo();
        LoadRewarded();
    }
    public void LoadVideo()
    {
        AdRequest request = new AdRequest.Builder().Build();

        Video = new InterstitialAd(VideoId);
        Video.OnAdOpening += Pause;
        Video.OnAdClosed += Resume;

        Video.OnAdClosed += PreLoadInterstitial;
        Video.LoadAd(request);
    }
    public void LoadRewarded()
    {
        AdRequest request = new AdRequest.Builder().Build();

        Rewarded = new RewardedAd(RewardedId);
        Rewarded.OnAdOpening += Pause;
        Rewarded.OnUserEarnedReward += Reward;
        Rewarded.OnAdClosed += ChecForReward;
        Rewarded.OnAdClosed += PreLoadRewarded;
        Rewarded.LoadAd(request);
    }


    public void Pause(object sender, EventArgs args)
    {
        ShowingAds = true;
        Level.active.SpecialPause();
    }
    public void Resume(object sender, EventArgs args)
    {
        ShowingAds = false;
        Level.active.SpecialResume();
    }
    public void ChecForReward(object sender, EventArgs args)
    {
        ShowingAds = false;
        Level.active.CheckForBonus();
    }
    public void Reward(object sender, EventArgs args)
    {
        Level.active.UserGotBonus();
    }

    public void PreLoadInterstitial(object sender, EventArgs args)
    {
        LoadVideo();
    }
    public void PreLoadRewarded(object sender, EventArgs args)
    {
        LoadRewarded();
    }

    public void CreateBanner(bool Top)
    {
        DestroyBanner();
        AdSize Size = Top ? AdSize.Banner : AdSize.IABBanner;
        AdPosition Position = Top ? AdPosition.Top : AdPosition.Bottom;
        banner = new BannerView(BannerId, Size, Position);
        AdRequest request = new AdRequest.Builder().Build();
        banner.LoadAd(request);
        banner.OnAdLoaded += ShowBanner;
    }
    private void ShowBanner(object sender, EventArgs args)
    {
        banner.Show();
    }
    public void HideBanner()
    {
        if(banner != null)
        {
            banner.Hide();
        }
    }
    public void DestroyBanner()
    {
        if (banner != null)
        {
            banner.Destroy();
            banner = null;
        }
    }
}
