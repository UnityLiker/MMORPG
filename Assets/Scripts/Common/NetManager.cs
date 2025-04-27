using Unity.Netcode;
using UnityEngine;

public class NetManager : NetworkManager
{
    public static NetManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void InitClient()
    { 
        StartClient();
    }

    public void InitServer()
    {
        StartServer();
    }
    
    public NetworkObject SpawnObject(ulong clientID, GameObject prefab, Vector3 pos)
    {
        // TODO:后续增加网络对象 对象池
        NetworkObject networkObject = Instantiate(prefab).GetComponent<NetworkObject>();
        networkObject.transform.position = pos;
        networkObject.SpawnWithOwnership(clientID);

        return networkObject;
    }
}
