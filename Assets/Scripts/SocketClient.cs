using System;
using System.IO;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SocketClient : MonoBehaviour
{
    TcpClient socket;

    NetworkStream stream;

    Thread receiveThread;

    public ChatUIManager uiManager;
    
    bool isRunning = true;
    
    
    public void Connect()
    {
        socket = new TcpClient("127.0.0.1", 5000);

        stream = socket.GetStream();

        receiveThread = new Thread(ReceiveMessage);

        receiveThread.IsBackground = true;

        receiveThread.Start();
    }
    
    void ReceiveMessage()
    {
        byte[] lengthBuffer = new byte[4];

        while (isRunning)
        {
            try
            {
                // 1. 길이 읽기
                ReadFull(stream, lengthBuffer, 4);
                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // 2. 데이터 읽기
                byte[] dataBuffer = new byte[length];
                ReadFull(stream, dataBuffer, length);
                
                // 3. JSON 변환
                string json = Encoding.UTF8.GetString(dataBuffer);
                Debug.Log(json);
                
                JObject obj = JObject.Parse(json);
                string type = obj["type"].ToString();

                if (type == "LOGIN_RESULT")
                {
                    LoginResultPacket result =
                        JsonConvert.DeserializeObject<LoginResultPacket>(json);

                    if (result.success)
                    {
                        UnityMainThreadDispatcher.Instance.Enqueue(() =>
                        {
                            uiManager.SetChatEnable(true);
                        });
                    }

                    continue;
                }

                ChatPacket packet =
                    JsonConvert.DeserializeObject<ChatPacket>(json);

                string finalMessage = "";

                switch (packet.type)
                {
                    case PacketType.CHAT:
                        finalMessage = $"[{packet.nickname}] {packet.message}";
                        break;

                    case PacketType.SYSTEM:
                        finalMessage = $"<color=yellow>{packet.message}</color>";
                        break;
                }

                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    uiManager.AddMessage(finalMessage);
                });
            }
            catch (IOException)
            {
                Debug.Log("Disconnected");
            }
            catch (SocketException)
            {
                Debug.Log("Socket Closed");
            }
            catch(Exception e)
            {
                Debug.Log(e.ToString());
            }
          
        }
    }

    public void SendChat(string msg)
    {
        ChatPacket packet = new ChatPacket();

        packet.type = PacketType.CHAT;
        packet.message = msg;

        SendPacket(packet);
        
        Console.WriteLine(
            $"[CLIENT SEND] nickname = {packet.nickname}");
        
    }
    
    void SendDisconnect()
    {
        if (socket == null)
            return;

        ChatPacket packet = new ChatPacket();

        packet.type = PacketType.DISCONNECT;
        // packet.nickname = nickname;
        packet.message = "";

        SendPacket(packet);
    }

    private void OnApplicationQuit()
    {
        SendDisconnect();
        
        isRunning = false;
        
        stream?.Close();

        socket?.Close();
    }
    
    public void Login(string id, string pw)
    {
        if (stream == null)
            Connect();

        LoginPacket packet = new();
        packet.type = PacketType.LOGIN;
        packet.id = id;
        packet.password = pw;

        SendPacket(packet);
    }
    
    static void ReadFull(NetworkStream stream, byte[] buffer, int size)
    {
        int offset = 0;

        while (offset < size)
        {
            int read = stream.Read(buffer, offset, size - offset);

            if (read <= 0)
                throw new Exception("Disconnected");

            offset += read;
        }
    }
    
    void SendPacket(object packet)
    {
        string json = JsonConvert.SerializeObject(packet);

        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

        byte[] length = BitConverter.GetBytes(jsonBytes.Length);

        stream.Write(length, 0, 4);
        stream.Write(jsonBytes, 0, jsonBytes.Length);
        
    }
    
    public void EnterRoom(int roomId)
    {
        EnterRoomPacket packet = new();

        packet.type = PacketType.ENTER_ROOM;

        packet.roomId = roomId;

        SendPacket(packet);
    }
    
}