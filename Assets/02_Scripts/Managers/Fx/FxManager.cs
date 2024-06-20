using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FxManager : MonoBehaviour, IManager
{
    public class FxPool
    {
        public string name;
        public int nRefCount = 1;
        public GameObject prf = null;
        public LinkedList<FxObject> activeObjects = new();
        public LinkedList<FxObject> inactiveObjects = new();

        public FxObject CreateFxObject(Transform trParent)
        {
            var go = Instantiate(prf, trParent);
            go.SetActive(false);

            var fxObject = go.GetComponent<FxObject>();
            inactiveObjects.AddLast(fxObject);
            return fxObject;
        }

        public void ReleaseFxObject(FxObject fxObject)
        {
            if (fxObject == null)
                return;

            fxObject.gameObject.SetActive(false);
            inactiveObjects.AddLast(fxObject.Node);

            if (Managers.IsValid)
                fxObject.transform.SetParent(Managers.Fx.transform);
        }

        public void ReturnAllActiveObject()
        {
            activeObjects.ForEach(node => node.Value.ReturnToPool(true));
        }
    }

    private SortedDictionary<string, FxPool> _dicFxPools = new();
    
    public IEnumerator Init()
    {
        yield break;
    }

    public void Release()
    {
        foreach (var fxPool in _dicFxPools.Values)
        {
            fxPool.ReturnAllActiveObject();
            _ReleaseFxPool(fxPool); 
        }
        
        Destroy(gameObject);
    }

    public void OnScene_Closed()
    {
        foreach (var pair in _dicFxPools)
            pair.Value.ReturnAllActiveObject();
    }

    public void RegistFxAddress(string prfAddress, int nCapacity)
    {
        if (string.IsNullOrWhiteSpace(prfAddress))
            return;

        if (!_dicFxPools.TryGetValue(prfAddress, out var fxPool))
            _CreateFxPool(prfAddress, Managers.Asset.Load<GameObject>(prfAddress), nCapacity);
        else
            ++fxPool.nRefCount;
    }

    public void RegistFxPrefab(GameObject prf, int nCapacity)
    {
        if (prf == null)
            return;

        if (!_dicFxPools.TryGetValue(prf.name, out var fxPool))
            _CreateFxPool(prf.name, prf, nCapacity);
        else
            ++fxPool.nRefCount;
    }

    public void UnregistFxAddress(string prfAddress)
    {
        if (!_dicFxPools.TryGetValue(prfAddress, out var fxPool) || --fxPool.nRefCount > 0)
            return;

        _ReleaseFxPool(fxPool);
        Managers.Asset.ReleaseHandle(prfAddress);
    }

    public void UnregistFxPrefab(GameObject prf)
    {
        if (!_dicFxPools.TryGetValue(prf.name, out var fxPool) || --fxPool.nRefCount > 0)
            return;
        
        _ReleaseFxPool(fxPool);
    }

    public FxObject PlayFx(string key, Transform trParent, Vector3 vLocalPos, Quaternion qLocalRot, Vector3 vLocalScale, bool bFollowParent, System.Action<FxObject> endCallback = null)
    {
        if (!_dicFxPools.TryGetValue(key, out var fxPool))
            return null;

        var fxObject = fxPool.inactiveObjects.First?.Value;
        var trFxObject = (Transform)null;
        bool hasParent = trParent != null;

        // Get Or Create
        if (fxObject == null)
        {
            fxObject = fxPool.CreateFxObject(hasParent ? trParent : transform);
            trFxObject = fxObject.transform;
        }
        else
        {
            trFxObject = fxObject.transform;
            if (hasParent)
                trFxObject.SetParent(trParent);
        }
        
        // Set Transform
        trFxObject.transform.SetLocalTRS(vLocalPos, qLocalRot ,vLocalScale);
        
        if (!bFollowParent && hasParent)
            trFxObject.SetParent(transform);

        // Gen
        fxPool.activeObjects.AddLast(fxObject.Node);
        fxObject.gameObject.SetActive(true);
        fxObject.Play(endCallback);
        return fxObject;
    }

    private FxPool _CreateFxPool(string poolName, GameObject prf, int nCapacity)
    {
        if (prf == null)
            return null;
        
#if UNITY_EDITOR
        if (prf.GetComponent<FxObject>() == null)
        {
            Debug.LogError($"FxManager :: CreateFxPool \"{poolName}\" is Failed. FxObject 컴포넌트를 찾지 못했습니다.");
            return null;
        }
#endif
        
        var result = new FxPool();
        result.name = poolName;
        result.prf = prf;
        _dicFxPools.Add(poolName, result);
        _ResizeFxPool(result, nCapacity, false);
        
        return result;
    }

    private void _ReleaseFxPool(FxPool fxPool)
    {
        _dicFxPools.Remove(fxPool.name);

        fxPool.name = null;
        fxPool.prf = null;
        foreach (var fxObject in fxPool.activeObjects)
            Destroy(fxObject);
        foreach (var fxObject in fxPool.inactiveObjects)
            Destroy(fxObject);
        
        fxPool.activeObjects.Clear();
        fxPool.activeObjects = null;
        fxPool.inactiveObjects.Clear();
        fxPool.inactiveObjects = null;
    }

    private void _ResizeFxPool(FxPool fxPool, int nCapacity, bool bForced)
    {
        nCapacity -= fxPool.activeObjects.Count;
        
        // 충분하다면
        if (nCapacity <= fxPool.inactiveObjects.Count)
        {
            if (!bForced)
                return;

            nCapacity = 0;
            while (nCapacity < fxPool.inactiveObjects.Count)
            {
                var node = fxPool.inactiveObjects.First;
                node.RemoveSelf();
                Destroy(node.Value);
            }
        }
        // 부족하다면
        else
        {
            while (nCapacity > fxPool.inactiveObjects.Count)
                fxPool.CreateFxObject(transform);
        }
    }
}
