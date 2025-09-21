using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnalyticsSessionTime {
    public class AnalyticsTimeEvent : MonoBehaviour {
        private float _sessionTime;

        private float SavedSessionTime {
            get => StorageManager.GameDataMain.SessionSeconds;
            set {
                StorageManager.GameDataMain.SessionSeconds = value;
                StorageManager.SaveGame();
            }
        }

        private int SavedPlaytimeMinutes {
            get => StorageManager.GameDataMain.SessionMinutes;
            set {
                StorageManager.GameDataMain.SessionMinutes = value;
                StorageManager.SaveGame();
            }
        }

        private void Start() {
            _sessionTime = SavedSessionTime;
            DontDestroyOnLoad(gameObject);
        }

        private void Update() {
            if (Application.isFocused) {
                _sessionTime += Time.unscaledDeltaTime;
            }

            if (_sessionTime >= 60) {
                _sessionTime -= 60;
                SavedSessionTime = _sessionTime;
                SavedPlaytimeMinutes++;

                if (Debug.isDebugBuild) {
                    Debug.Log($"Send playtime event. Elapsed minutes: {SavedPlaytimeMinutes.ToString()}");
                }

                try {
                    ZhukovskyAnalyticsManager.Instance.SendCustomEvent("timer", new Dictionary<string, object> {
                        { "time", SavedPlaytimeMinutes.ToString() }
                    });
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }

        private void OnApplicationQuit() {
            SavedSessionTime = _sessionTime;
            PlayerPrefs.Save();
        }

        private void OnDestroy() {
            SavedSessionTime = _sessionTime;
            PlayerPrefs.Save();
        }
    }
}