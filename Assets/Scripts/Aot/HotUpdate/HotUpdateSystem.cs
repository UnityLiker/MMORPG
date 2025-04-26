using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using JKFrame;
using UnityEngine.UIElements;
using HybridCLR;
public class HotUpdateSystem : MonoBehaviour
{
    [SerializeField] private TextAsset[] aotDllAssets;
    [SerializeField] private string[] hotUpdateDllFIleNames;
    [SerializeField] private string versionInfoAddressableKey;
    private Action<float> onPercentageForEachFile;
    private Action<bool> onOver;


    public void StartHotUpdate(Action<float> onPercentageForEachFile, Action<bool> onOver)
    {
        this.onPercentageForEachFile = onPercentageForEachFile;
        this.onOver = onOver;
        StartCoroutine(DoUpdateAddressables());
    }

    private bool succeed;
    private IEnumerator DoUpdateAddressables()
    {
        yield return Addressables.InitializeAsync();
        bool succeed = true;

        AsyncOperationHandle<List<string>> checkForCatalogUpdateHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkForCatalogUpdateHandle;
        if (checkForCatalogUpdateHandle.Status != AsyncOperationStatus.Succeeded)
        {
            succeed = false;
            JKLog.Error($"UpdateCatalogs失败:{checkForCatalogUpdateHandle.OperationException.Message}");
        }
        else
        {
            JKLog.Log("检测目录更新成功");

            //优先下载版本信息
            TextAsset versionInfoTextAsset = GetVersionInfo();
            JKLog.Log($"版本信息:{versionInfoTextAsset.text}");

            // 下载最新的目录
            if (checkForCatalogUpdateHandle.Result.Count > 0)
            {
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(checkForCatalogUpdateHandle.Result, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded) {
                    succeed = false;
                    JKLog.Error($"UpdateCatalogs失败:{updateCatalogsHandle.OperationException.Message}");
                }
                else
                {
                    List<IResourceLocator> locators = updateCatalogsHandle.Result;
                    foreach (IResourceLocator locator in locators) {
                        AsyncOperationHandle<long> sizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
                        yield return sizeHandle;
                        if (sizeHandle.Status != AsyncOperationStatus.Succeeded)
                        {
                            succeed = false;
                            JKLog.Error($"GetDownloadSizeAsync失败:{updateCatalogsHandle.OperationException.Message}");
                        }
                        else
                        {
                            JKLog.Log("下载目录更新成功");
                            ShowLoadingWindow();
                            long downloadSize = sizeHandle.Result;
                            if (downloadSize > 0)
                            {
                                //实际的下载
                                var downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.None, false);
                                while (downloadDependenciesHandle.IsDone)
                                {
                                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed)
                                    {
                                        succeed = false;
                                        JKLog.Error($"downloadDependencies失败:{ downloadDependenciesHandle.OperationException.Message}");
                                        break;
                                    }
                                    //分发下载进度
                                    float percentage = downloadDependenciesHandle.PercentComplete;
                                    onPercentageForEachFile?.Invoke(percentage);
                                    UpdateLoadingWindowProgress(downloadSize * percentage, downloadSize);
                                    JKLog.Log($"下载进度：{percentage}");
                                    yield return CoroutineTool.WaitForFrames();
                                }
                                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                                {
                                    JKLog.Error($"下载进度完成:{locator.LocatorId}");
                                }

                                Addressables.Release(downloadDependenciesHandle);
                            }
                        }

                        Addressables.Release(sizeHandle);
                    }
                    CloseLoadingWindow();
                }
            } 
            else JKLog.Log("无需更新");
        }

        Addressables.Release(checkForCatalogUpdateHandle);
        if (succeed) 
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();
        }
        onOver?.Invoke(succeed);
    }

    private void LoadHotUpdateDll()
    {
        for (int i = 0; i < hotUpdateDllFIleNames.Length; i++)
        {
            TextAsset dllTextAsset = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDllFIleNames[i]).WaitForCompletion();
            System.Reflection.Assembly.Load(dllTextAsset.bytes);
            JKLog.Log($"加载{hotUpdateDllFIleNames[i]}程序集");
        }
    }

    private void LoadMetaForAOTAssemblies()
    {
        for (int i = 0; i < aotDllAssets.Length; i++)
        {
            byte[] dllBytes = aotDllAssets[i].bytes;
            LoadImageErrorCode errorCode = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            JKLog.Log($"LoadMetaForAOTAssemblies: {aotDllAssets[i].name}, {errorCode}");
        }
    }

    private TextAsset GetVersionInfo()
    {
        Addressables.DownloadDependenciesAsync(versionInfoAddressableKey, true).WaitForCompletion();
        return Addressables.LoadAssetAsync<TextAsset>(versionInfoAddressableKey).WaitForCompletion();
    }

    private UI_LoadingWindow loadingWindow;
    private void ShowLoadingWindow()
    {
        loadingWindow = UISystem.Show<UI_LoadingWindow>();
        loadingWindow.Set("Loading....");
    }

    private void CloseLoadingWindow()
    {
        UISystem.Close<UI_LoadingWindow>();
        loadingWindow = null;
    }

    private void UpdateLoadingWindowProgress(float current, float max)
    {
        loadingWindow.UpdateDownloadProgress(current, max);
    }
}
