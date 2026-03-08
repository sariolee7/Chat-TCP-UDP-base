using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ImageMessageUI : MonoBehaviour
{
    [SerializeField] private RawImage messageImage;
    [SerializeField] private Image statusImage;
    [SerializeField] private TMP_Text errorText;

    [Header("Status Sprites")]
    [SerializeField] private Sprite sentSprite;
    [SerializeField] private Sprite failedSprite;

    public void Initialize(Texture2D texture)
    {
        if (messageImage != null)
            messageImage.texture = texture;

        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }

    public void Initialize(byte[] imageData)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);

        if (messageImage != null)
            messageImage.texture = tex;

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
