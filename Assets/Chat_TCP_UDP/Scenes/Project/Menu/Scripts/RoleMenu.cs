using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleMenu : MonoBehaviour
{
    public void SelectClient()
    {
        ProtocolState.role = "Client";

        if (ProtocolState.useTCP)
        {
            SceneManager.LoadScene("Tcp_Client");
        }
        else
        {
            SceneManager.LoadScene("Udp_Client");
        }
    }

    public void SelectServer()
    {
        ProtocolState.role = "Server";

        if (ProtocolState.useTCP)
        {
            SceneManager.LoadScene("Tcp_Server");
        }
        else
        {
            SceneManager.LoadScene("Udp_Server");
        }
    }
}