using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SfxObject : MonoBehaviour
{
    private SfxManager _pool;
    public LinkedListNode<SfxObject> Node { get; private set; }

    [SerializeField]
    private AudioSource _audioSource;
    public AudioSource Audio => _audioSource;

    private Coroutine _corStop;

    private System.Action<AudioClip> _endCallback;

    private void Awake()
    {
        Node = new LinkedListNode<SfxObject>(this);
        _audioSource = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _corStop = null;
    }

    private void OnDestroy()
    {
        if (Node != null)
        {
            Node.RemoveSelf();
            Node.Value = null;
            Node = null;   
        }
    }

    public void Init(SfxManager master)
    {
        _pool = master;
    }

    public void Play(AudioClip clip, float spatialBlend, bool bLoop, System.Action<AudioClip> endCallback)
    {
        _endCallback = endCallback;
        _audioSource.clip = clip;
        _audioSource.spatialBlend = spatialBlend;
        _audioSource.loop = bLoop;
            
        if (!bLoop)
            ReturnToPool(false);
    }

    public void ReturnToPool(bool bForced)
    {
        if (bForced)
        {
            var clip = _audioSource.clip;
            _audioSource.clip = null;
            
            StopAllCoroutines();
            _corStop = null;
            
            if (_pool != null)
                _pool.ReleaseSfxObject(this);
            
            if (_endCallback != null)
            {
                var call = _endCallback;
                _endCallback = null;
                call.Invoke(clip);
            }
            
            return;
        }

        if (_corStop != null)
            return;

        if (_audioSource.loop)
        {
            IEnumerator SoundOff()
            {
                var fStartTime = Time.realtimeSinceStartup;
                
                var volume = _audioSource.volume;
                while (_audioSource.volume > 0)
                {
                    _audioSource.volume = Mathf.Clamp01(Mathf.Lerp(volume, 0f, Time.realtimeSinceStartup - fStartTime));
                    yield return null;
                }
                
                ReturnToPool(true);
            }
            
            _corStop = StartCoroutine(SoundOff());
        }
        else
        {
            IEnumerator WaitForEnd()
            {
                while (_audioSource.isPlaying)
                    yield return null;
                
                ReturnToPool(true);
            }

            _corStop = StartCoroutine(WaitForEnd());
        }
    }
}