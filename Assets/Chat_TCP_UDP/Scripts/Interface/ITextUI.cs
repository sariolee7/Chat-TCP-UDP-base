using System;

public interface ITextUI
{
    event Action<string> OnTextChanged;
    event Action OnTextCleared;

    bool HasText();
    string GetText();
    void ClearInput();
    void SetReceivedText(string text);
}