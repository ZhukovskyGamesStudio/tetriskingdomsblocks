using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioQueueMixer : MonoBehaviour {
    [SerializeField]
    private List<AudioSource> _audioSources;

    private Queue<AudioSource> _audioSourcesQ;

    [SerializeField]
    private float _startPercent = 0.25f;

    [SerializeField]
    private int _sourcePoolCount = 3;

    private AudioSource _currentPlaying;
    private int _needVolume;

    public float VolumeMultiplier = 1;

    private void Awake() {
        if (_sourcePoolCount > 0) {
            CreateSources();
        }

        _audioSourcesQ = new Queue<AudioSource>(_audioSources.OrderBy((_) => Random.Range(0, 1f)));
    }

    private void Update() {
        if (_currentPlaying != null) {
            _currentPlaying.volume = _needVolume * VolumeMultiplier;
        }
    }

    public void StopCurrentAudioSource(bool isPlay) {
        if (isPlay)
            _needVolume = 1;
        else
            _needVolume = 0;

        if (_currentPlaying) {
            _currentPlaying.volume = _needVolume * VolumeMultiplier;
        }
    }

    private void CreateSources() {
        for (int i = 0; i < _sourcePoolCount - 1; i++) {
            var newComp = gameObject.AddComponent<AudioSource>();
            newComp.clip = _audioSources[0].clip;
            newComp.priority = _audioSources[0].priority;
            newComp.playOnAwake = false;
            newComp.volume = _audioSources[0].volume;
            newComp.outputAudioMixerGroup = _audioSources[0].outputAudioMixerGroup;
            _audioSources.Add(newComp);
        }
    }

    public async UniTask PlayNext() {
        AudioSource next = _audioSourcesQ.Dequeue();
        float tenPercentTime = next.clip.length * _startPercent;
        next.time = tenPercentTime;
        next.Play();
        _audioSourcesQ.Enqueue(next);
        await UniTask.WaitWhile(() => next.isPlaying, cancellationToken: this.GetCancellationTokenOnDestroy());
        next.Stop();
    }

    public async UniTask PlayNextBlended(CancellationToken cancellationToken) {
        // Fade out предыдущий
        if (_currentPlaying != null && _currentPlaying.isPlaying) {
            float startVolume = _currentPlaying.volume;
            float t = 0f;
            while (t < 0.5f) {
                t += Time.unscaledDeltaTime;
                _currentPlaying.volume = Mathf.Lerp(startVolume, 0f, t / 0.5f);
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            _currentPlaying.Stop();
            _currentPlaying.volume = startVolume;
        }

        // Воспроизвести следующий с fade in
        AudioSource next = _audioSourcesQ.Dequeue();
        float tenPercentTime = next.clip.length * _startPercent;
        next.time = tenPercentTime;
        next.volume = 0f;
        next.Play();
        _currentPlaying = next;
        _audioSourcesQ.Enqueue(next);

        float tIn = 0f;
        while (tIn < 0.5f) {
            tIn += Time.unscaledDeltaTime;
            next.volume = Mathf.Lerp(0f, _needVolume, tIn / 0.5f) * VolumeMultiplier;
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
        }

        next.volume = _needVolume * VolumeMultiplier;

        await UniTask.WaitWhile(() => next.isPlaying, cancellationToken: cancellationToken);
        next.Stop();
    }
}