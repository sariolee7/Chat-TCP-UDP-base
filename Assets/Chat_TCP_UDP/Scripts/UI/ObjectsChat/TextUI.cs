using System;
using TMPro;
using UnityEngine;

public class TextUI : MonoBehaviour, ITextUI
{
    [Header("UI - Text")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text messageText;

    public event Action<string> OnTextChanged;
    public event Action OnTextCleared;

    void Awake()
    {
        messageInput.onValueChanged.AddListener(HandleTextChanged);
    }

    void HandleTextChanged(string text)
    {
        OnTextChanged?.Invoke(text);
    }

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
        OnTextCleared?.Invoke();
    }

    public void SetReceivedText(string text)
    {
        messageText.text = text;
    }
}