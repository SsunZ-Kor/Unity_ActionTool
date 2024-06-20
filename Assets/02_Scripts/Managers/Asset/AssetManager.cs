using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public partial class AssetManager : IManager
{
    private struct HandleInfo
    {
        public AsyncOperationHandle handle;
        public uint nRefCount;
    }

    private Dictionary<string, HandleInfo> _dicHandles = new();
    private HashSet<string> _setDontReleaseOnLoad = new();
    
    public IEnumerator Init()
    {
        // Todo :: AssetManager.Init
        yield break;
    }

    public void Release()
    {
        ReleaseAllHandle(true);
        
        // Todo :: AssetManager.Release
    }
    
    public T Load<T>(string address) where T : Object
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;
        
        if (!_dicHandles.TryGetValue(address, out var handleInfo))
        {
            handleInfo.handle = Addressables.LoadAssetAsync<T>(address);
            handleInfo.nRefCount = 1;
            handleInfo.handle.WaitForCompletion();
            _dicHandles.Add(address, handleInfo);
        }

        ++handleInfo.nRefCount;
        
        return handleInfo.handle.Result as T;
    }

    public void LoadAsync<T>(string address, System.Action<T> endCallback) where T : Object
    {
        if (endCallback == null)
            return;

        if (string.IsNullOrWhiteSpace(address))
        {
            endCallback.Invoke(null);
            return;
        }
        
        if (!_dicHandles.TryGetValue(address, out var handleInfo))
        {
            handleInfo.handle = Addressables.LoadAssetAsync<T>(address);
            handleInfo.nRefCount = 1;
            handleInfo.handle.Completed += (x) => endCallback(x.Result as T);
            _dicHandles.Add(address, handleInfo);
            return;
        }

        ++handleInfo.nRefCount;

        if (handleInfo.handle.IsDone)
            endCallback.Invoke(handleInfo.handle.Result as T);
        else
            handleInfo.handle.Completed += (x) => endCallback(x.Result as T);
    }
    
    public void ReleaseHandle(string address)
    {
        if (!_dicHandles.TryGetValue(address, out var handleInfo) || --handleInfo.nRefCount > 0)
            return;
        
        if (handleInfo.handle.IsValid())
            Addressables.Release(handleInfo.handle);

        _dicHandles.Remove(address);
    }

    public SceneInstance LoadScene(string sceneAddress, LoadSceneMode loadSceneMode, bool activeOnLoad)
    {
        if (_dicHandles.TryGetValue(sceneAddress, out var handleInfo))
            return (SceneInstance)handleInfo.handle.Result;
        
        handleInfo.handle = Addressables.LoadSceneAsync(sceneAddress, loadSceneMode, activeOnLoad);
        handleInfo.nRefCount = 1;
        handleInfo.handle.WaitForCompletion();
        _dicHandles.Add(sceneAddress, handleInfo);

        return (SceneInstance)handleInfo.handle.Result;
    }

    public void LoadSceneAsync(string sceneAddress, LoadSceneMode loadSceneMode, bool activeOnLoad, System.Action<SceneInstance> endCallback)
    {
        if (_dicHandles.TryGetValue(sceneAddress, out var handleInfo))
            endCallback?.Invoke((SceneInstance)handleInfo.handle.Result);
        
        handleInfo.handle = Addressables.LoadSceneAsync(sceneAddress, loadSceneMode, activeOnLoad);
        handleInfo.nRefCount = 1;
        if (endCallback != null)
            handleInfo.handle.Completed += (x) => endCallback((SceneInstance)x.Result);
        
        _dicHandles.Add(sceneAddress, handleInfo);
    }
    
    public void UnloadScene(string sceneAddress)
    {
        if (!_dicHandles.TryGetValue(sceneAddress, out var handleInfo))
            return;

        var unloadHandle = Addressables.UnloadSceneAsync((SceneInstance)handleInfo.handle.Result);
        unloadHandle.WaitForCompletion();
        _dicHandles.Remove(sceneAddress);

        if (handleInfo.handle.IsValid())
            Addressables.Release(handleInfo.handle);
    }
    
    public void UnloadSceneAsync(string sceneAddress)
    {
        if (!_dicHandles.TryGetValue(sceneAddress, out var handleInfo))
            return;

        var unloadHandle = Addressables.UnloadSceneAsync((SceneInstance)handleInfo.handle.Result);
        unloadHandle.Completed += (x) =>
        {
            if (handleInfo.handle.IsValid())
                Addressables.Release(handleInfo.handle);
        };

        _dicHandles.Remove(sceneAddress);
    }

    public void DontReleaseOnLoad(string address)
    {
        _setDontReleaseOnLoad.Add(address);
    }

    public void DoReleaseOnLoad(string address)
    {
        _setDontReleaseOnLoad.Remove(address);
    }

    public void ReleaseAllHandle(bool bContainDontDestroyOnLoad)
    {
        var handles = _dicHandles.ToList();
        foreach (var pair in handles)
        {
            var handleInfo = pair.Value;
            if (!handleInfo.handle.IsValid()
                || (!bContainDontDestroyOnLoad && _setDontReleaseOnLoad.Contains(pair.Key)))
                continue;

            Addressables.Release(handleInfo.handle);
            _dicHandles.Remove(pair.Key);
        }
    }
}