using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScript : MonoBehaviour
{
    [SerializeField]
    private GameObject usernameInput;
    [SerializeField]
    private GameObject passwordInput;

    private TcpClient client;

	private void ConnectToTcpServer()
	{
		client = new TcpClient(GetLocalIPAddress(), 5000);
	}
 
	public void Login()
	{
        ConnectToTcpServer();

        SendMessage();

        RecieveMessage();
	}

	private void RecieveMessage()
    {
        try
        {
            NetworkStream ns = client.GetStream();
            Byte[] bytes = new Byte[1024];
            Debug.Log("client running");
            // Get a stream object for reading 				
            using (NetworkStream stream = client.GetStream())
            {
                int length;
                // Read incomming stream into byte arrary. 					
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var incommingData = new byte[length];
                    Array.Copy(bytes, 0, incommingData, 0, length);
                    // Convert byte array to string message.
                    string serverMessage = Encoding.ASCII.GetString(incommingData);
                    Debug.Log("server message received as: " + serverMessage);

                    if (serverMessage == "ok")
                        LoginSucces();
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

	private void SendMessage()
	{
		try
		{
			Debug.Log(client);
			// Get a stream object for writing. 			
			NetworkStream ns = client.GetStream();

            JObject json = new JObject();
            json["username"] = usernameInput.transform.Find("Text").GetComponent<Text>().text;
            json["password"] = passwordInput.transform.Find("Text").GetComponent<Text>().text;

            //var bytes = Encoding.ASCII.GetBytes(json.ToString());
            //ns.Write(bytes, 0, bytes.Length);
            Debug.Log("sending: " + json.ToString());

            var data = Encoding.ASCII.GetBytes(json.ToString());
            ns.Write(data, 0, data.Length);
        }
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private void LoginSucces()
    {
        SceneManager.LoadScene("Login_Scene");
    }
    private static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}
