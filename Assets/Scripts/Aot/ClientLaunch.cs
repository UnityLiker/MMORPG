using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ClientLaunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool success) => {
            if (success) {
                Addressables.InstantiateAsync("GameObject").WaitForCompletion();
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
