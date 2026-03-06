using TMPro;
using UnityEngine;

public class ServerTextUI : MonoBehaviour
{
    [Header("UI - Text")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;

    public bool HasText()
    {
        return !string.IsNullOrEmpty(messageInput.text);
    }

    public string GetText()
    {
        return messageInput.text;
    }

    public void ClearInput()
    {
        messageInput.text = "";
    }

    public void SetReceivedText(string text)
    {
        messageText.text = text;
    }
}