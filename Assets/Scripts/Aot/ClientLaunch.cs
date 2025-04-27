using JKFrame;
using UnityEngine;

public class ClientLaunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<HotUpdateSystem>().StartHotUpdate(null, (bool success) => {
            if (success) {
                OnHotUpdateSucceed();
            }
        });
    }

    // Update is called once per frame
    void OnHotUpdateSucceed()
    {
        NetManager.Instance.InitClient();
        SceneSystem.LoadScene("Game");
        JKLog.Succeed("InitClient");
    }
}
