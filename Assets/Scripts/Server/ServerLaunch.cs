using JK.Log;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerLaunch : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        InitServers();
        SceneManager.LoadScene("Game");
    }

    // Update is called once per frame
    void InitServers()
    {
        ClientsManager.Instance.Init();
        NetManager.Instance.InitServer();
        JKLog.Succeed("InitServers Succeed");
    }
}
