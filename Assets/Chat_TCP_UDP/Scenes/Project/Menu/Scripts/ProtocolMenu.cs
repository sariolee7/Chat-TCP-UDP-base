using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolMenu : MonoBehaviour
{
    public void SelectClient()
    {
        Debug.Log("Select: Client");
        SceneManager.LoadScene("Client");
    }

    public void SelectServer()
    {
        Debug.Log("Select: Server");
        SceneManager.LoadScene("Server");
    }

        public void SelectTCP()
    {
        ProtocolState.useTCP = true;
        Debug.Log("Select: TCP");

    }

    public void SelectUDP()
    {
        ProtocolState.useTCP = false;
        Debug.Log("Select: UDP");
    }

}