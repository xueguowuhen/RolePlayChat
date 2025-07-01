using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatRootItem : MonoBehaviour
{
    public TMP_Text ChatContent;
    public TMP_Text ChatName;
    public Image ChatAvatar;

    public void SetUI(string textContent, string chatName, Sprite sprite = null)
    {
        ChatContent.text = textContent;

        ChatName.text = chatName;
        if (sprite != null)
        {
            ChatAvatar.sprite = sprite;
            ChatAvatar.gameObject.SetActive(true);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
