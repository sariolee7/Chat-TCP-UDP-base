using SFB;
using System;
using System.IO;
using TMPro;
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
    private Texture2D _receivedTexture;

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

    public void InstantiateSentImage(Texture2D texture)
    {
        GameObject msg = Instantiate(sentImagePrefab, chatContent);

        Image bg = msg.GetComponent<Image>();
        bg.color = sentColor;

        RawImage image = msg.GetComponentInChildren<RawImage>();
        image.texture = texture;
    }

    public void InstantiateReceivedImage(byte[] data)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);

        _receivedTexture = tex;

        GameObject msg = Instantiate(receivedImagePrefab, chatContent);

        Image bg = msg.GetComponent<Image>();
        bg.color = receivedColor;

        RawImage image = msg.GetComponentInChildren<RawImage>();
        image.texture = _receivedTexture;
    }
}