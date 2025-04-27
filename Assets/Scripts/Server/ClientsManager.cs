#if UNITY_SERVER || SERVER_EDITOR_TEST
using JKFrame;
using UnityEngine;

public class ClientsManager : SingletonMono<ClientsManager>
{
    public GameObject playerPrefab;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }


    public void Init()
    {
        NetManager.Instance.OnClientConnectedCallback += OnClientConnectedCallback;
    }


    // Update is called once per frame
    void OnClientConnectedCallback(ulong clientID)
    {
        // TODO: Ԥ���塢����Ⱥ�����������
        NetManager.Instance.SpawnObject(clientID, playerPrefab, Vector3.zero);
    }
}

#endif