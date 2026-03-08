using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextMessageUI : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Image statusImage;
    [SerializeField] private TMP_Text errorText;

    [Header("Status Sprites")]
    [SerializeField] private Sprite sentSprite;
    [SerializeField] private Sprite failedSprite;

    public void Initialize(string text)
    {
        if (messageText != null)
            messageText.text = text;

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public void SetSent()
    {
        if (statusImage != null && sentSprite != null)
            statusImage.sprite = sentSprite;

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public void SetFailed(string error)
    {
        if (statusImage != null && failedSprite != null)
            statusImage.sprite = failedSprite;

        if (errorText != null)
        {
            errorText.gameObject.SetActive(true);
            errorText.text = error;
        }
    }

    public void HideStatus()
    {
        if (statusImage != null)
            statusImage.gameObject.SetActive(false);

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }
}
