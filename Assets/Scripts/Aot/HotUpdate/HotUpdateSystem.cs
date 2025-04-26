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
            JKLog.Error($"UpdateCatalogsʧ��:{checkForCatalogUpdateHandle.OperationException.Message}");
        }
        else
        {
            JKLog.Log("���Ŀ¼���³ɹ�");

            // �������µ�Ŀ¼
            if (checkForCatalogUpdateHandle.Result.Count > 0)
            {
                AsyncOperationHandle<List<IResourceLocator>> updateCatalogsHandle = Addressables.UpdateCatalogs(checkForCatalogUpdateHandle.Result, false);
                yield return updateCatalogsHandle;
                if (updateCatalogsHandle.Status != AsyncOperationStatus.Succeeded) {
                    succeed = false;
                    JKLog.Error($"UpdateCatalogsʧ��:{updateCatalogsHandle.OperationException.Message}");
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
                            JKLog.Error($"GetDownloadSizeAsyncʧ��:{updateCatalogsHandle.OperationException.Message}");
                        }
                        else
                        {
                            JKLog.Log("����Ŀ¼���³ɹ�");
                            long downloadSize = sizeHandle.Result;
                            if (downloadSize > 0)
                            {
                                //ʵ�ʵ�����
                                var downloadDependenciesHandle = Addressables.DownloadDependenciesAsync(locator.Keys, Addressables.MergeMode.None, false);
                                while (downloadDependenciesHandle.IsDone)
                                {
                                    if (downloadDependenciesHandle.Status == AsyncOperationStatus.Failed)
                                    {
                                        succeed = false;
                                        JKLog.Error($"downloadDependenciesʧ��:{ downloadDependenciesHandle.OperationException.Message}");
                                        break;
                                    }
                                    //�ַ����ؽ���
                                    float percentage = downloadDependenciesHandle.PercentComplete;
                                    onPercentageForEachFile?.Invoke(percentage);
                                    JKLog.Log($"���ؽ��ȣ�{percentage}");
                                    yield return CoroutineTool.WaitForFrames();
                                }
                                if (downloadDependenciesHandle.Status == AsyncOperationStatus.Succeeded)
                                {
                                    JKLog.Error($"���ؽ������:{locator.LocatorId}");
                                    break;
                                }

                                Addressables.Release(downloadDependenciesHandle);
                            }
                        }

                        Addressables.Release(sizeHandle);
                    }
                }
            } 
            else JKLog.Log("�������");
        }

        onOver?.Invoke(succeed);
        Addressables.Release(checkForCatalogUpdateHandle);
        if (succeed)
        {
            LoadHotUpdateDll();
            LoadMetaForAOTAssemblies();
        }
    }

    private void LoadHotUpdateDll()
    {
        for (int i = 0; i < hotUpdateDllFIleNames.Length; i++)
        {
            TextAsset dllTextAsset = Addressables.LoadAssetAsync<TextAsset>(hotUpdateDllFIleNames[i]).WaitForCompletion();
            System.Reflection.Assembly.Load(dllTextAsset.bytes);
            JKLog.Log($"����{hotUpdateDllFIleNames[i]}����");
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
}
