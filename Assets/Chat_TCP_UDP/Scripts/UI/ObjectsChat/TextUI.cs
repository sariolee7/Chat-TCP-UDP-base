using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextUI : MonoBehaviour, ITextUI
{
    [Header("Input")]
    [SerializeField] private TMP_InputField messageInput;

    [Header("Chat Content")]
    [SerializeField] private Transform chatContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject sentTextPrefab;
    [SerializeField] private GameObject receivedTextPrefab;

    [Header("Colors")]
    [SerializeField] private Color sentColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color receivedColor = new Color(0.8f, 0.8f, 0.8f);

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

    public void InstantiateSentText(string text)
    {
        GameObject msg = Instantiate(sentTextPrefab, chatContent);

        TMP_Text textComponent = msg.GetComponentInChildren<TMP_Text>();
        textComponent.text = text;

        Image bg = msg.GetComponent<Image>();
         bg.color = sentColor;
    }

    public void InstantiateReceivedText(string text)
    {
        GameObject msg = Instantiate(receivedTextPrefab, chatContent);

        TMP_Text textComponent = msg.GetComponentInChildren<TMP_Text>();
        textComponent.text = text;

        Image bg = msg.GetComponent<Image>();
        bg.color = receivedColor;
    }
}