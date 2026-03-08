using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIChatElement : MonoBehaviour
{
    public enum ElementType
    {
        TextInput,
        ImagePreview,
        AudioPreview,
        ImageButton,
        AudioButton,
        SendButton
    }

    public ElementType elementType;

    public Button Button => GetComponent<Button>();
    public TMP_InputField InputField => GetComponent<TMP_InputField>();
}