using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageStatusUI : MonoBehaviour
{
    [Header("Connection Status")]
    [SerializeField] private Image connectionStatusImage;
    [SerializeField] private TMP_Text connectionStatusText;

    [Header("Sprites")]
    [SerializeField] private Sprite connectedSprite;
    [SerializeField] private Sprite disconnectedSprite;

    [Header("Colors")]
    [SerializeField] private Color connectedColor = Color.green;
    [SerializeField] private Color disconnectedColor = Color.red;

    public void SetConnected()
    {
        if (connectionStatusImage != null && connectedSprite != null)
            connectionStatusImage.sprite = connectedSprite;

        if (connectionStatusText != null)
        {
            connectionStatusText.text = "Connected";
            connectionStatusText.color = connectedColor;
        }
    }

    public void SetDisconnected(string reason = "Disconnected")
    {
        if (connectionStatusImage != null && disconnectedSprite != null)
            connectionStatusImage.sprite = disconnectedSprite;

        if (connectionStatusText != null)
        {
            connectionStatusText.text = reason;
            connectionStatusText.color = disconnectedColor;
        }
    }
}
