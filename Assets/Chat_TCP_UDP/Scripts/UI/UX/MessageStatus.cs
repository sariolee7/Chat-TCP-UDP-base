using System;

public enum MessageStatus
{
    Sent,
    Failed
}

public class SendResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }

    private SendResult() { }

    public static SendResult Success()
    {
        return new SendResult { IsSuccess = true };
    }

    public static SendResult Failure(string errorMessage)
    {
        return new SendResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
