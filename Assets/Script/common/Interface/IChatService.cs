using System;
using System.Collections;

public interface IChatService
{
    IEnumerator SendMessageCoroutine(
        string userText,
        Action<string> onChunk,
        Action<Exception> onError
    );

}
