using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioQueueMixer : MonoBehaviour {
    [SerializeField]
    private List<AudioSource> _audioSources;

    private Queue<AudioSource> _audioSourcesQ;

    [SerializeField]
    private float _startPercent = 0.25f;

    private void Awake() {
        _audioSourcesQ = new Queue<AudioSource>(_audioSources.OrderBy((_) => Random.Range(0, 1f)));
    }

    public void PlayNext() {
        var next = _audioSourcesQ.Dequeue();
        float tenPercentTime = next.clip.length * _startPercent;
        next.time = tenPercentTime;
        next.Play();
        _audioSourcesQ.Enqueue(next);
    }
}