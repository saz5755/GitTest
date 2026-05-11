using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUIManager : MonoBehaviour
{
    public TMP_Text chatLogText;
    public SocketClient socketClient;
    [Header("Chat")]
    public TMP_InputField inputField;
    public Button sendButton;
    
    [Header("Login")]
    public TMP_InputField idInputField;
    public TMP_InputField pwInputField;
    public Button loginButton;
    
    
    
    void Start()
    {
        sendButton.onClick.AddListener(OnClickSend);

        // connectButton.onClick.AddListener(OnClickConnect);

        inputField.onSubmit.AddListener(OnSubmitChat);
        
        loginButton.onClick.AddListener(OnClickLogin);

        inputField.interactable = false;
        
    }

    void OnClickSend()
    {
        if (string.IsNullOrEmpty(inputField.text))
            return;
        
        Debug.Log("Send");
        
        socketClient.SendChat(inputField.text);

        inputField.text = "";
    }

    public void AddMessage(string message)
    {
        chatLogText.text += message + "\n";
    }
    
    void OnSubmitChat(string value)
    {
        OnClickSend();

        inputField.ActivateInputField();
    }
    
    void OnClickLogin()
    {
        socketClient.Login(idInputField.text, pwInputField.text);
    }
    
    public void SetChatEnable(bool enable)
    {
        inputField.interactable = enable;
    }
}