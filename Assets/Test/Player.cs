using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    // Update is called once per frame
    void Update()
    {  
        if (!IsSpawned) return;
        if (IsOwner)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = new Vector3(h, 0, v).normalized;
            HandleMovementServerRpc(inputDir);
        }

        if (IsServer)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestClientRpc();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void HandleMovementServerRpc(Vector3 inputDir)
    {
        transform.Translate(Time.deltaTime * 10 * inputDir);
    }

    [ClientRpc]
    private void TestClientRpc()
    {
        GameObject.CreatePrimitive(PrimitiveType.Capsule).transform.position = transform.position;
    }
}
