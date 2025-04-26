using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class TestHotUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("¿ªÊ¼");

        TextAsset dllText = Addressables.LoadAssetAsync<TextAsset>("HotUpdate.dll").WaitForCompletion();
        System.Reflection.Assembly.Load(dllText.bytes);
        Addressables.InstantiateAsync("GameObject").WaitForCompletion();

        Debug.Log("½áÊø");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
