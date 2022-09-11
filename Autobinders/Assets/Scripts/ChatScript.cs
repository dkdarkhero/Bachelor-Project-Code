using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatScript : MonoBehaviour
{
    [SerializeField]
    private GameObject chatPanel, textObject;
    [SerializeField]
    private InputField chatBox;
    [SerializeField]
    private ChatTCPClient chatTCPClient;

    private List<GameObject> messages = new List<GameObject>();
    private int maxChatMessages = 20;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                chatTCPClient.SendMessageToChatServer(chatBox.text);
                chatBox.text = "";
            }
        }
    }

    public void SendMessageToChat(string msg)
    {
        //if (messages.Count > maxChatMessages)
        //{
        //    messages.RemoveAt(0);
        //}

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newText.GetComponent<Text>().text = msg;

        messages.Add(newText);
    }
}
