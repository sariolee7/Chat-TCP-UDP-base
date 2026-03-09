using UnityEngine;
using UnityEngine.SceneManagement;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    //The exit button works both in Unity and in the application
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
