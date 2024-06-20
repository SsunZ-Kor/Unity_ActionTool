using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class SfxManager : MonoBehaviour, IManager
{
    protected LinkedList<SfxObject> _activeObjects = new();
    protected LinkedList<SfxObject> _inactiveObjects = new();

    private float _fVolume = 1f;
    public float Volume {
        get
        {
            return _fVolume;
        }
        set
        {
            _fVolume = value;
            _activeObjects.ForEach(node => node.Value.Audio.volume = _fVolume);
            _inactiveObjects.ForEach(node => node.Value.Audio.volume = _fVolume);
        }
    }

    public IEnumerator Init()
    {
        for (int i = 0; i < 32; ++i)
        {
            var sfxObject = CreateSfxObject();
            ReleaseSfxObject(sfxObject);
        }
        
        yield break;
    }

    public void Release()
    {
        _activeObjects.ForEach(node => node.Value.ReturnToPool(true));
        Destroy(gameObject);
    }

    public void OnScene_Closed()
    {
        _activeObjects.ForEach(node => node.Value.ReturnToPool(true));
    }

    public SfxObject Play3D(AudioClip clip, bool bLoop, Transform trParent, Vector3 vLocalPos, bool bFollowParent, System.Action<AudioClip> endCallback = null)
    {
        if (clip == null)
            return null;
        
        var sfxObject = GetSfxObject();
        
        // Active List로 변경
        sfxObject.Node.RemoveSelf();
        _activeObjects.AddLast(sfxObject.Node);
        sfxObject.gameObject.SetActive(true);

        // Hierarchy 세팅
        var tr = sfxObject.transform;
        tr.SetParent(trParent);
        tr.localPosition = vLocalPos;

        if (bFollowParent)
            tr.SetParent(this.transform);
        
        // Audio 세팅
        sfxObject.Play(clip, 1f, bLoop, endCallback);
        return sfxObject;
    }

    public SfxObject Play2D(AudioClip clip, bool bLoop, System.Action<AudioClip> endCallback = null)
    {
        if (clip == null)
            return null;
        
        var sfxObject = GetSfxObject();
        
        // Active List로 변경
        sfxObject.Node.RemoveSelf();
        _activeObjects.AddLast(sfxObject.Node);
        sfxObject.gameObject.SetActive(true);

        // Hierarchy 세팅
        var tr = sfxObject.transform;
        tr.localPosition = Vector3.zero;
        
        sfxObject.Play(clip, 0f, bLoop, endCallback);
        return sfxObject;
    }

    public void ReleaseSfxObject(SfxObject sfxObject)
    {
        if (sfxObject == null)
            return;

        // Audio 세팅
        var audio = sfxObject.Audio;
        audio.clip = null;
        audio.loop = false;
        
        // Hierarchy 세팅
        sfxObject.transform.SetParent(transform);
        
        // Inactive List로 변경
        sfxObject.Node.RemoveSelf();
        _inactiveObjects.AddLast(sfxObject.Node);
        sfxObject.GameObject().SetActive(false);
    }

    private SfxObject GetSfxObject()
    {
        // 꺼진 오브젝트에서 찾아본다.
        var nodeSfx = _inactiveObjects.First;
        if (nodeSfx != null && nodeSfx.Value != null)
            return nodeSfx.Value;
        
        // 오브젝트가 없다면 현재 출력된 오브젝트(Loop가 아닌)에서 찾아온다.
        nodeSfx = _activeObjects.First;
        while (nodeSfx != null)
        {
            var crrNodeSfx = nodeSfx;
            nodeSfx = nodeSfx.Next;

            if (crrNodeSfx.Value == null) // Desotry 됬을 수도 있다. 걸러주자
            {
                crrNodeSfx.RemoveSelf();
                continue;
            }

            if (crrNodeSfx.Value.Audio.loop)
                continue;

            return nodeSfx.Value;
        }

        // 그래도 못찾았다면 하나 생성한다.
        return CreateSfxObject();
    }

    private SfxObject CreateSfxObject()
    {
        var goSfx = new GameObject("SFXObject");
        var sfxObject = goSfx.AddComponent(typeof(SfxObject)) as SfxObject;

        sfxObject.Init(this);
        return sfxObject;
    }
}