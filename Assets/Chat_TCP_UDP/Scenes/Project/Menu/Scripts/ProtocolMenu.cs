using UnityEngine;
using UnityEngine.SceneManagement;

public class ProtocolMenu : MonoBehaviour
{
    public void SelectTCP()
    {
        ProtocolState.useTCP = true;
        Debug.Log("Protocolo seleccionado: TCP");
        SceneManager.LoadScene("Cliente_Server");
    }

    public void SelectUDP()
    {
        ProtocolState.useTCP = false;
        Debug.Log("Protocolo seleccionado: UDP");
        SceneManager.LoadScene("Cliente_Server");
    }
}