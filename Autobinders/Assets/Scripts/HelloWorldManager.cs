
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;

namespace HelloWorld
{
    public class HelloWorldManager : MonoBehaviour
    {

        public TextMeshProUGUI ipText;
        public TMP_InputField ipInputField;
        public GameObject nwMan;

        private bool writingIp = false;

        private void Start()
        {
        }

        private void Update()
        {
            if (writingIp == true)
            { 
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    writingIp = false;
                    ipInputField.interactable = false;
                    launchClient();
                }
            }
        }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else
            {
                StatusLabels();

                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (GUILayout.Button("Client"))
            {
                ipInputField.transform.gameObject.SetActive(true);
                ipInputField.interactable = true;
                writingIp = true;
            }

            if (GUILayout.Button("Server"))
            {
                NetworkManager.Singleton.StartServer();
                ipText.transform.gameObject.SetActive(true);
                ipText. text = $"IP: {LocalIPAddress()}";

            }
        }

        static void StatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }

        static void SubmitNewPosition()
        {
            if (GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            {
                if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
                {
                    foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                        NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid).GetComponent<HelloWorldPlayer>().Move();
                }
                else
                {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                    var player = playerObject.GetComponent<HelloWorldPlayer>();
                    player.Move();
                }
            }
        }
        public string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public void launchClient()
        {
            nwMan.GetComponent<Unity.Netcode.Transports.UNET.UNetTransport>().ConnectAddress = ipInputField.text;
            NetworkManager.Singleton.StartClient();
            ipInputField.transform.gameObject.SetActive(false);
        }
    }
}