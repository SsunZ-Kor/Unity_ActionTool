using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public partial class ActorManager
{
    public class TargetingObjectPool
    {
        public string name;
        public int nRefCount;
        public GameObject prf;
        public string addressableKey;
        public LinkedList<TargetingObject> activeObjects = new();
        public LinkedList<TargetingObject> inactiveObjects = new();
        
        public TargetingObject CreateTargetingObject(Transform trParent)
        {
            var go = Instantiate(prf, trParent);
            go.SetActive(false);

            var targetingObject = go.GetComponent<TargetingObject>();
            inactiveObjects.AddLast(targetingObject);
            return targetingObject;
        }

        public void ReleaseTargetingObject(TargetingObject targetingObject)
        {
            if (targetingObject == null)
                return;

            targetingObject.gameObject.SetActive(false);
            inactiveObjects.AddLast(targetingObject.Node);
            
            if (Managers.IsValid)
                targetingObject.transform.SetParent(Managers.Fx.transform);
        }

        public void ReturnAllActiveObject()
        {
            activeObjects.ForEach(node => node.Value.ReturnToPool());
        }
    }

    private Transform trRootTO;
    private SortedDictionary<string, TargetingObjectPool> _dicTOPools = new();

    private void _OnInit_TargetingObject()
    {
        if (trRootTO == null)
        {
            trRootTO = new GameObject("Root_TargetingObject").transform;
            trRootTO.SetParent(transform);
            trRootTO.gameObject.isStatic = true;
        }
    }

    private void _OnRelease_TargetingObject()
    {
        foreach (var toPool in _dicTOPools.Values)
        {
            toPool.ReturnAllActiveObject();
            _ReleaseTOPool(toPool);
        }
    }

    public void RegistTargetingObjectAddress(string prfAddress)
    {
        if (string.IsNullOrWhiteSpace(prfAddress))
            return;

        if (!_dicTOPools.TryGetValue(prfAddress, out var toPool))
        {
            toPool = _CreateTOPool(prfAddress, Managers.Asset.Load<GameObject>(prfAddress), 1);
            toPool.addressableKey = prfAddress;
        }
        else
            ++toPool.nRefCount;
    }

    public void RegistTargetingObjectPrefab(GameObject prf)
    {
        if (prf == null)
            return;

        if (!_dicTOPools.TryGetValue(prf.name, out var toPool))
            _CreateTOPool(prf.name, prf, 1);
        else
            ++toPool.nRefCount;
    }
    
    public void UnregistTargetingObjectAddress(string prfAddress)
    {
        if (string.IsNullOrWhiteSpace(prfAddress))
            return;
        
        if (!_dicTOPools.TryGetValue(prfAddress, out var fxPool) || --fxPool.nRefCount > 0)
            return;

        _ReleaseTOPool(fxPool);
        Managers.Asset.ReleaseHandle(prfAddress);
    }

    public void UnregistTargetingObjectAddress(GameObject prf)
    {
        if (!_dicTOPools.TryGetValue(prf.name, out var fxPool) || --fxPool.nRefCount > 0)
            return;
        
        _ReleaseTOPool(fxPool);
    }
    
    public TargetingObject GenerateTargetingObject(
        string key, 
        Actor.Actor master,
        float fDelayTime,
        float fLifeTime,
        System.Action<Actor.ActorTarget> hitCallback,
        Transform trParent, 
        Vector3 vLocalPos, 
        Quaternion qLocalRot, 
        Vector3 vLocalScale,
        bool bFollowParent)
    {
        if (!_dicTOPools.TryGetValue(key, out var toPool))
            return null;
        
        var to = toPool.inactiveObjects.First?.Value;
        var trTO = (Transform)null;
        bool hasParent = trParent != null;

        // Get Or Create
        if (to == null)
        {
            to = toPool.CreateTargetingObject(hasParent ? trParent : transform);
            trTO = to.transform;
        }
        else
        {
            trTO = to.transform;
            if (hasParent)
                trTO.SetParent(trParent);
        }
        
        // Set Transform
        trTO.transform.SetLocalTRS(vLocalPos, qLocalRot ,vLocalScale);
        
        if (!bFollowParent && hasParent)
            trTO.SetParent(transform);
        
        // Gen
        toPool.activeObjects.AddLast(to.Node);
        to.gameObject.SetActive(true);
        to.Init(master, fDelayTime, fLifeTime, hitCallback);
        
        return to;
    }
    
    private TargetingObjectPool _CreateTOPool(string poolName, GameObject prf, int nCapacity)
    {
        if (prf == null)
            return null;
        
#if UNITY_EDITOR
        if (prf.GetComponent<TargetingObject>() == null)
        {
            Debug.LogError($"ActorManager :: CreateTOPool \"{poolName}\" is Failed. TargetingObject 컴포넌트를 찾지 못했습니다.");
            return null;
        }
#endif
        
        var result = new TargetingObjectPool();
        result.name = poolName;
        result.prf = prf;
        _dicTOPools.Add(poolName, result);
        _ResizeTOPool(result, nCapacity, false);
        
        return result;
    }
    
    private void _ReleaseTOPool(TargetingObjectPool toPool)
    {
        if (toPool == null)
            return;

        if (!string.IsNullOrWhiteSpace(toPool.addressableKey) && Managers.IsValid)
            Managers.Asset.ReleaseHandle(toPool.addressableKey);
        
        _dicTOPools.Remove(toPool.name);

        toPool.name = null;
        toPool.prf = null;
        foreach (var fxObject in toPool.activeObjects)
            Destroy(fxObject);
        foreach (var fxObject in toPool.inactiveObjects)
            Destroy(fxObject);
        
        toPool.activeObjects.Clear();
        toPool.activeObjects = null;
        toPool.inactiveObjects.Clear();
        toPool.inactiveObjects = null;
    }
    
    private void _ResizeTOPool(TargetingObjectPool fxPool, int nCapacity, bool bForced)
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
                fxPool.CreateTargetingObject(transform);
        }
    }
}
