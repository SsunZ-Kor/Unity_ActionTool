using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[System.Serializable]
public class GameObjectPool<TComponent> where TComponent : Component
{
    [SerializeField] 
    protected GameObject _prefab;
    [SerializeField] 
    protected Transform _trRoot;
    [SerializeField]
    protected int _nCount;
    
    private Queue<TComponent> _queueGoGameObjects = new();

    public void Init() => Init(_prefab, _trRoot, _nCount);
    
    public void Init(GameObject prf, Transform trRoot, int nCount = 1)
    {
        if (prf == null)
            return;
        
        // 프리펩 유효성 체크
        _prefab = prf;
        _prefab.SetActive(false);

        // 카운트 유효성 체크
        if (_nCount < 0)
            _nCount = 1;
        
        // TransformRoot 유효성 체크
        _trRoot = trRoot;
        if (_trRoot == null)
            CheckAndCreateRoot();
        else
            _trRoot.gameObject.SetActive(false);

        // Pooling
        while (_queueGoGameObjects.Count < nCount)
            _queueGoGameObjects.Enqueue(_CreateGameObject());

    }

    public TComponent Pop()
    {
        TComponent comp = null;
        if (_queueGoGameObjects.Count == 0)
            comp = _CreateGameObject();
        else
            comp = _queueGoGameObjects.Dequeue();

        comp.gameObject.SetActive(true);
        return comp;
    }

    public void Push(TComponent comp)
    {
        CheckAndCreateRoot();
        
        comp.transform.SetParent(_trRoot);
        _queueGoGameObjects.Enqueue(comp);
    }

    private TComponent _CreateGameObject()
    {
        var go = GameObject.Instantiate(_prefab);
        return go.GetComponent<TComponent>();
    }

    private void CheckAndCreateRoot()
    {
        if (_trRoot != null)
            return;
        
        var goRoot = new GameObject($"GO_POOL_{_prefab.name}");
        goRoot.SetActive(false);
        _trRoot = goRoot.transform;
    }
}