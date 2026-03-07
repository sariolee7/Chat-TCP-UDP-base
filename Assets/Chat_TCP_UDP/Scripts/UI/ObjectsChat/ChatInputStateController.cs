using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatInputStateController : MonoBehaviour
{

    public static ChatInputStateController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextUI textUI;
    [SerializeField] private ImageUI imageUI;
    [SerializeField] private AudioUI audioUI;

    [SerializeField] private Vector2 imagePreviewTarget;
    [SerializeField] private Vector2 audioPreviewTarget;

    private TMP_InputField textInput;
    private RectTransform imagePreview;
    private RectTransform audioPreview;

    private Button imageButton;
    private Button audioButton;
    private Button sendButton;

    private Vector2 imagePreviewStart;

    private Vector2 audioPreviewStart;

    private enum ChatState
    {
        Normal,
        ImageLoaded,
        AudioLoaded
    }

    private ChatState currentState = ChatState.Normal;

    void Awake()
    {

        Instance = this;

        var elements = GetComponentsInChildren<UIChatElement>(true);

        foreach (var element in elements)
        {
            switch (element.elementType)
            {
                case UIChatElement.ElementType.TextInput:
                    textInput = element.InputField;
                    break;

                case UIChatElement.ElementType.ImagePreview:
                    imagePreview = element.GetComponent<RectTransform>();
                    break;

                case UIChatElement.ElementType.AudioPreview:
                    audioPreview = element.GetComponent<RectTransform>();
                    break;

                case UIChatElement.ElementType.ImageButton:
                    imageButton = element.Button;
                    break;

                case UIChatElement.ElementType.AudioButton:
                    audioButton = element.Button;
                    break;

                case UIChatElement.ElementType.SendButton:
                    sendButton = element.Button;
                    break;
            }
        }

        imagePreviewStart = imagePreview.anchoredPosition;
        audioPreviewStart = audioPreview.anchoredPosition;
        sendButton.interactable = false;
    }

    void OnEnable()
    {
        audioUI.OnAudioLoaded += HandleAudioLoaded;
        imageUI.OnImageLoaded += HandleImageLoaded;

        audioUI.OnAudioCleared += ResetState;
        imageUI.OnImageCleared += ResetState;

        textInput.onValueChanged.AddListener(CheckSendButton);
    }

    void OnDisable()
    {
        audioUI.OnAudioLoaded -= HandleAudioLoaded;
        imageUI.OnImageLoaded -= HandleImageLoaded;

        audioUI.OnAudioCleared -= ResetState;
        imageUI.OnImageCleared -= ResetState;

        textInput.onValueChanged.RemoveListener(CheckSendButton);
    }

    void CheckSendButton(string text)
    {
        sendButton.interactable = !string.IsNullOrEmpty(text);
    }

    void HandleImageLoaded()
    {
        if (currentState == ChatState.AudioLoaded)
            return;

        currentState = ChatState.ImageLoaded;

        audioButton.interactable = false;
        sendButton.interactable = true;

        StopAllCoroutines();
        StartCoroutine(LerpPreview(imagePreview, imagePreviewStart, imagePreviewTarget));
    }

    void HandleAudioLoaded()
    {
        if (currentState != ChatState.Normal)
            return;

        currentState = ChatState.AudioLoaded;

        textInput.interactable = false;
        textInput.gameObject.SetActive(false);

        imageButton.interactable = false;
        sendButton.interactable = true;

        StopAllCoroutines();
        StartCoroutine(LerpPreview(audioPreview, audioPreviewStart, audioPreviewTarget));
    }

    public void OnMessageSent()
    {
        ResetState();
    }

    public void ResetState()
    {
        currentState = ChatState.Normal;

        textInput.interactable = true;
        textInput.gameObject.SetActive(true);
        textInput.text = "";

        imageButton.interactable = true;
        audioButton.interactable = true;

        sendButton.interactable = false;

        imagePreview.anchoredPosition = imagePreviewStart;
        audioPreview.anchoredPosition = audioPreviewStart;

    }

    IEnumerator LerpPreview(RectTransform target, Vector2 start, Vector2 end)
    {
        float time = 0;
        float duration = 0.8f;

        while (time < duration)
        {
            time += Time.deltaTime;

            target.anchoredPosition = Vector2.Lerp(start, end, time / duration);

            yield return null;
        }

        target.anchoredPosition = end;
    }
}