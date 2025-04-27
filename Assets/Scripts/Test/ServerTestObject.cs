using Unity.Netcode;
using UnityEngine;

public class ServerTestObject : NetworkBehaviour
{
    public float moveSpeed;
    public static ServerTestObject Instance;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
#if UNITY_SERVER || SERVER_EDITOR_TEST
        Instance = this;
#endif
    }

    private void Update()
    {
#if UNITY_SERVER || SERVER_EDITOR_TEST
        if (IsServer)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovement(inputDir);
        }
#endif
    }

    private void HandleMovement(Vector3 inputDir)
    {
        transform.Translate(Time.deltaTime * moveSpeed * inputDir);
    }
}
