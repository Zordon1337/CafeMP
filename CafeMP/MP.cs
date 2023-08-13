
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace CafeMP
{
    class MP:MonoBehaviour
    {
        bool isServerCreated = false;
        bool isConnected = false;
        bool AlreadyChecked = false;
        string logs = "";
        int port = 9999;
        string IP = "127.0.0.1";
        GameObject Player1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject Player2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        public void Start()
        {
            
        }
        private async void StartServer()
        {
            await Task.Run(() =>
            {
                Player2.SetActive(false);
                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start();
                logs += $"\nServer started listening on 0.0.0.0:{port}";
                while(true)
                {
                    TcpClient client = server.AcceptTcpClient();  

                    NetworkStream ns = client.GetStream(); 

                     

                    while (client.Connected)  
                    {
                        byte[] buffer = new byte[client.ReceiveBufferSize];
                        int bytesRead = ns.Read(buffer, 0, client.ReceiveBufferSize);
                        string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        if(dataReceived.Contains("Pos"))
                        {
                            logs += "Got Pos Packet from client";
                            string[] datagot = dataReceived.Split('|');
                            Player2.transform.position = new Vector3(float.Parse(datagot[1]), float.Parse(datagot[2]), float.Parse(datagot[3]));
                            Vector3 pos = Player1.transform.position;
                            byte[] datatowrite = new byte[1024];
                            datatowrite = Encoding.Default.GetBytes($"Pos|{pos.x}|{pos.y}|{pos.z}");
                            ns.Write(datatowrite,0, datatowrite.Length);
                        }
                        if(dataReceived.Contains("PlayerWelcome"))
                        {

                            logs += "Got PlayerWelcome Packet from client";
                            Player2.SetActive(true);
                        }
                        if (dataReceived.Contains("P1Pos"))
                        {
                            logs += "Got P1Pos Packet from client";
                            byte[] data = new byte[1024];
                            data = Encoding.Default.GetBytes("P1Pos|"+GameObject.FindGameObjectWithTag("Player").transform.position.x + "|" + GameObject.FindGameObjectWithTag("Player").transform.position.y + "|" + GameObject.FindGameObjectWithTag("Player").transform.position.z);
                            ns.Write(data, 0, data.Length);
                        }
                        if(dataReceived.Contains("Client connected"))
                        {
                            byte[] hello = new byte[1024];
                            hello = Encoding.Default.GetBytes("OK");
                            ns.Write(hello, 0, hello.Length);
                        }
                        client.Close();
                    }
                }
            });
        }
        private void Write(NetworkStream ns, string packet ,string MSG)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(MSG);
            ns.Write(data, 0, data.Length);
        }
        private async void ConnectToServer()
        {
            TcpClient client = new TcpClient(IP,9999);
            NetworkStream stream = client.GetStream();
            
            byte[] buffer = new byte[client.ReceiveBufferSize];
            int bytesRead = stream.Read(buffer, 0, client.ReceiveBufferSize);
            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            logs += "Got response: " + dataReceived;
            if (dataReceived.Contains("OK"))
            {
                logs += "\nConnection to server was established";
            }
            //finding player(our) pos
            Vector3 OldPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            // sent after connecting
            Write(stream, "Pos", $"|{OldPos.x}|{OldPos.y}|{OldPos.z}");
            while (true)
            {
                Thread.Sleep(500);
                Write(stream, "Pos", $"|{OldPos.x}|{OldPos.y}|{OldPos.z}");
                byte[] bf = new byte[client.ReceiveBufferSize];
                int br = stream.Read(bf, 0, client.ReceiveBufferSize);
                string dr = Encoding.ASCII.GetString(bf, 0, br);
                string[] dg = dr.Split('|');
                Player1.transform.position = new Vector3(float.Parse(dg[1]), float.Parse(dg[2]), float.Parse(dg[3]));

            }
        }
        public void OnGUI()
        {
            GUI.Box(new Rect(0f, 0f, 250f, 225f), "CafeMP");
            if (!isServerCreated && !isConnected)
            {
                
                GUI.TextArea(new Rect(0f, 25f, 250f, 30f), IP);
                if (GUI.Button(new Rect(0f, 175f, 250f, 25f), "Create Server"))
                {
                    isServerCreated = true;
                    StartServer();
                }
                if (GUI.Button(new Rect(0f, 200f, 250f, 25f), "Join Server"))
                {
                    isConnected = true;
                }
                
            } else
            {

            }
            GUI.Box(new Rect(0f, 250f, 500f, 325f), "Logs");
            if(isServerCreated ^ isConnected)
            {
                GUI.TextField(new Rect(25f, 260f, 450f, 310f), logs);
            }

        }
        public void Update()
        {

        }
    }
}
