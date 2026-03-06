using SFB;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ClientImageUI : MonoBehaviour
{
    [Header("UI - Images")]
    [SerializeField] private RawImage receivedImage;
    [SerializeField] private RawImage sentImage;

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
    }

    public void SetReceivedImage(byte[] data)
    {
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);

        _receivedTexture = tex;
        receivedImage.texture = _receivedTexture;
    }
}