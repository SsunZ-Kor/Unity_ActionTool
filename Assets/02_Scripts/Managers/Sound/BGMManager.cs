using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BGMChangeType
{
    CrossFade,
    FadeOutIn,
}

public class BGMManager : MonoBehaviour, IManager
{
    public class AudioInfo
    {
        public AudioSource Audio;

        public float fCurrentVolume;
        public float fTargetDuration;
        public float fElapsedTime;
    }
    
    private float _fVolume = 1f;
    public float Volume {
        get
        {
            return _fVolume;
        }
        set
        {
            _fVolume = value;
        }
    }
    
    private AudioInfo _mainAudio = null;
    private readonly Dictionary<AudioClip, AudioInfo> _dicDeactivate = new();   // 꺼지고 있는 중
    private readonly Queue<AudioInfo> _queueInactivate = new();                 // 이미 꺼진 것
    private readonly List<AudioClip> _listInactivateClip = new(5);       // 꺼지고 있는 중에서 삭제될 목록
    
    public IEnumerator Init()
    {
        yield break;
    }
    
    public void Release()
    {
        Destroy(gameObject);
    }

    private void Update()
    {
        // MainAudio 업데이트
        if (_mainAudio != null && _mainAudio.fElapsedTime < _mainAudio.fTargetDuration)
        {
            _mainAudio.fElapsedTime += Time.unscaledDeltaTime;
            _mainAudio.fCurrentVolume = Mathf.Clamp01(_mainAudio.fElapsedTime / _mainAudio.fTargetDuration);
            _mainAudio.Audio.volume = _mainAudio.fCurrentVolume * Volume;
        }
        
        // DeactivateAudio 업데이트 및 제거 수집
        foreach (var pair in _dicDeactivate)
        {
            var audioInfo = pair.Value;

            if (audioInfo.fElapsedTime < audioInfo.fTargetDuration)
            {
                audioInfo.fElapsedTime += Time.unscaledDeltaTime;
                audioInfo.fCurrentVolume = Mathf.Clamp01(1f - audioInfo.fElapsedTime / audioInfo.fTargetDuration);
                audioInfo.Audio.volume = _mainAudio.fCurrentVolume * Volume;
            }
            else
            {
                // 초기화 및 InactivateAudio로 전환
                audioInfo.Audio.clip = null;
                audioInfo.fTargetDuration = 0f;
                audioInfo.fElapsedTime = 0f;
                _queueInactivate.Enqueue(audioInfo);
                
                // DeactivateAudio에서 삭제할 목록으로 이전
                _listInactivateClip.Add(pair.Key);
            }
        }

        // DeactivateAudio에서 삭제
        if (_listInactivateClip.Count > 0)
        {
            foreach (var clip in _listInactivateClip)
                _dicDeactivate.Remove(clip);

            _listInactivateClip.Clear();   
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        // Release Main
        if (_mainAudio != null && _mainAudio.Audio != null)
        {
            _mainAudio.Audio.clip = null;
            _mainAudio.Audio = null;
        }

        _mainAudio = null;

        // Release Deactivate
        foreach (var pair in _dicDeactivate)
        {
            pair.Value.Audio.clip = null;
            pair.Value.Audio = null;
        }
        
        _dicDeactivate.Clear();

        // Release Inactivate
        foreach (var audioInfo in _queueInactivate)
        {
            audioInfo.Audio.clip = null;
            audioInfo.Audio = null;
        }
        
        _queueInactivate.Clear();
    }

    public void PlayBGM(AudioClip clip, BGMChangeType changeType, float fOutDuration, float fInDuration)
    {
        if (_mainAudio != null && _mainAudio.Audio.clip == clip)
        {
            _mainAudio.fTargetDuration = fInDuration;

            if (fInDuration == 0f)
            {
                _mainAudio.fElapsedTime = 0f;
                _mainAudio.fCurrentVolume = 1f;
                _mainAudio.Audio.volume = Volume;
            }
            else
            {
                _mainAudio.fElapsedTime = fInDuration * _mainAudio.fCurrentVolume;
            }
            
            return;
        }
        
        var prevAudioInfo = _mainAudio;
        StopBGM(fOutDuration);

        if (clip == null)
            return;
        
        StopAllCoroutines();
        
        var fWaitTime = (prevAudioInfo == null || changeType == BGMChangeType.CrossFade)
            ? 0f
            : prevAudioInfo.fTargetDuration - prevAudioInfo.fElapsedTime;

        StartCoroutine(_PlayBGM(clip, fWaitTime, fInDuration));
    }
    
    public void StopBGM(float fOutDuration)
    {
        if (_mainAudio == null)
            return;

        // 재생 중인 BGM 제거
        _mainAudio.fTargetDuration = fOutDuration;
        _mainAudio.fElapsedTime = fOutDuration * (1f - _mainAudio.fCurrentVolume);

        _dicDeactivate.Add(_mainAudio.Audio.clip, _mainAudio);
        _mainAudio = null;
    }

    private IEnumerator _PlayBGM(AudioClip clip, float fWaitTime, float fInDuration)
    {
        if (fWaitTime > 0f)
            yield return new WaitForSecondsRealtime(fWaitTime);

        if (!_dicDeactivate.TryGetValue(clip, out _mainAudio))
            _mainAudio = _GetAudioInfo();

        _mainAudio.Audio.clip = clip;
        _mainAudio.fTargetDuration = fInDuration;

        if (fInDuration == 0f)
        {
            _mainAudio.fElapsedTime = 0f;
            _mainAudio.fCurrentVolume = 1f;
            _mainAudio.Audio.volume = Volume;
        }
        else
        {
            _mainAudio.fElapsedTime = fInDuration * _mainAudio.fCurrentVolume;
        }
    }

    private AudioInfo _GetAudioInfo()
    {
        var result = _queueInactivate.Dequeue();
        if (result != null)
            return result;

        result = new AudioInfo();
        result.Audio = this.AddComponent<AudioSource>();
        return result;
    }
}