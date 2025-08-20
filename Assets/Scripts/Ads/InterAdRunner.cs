using UnityEngine;

public class InterAdRunner {
    public bool IsInterAdRunEnabled;

    private float _interAdCooldown;
    private IAdsProvider _ads;
    private float _timer;

    public InterAdRunner(float cooldown, IAdsProvider ads) {
        _interAdCooldown = cooldown;
        _ads = ads;
    }

    public void Update() {
        if (!IsInterAdRunEnabled) {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer >= _interAdCooldown) {
            _ads.ShowInterAd("timed_inter_ad");
        }
    }
}