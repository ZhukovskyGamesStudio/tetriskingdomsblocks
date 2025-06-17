using System;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;

public class VibrationsManager : MonoBehaviour {
    public static VibrationsManager Instance { get; private set; }

    [SerializeField]
    private HapticSource _diceSource;

    private Dictionary<VibrationType, HapticSource> _vibrationsD;
    private Dictionary<VibrationType, HapticPatterns.PresetType> _vibrationsPresets;

    private void Awake() {
        Instance = this;
        InitDictionary();
    }

    private void InitDictionary() {
        _vibrationsD = new Dictionary<VibrationType, HapticSource> {
            { VibrationType.PlacePiece, _diceSource },
            { VibrationType.AllRow, _diceSource },
            { VibrationType.Win, _diceSource },
            { VibrationType.Lose, _diceSource }
        };
        _vibrationsPresets = new Dictionary<VibrationType, HapticPatterns.PresetType> {
            { VibrationType.PlacePiece, HapticPatterns.PresetType.HeavyImpact },
            { VibrationType.AllRow, HapticPatterns.PresetType.Success },
            { VibrationType.Win, HapticPatterns.PresetType.Success },
            { VibrationType.Lose, HapticPatterns.PresetType.Failure }
        };
    }

    public void SpawnVibration(VibrationType type) {
        _vibrationsD[type].Play();
    }

    public void SpawnVibrationEmhpasis(float amplitude, float frequency = 0.7f) {
        HapticPatterns.PlayEmphasis(amplitude, frequency);
    }

    public void SpawnContinuous(float amplitude, float frequency, float duration) {
        HapticController.fallbackPreset = HapticPatterns.PresetType.LightImpact;
        HapticPatterns.PlayConstant(amplitude, frequency, duration);
    }
}

[Serializable]
public enum VibrationType {
    PlacePiece,
    AllRow,
    Win,
    Lose
}