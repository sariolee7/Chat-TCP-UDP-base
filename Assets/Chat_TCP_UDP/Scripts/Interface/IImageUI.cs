using System;
using UnityEngine;

public interface IImageUI
{
    event Action OnImageLoaded;
    event Action OnImageCleared;

    bool HasImage();
    Texture2D GetLoadedTexture();

    void LoadImageFromExplorer();
    void ClearImage();
}