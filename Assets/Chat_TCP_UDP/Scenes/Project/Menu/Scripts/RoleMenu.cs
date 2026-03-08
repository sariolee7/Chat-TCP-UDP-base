using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleMenu : MonoBehaviour
{
    public void SelectClient()
    {
        SceneManager.LoadScene("Udp_Client");
    }

    public void SelectServer()
    {
        SceneManager.LoadScene("Tcp_Server");
    }
}