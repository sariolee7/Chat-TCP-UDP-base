using SFB;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ImageUI : MonoBehaviour, IImageUI
{
    [Header("Preview")]
    [SerializeField] private RawImage sentImage;

    [Header("Chat Content")]
    [SerializeField] private Transform chatContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject sentImagePrefab;
    [SerializeField] private GameObject receivedImagePrefab;

    [Header("Colors")]
    [SerializeField] private Color sentColor = new Color(0.3f, 0.6f, 1f);
    [SerializeField] private Color receivedColor = new Color(0.8f, 0.8f, 0.8f);

    public event Action OnImageLoaded;
    public event Action OnImageCleared;

    private Texture2D _loadedTexture;
    private ImageMessageUI _lastSentMessageUI;

    public bool HasImage()
    {
        return _loadedTexture != null;
    }

    public Texture2D GetLoadedTexture()
    {
        return _loadedTexture;
    }

    public void LoadImageFromExplorer()
    {
        var extensions = new[]
        {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
        };

        string[] paths = StandaloneFileBrowser.OpenFilePanel(
            "Select Image",
            "",
            extensions,
            false
        );

        if (paths.Length == 0)
            return;

        string path = paths[0];

        if (!File.Exists(path))
            return;

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(File.ReadAllBytes(path));

        _loadedTexture = tex;
        sentImage.texture = _loadedTexture;

        OnImageLoaded?.Invoke();
    }

    public void ClearImage()
    {
        _loadedTexture = null;
        sentImage.texture = null;

        OnImageCleared?.Invoke();
    }

    public GameObject InstantiateSentImage(Texture2D texture)
    {
        GameObject msg = Instantiate(sentImagePrefab, chatContent);

        ImageMessageUI imageMessageUI = msg.GetComponent<ImageMessageUI>();
        if (imageMessageUI != null)
        {
            imageMessageUI.Initialize(texture);
            _lastSentMessageUI = imageMessageUI;
        }
        else
        {
            RawImage image = msg.GetComponentInChildren<RawImage>();
            if (image != null)
                image.texture = texture;
        }

        Image bg = msg.GetComponent<Image>();
        if (bg != null)
            bg.color = sentColor;

        return msg;
    }

    public GameObject InstantiateReceivedImage(byte[] data)
    {
        GameObject msg = Instantiate(receivedImagePrefab, chatContent);

        ImageMessageUI imageMessageUI = msg.GetComponent<ImageMessageUI>();
        if (imageMessageUI != null)
        {
            imageMessageUI.Initialize(data);
            imageMessageUI.HideStatus();
        }
        else
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(data);

            RawImage image = msg.GetComponentInChildren<RawImage>();
            if (image != null)
                image.texture = tex;
        }

        Image bg = msg.GetComponent<Image>();
        if (bg != null)
            bg.color = receivedColor;

        return msg;
    }

    public void SetLastMessageSent()
    {
        if (_lastSentMessageUI != null)
            _lastSentMessageUI.SetSent();
    }

    public void SetLastMessageFailed(string error)
    {
        if (_lastSentMessageUI != null)
            _lastSentMessageUI.SetFailed(error);
    }
}