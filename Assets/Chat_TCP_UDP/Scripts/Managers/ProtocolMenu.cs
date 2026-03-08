using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolMenu : MonoBehaviour
{
    public void SelectClient()
    {
        ChatManager.Instance.ChangeScene("Client");
    }

    public void SelectServer()
    {
        ChatManager.Instance.ChangeScene("Server");
    }

        public void SelectTCP()
    {
        ProtocolState.useTCP = true;

    }

    public void SelectUDP()
    {
        ProtocolState.useTCP = false;
    }

}