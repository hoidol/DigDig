using System;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;

public class AddressableMgr
{
    private static List<AsyncOperationHandle> handles = new List<AsyncOperationHandle>();
    public static async UniTask<T> Load<T>(string address)
    {
        T data = default;
        try
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Addressables.LoadAssetAsync<>() is failed({handle.Status}) for '{address}'");
                throw handle.OperationException;
            }

            data = handle.Result;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Addressables.LoadAssetAsync<>() is failed({ex.Message}) for '{address}'");
        }

        return data;
    }

    public static async UniTask Load<T>(string address, Action<T[]> result)
    {
        T[] data = Array.Empty<T>();
        try
        {
            var handle = Addressables.LoadAssetsAsync<T>(address, null, Addressables.MergeMode.Union);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({handle.Status}) for label '{address}'");
                throw handle.OperationException;
            }

            data = handle.Result.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({ex.Message}) for label '{address}'");
        }

        result(data);
    }


    public static async UniTask LoadAll<T>(string path, string[] addresses, Action<T[]> result)
    {
        T[] data = Array.Empty<T>();
        for (int i = 0; i < addresses.Length; i++)
        {
            addresses[i] = Path.Combine(path, addresses[i]);
        }

        try
        {
            var handle = Addressables.LoadAssetsAsync<T>(addresses, null, Addressables.MergeMode.Union);
            handles.Add(handle);

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({handle.Status}) for '{string.Join(",", addresses)}'");
                throw handle.OperationException;
            }

            data = handle.Result.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({ex.Message}) for '{string.Join(",", addresses)}'");
        }

        result(data);
    }
    public static async UniTask LoadAllByLabel<T>(string label, Action<T[]> result)
    {
        // Debug.Log("LoadAllByLabel 1");
        T[] data = Array.Empty<T>();
        try
        {
            var handle = Addressables.LoadAssetsAsync<T>(label, null, Addressables.MergeMode.Union);
            handles.Add(handle);
            // Debug.Log("LoadAllByLabel 1");
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({handle.Status}) for label '{label}'");
                throw handle.OperationException;
            }

            data = handle.Result.ToArray();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Addressables.LoadAssetsAsync<>() is failed({ex.Message}) for label '{label}'");
        }
        // Debug.Log($"LoadAllByLabel555 {data.Length}");
        result(data);
    }


}