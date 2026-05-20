using System;
using System.IO;
using System.Net;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class SocketClient : MonoBehaviour
{
    TcpClient socket;
    
    UdpClient udp;
    
    Thread udpReceiveThread;

    NetworkStream stream;

    Thread receiveThread;
    
    public string myNickname;

    public ChatUIManager uiManager;
    
    public PlayerManager playerManager;
    
    bool isRunning = true;

    // "127.0.0.1" 자기자신 IP
    public string connectIP = "172.30.201.117";
    
    public void Connect()
    {
        socket = new TcpClient(connectIP, 5000);
        
        stream = socket.GetStream();

        receiveThread = new Thread(ReceiveMessage);

        receiveThread.IsBackground = true;

        receiveThread.Start();
        
        // UDP 생성
        udp = new UdpClient(0);
        udp.Connect(connectIP, 6000);        
        
        udpReceiveThread = new Thread(UdpReceiveLoop);

        udpReceiveThread.IsBackground = true;

        udpReceiveThread.Start();
        
    }
    
void ReceiveMessage()
{
    byte[] lengthBuffer = new byte[4];

    while (isRunning)
    {
        try
        {
            // 1. 패킷 길이 읽기
            ReadFull(stream, lengthBuffer, 4);

            int length =
                BitConverter.ToInt32(lengthBuffer, 0);

            // 2. 패킷 데이터 읽기
            byte[] dataBuffer = new byte[length];

            ReadFull(stream, dataBuffer, length);

            // 3. JSON 문자열 변환
            string json =
                Encoding.UTF8.GetString(dataBuffer);

            // Debug.Log($"[RECV] {json}");

            // 4. type 파싱
            JObject obj = JObject.Parse(json);

            PacketType type =
                (PacketType)obj["type"].Value<int>();

            switch (type)
            {
                case PacketType.LOGIN_RESULT:
                {
                    LoginResultPacket packet =
                        JsonConvert.DeserializeObject<LoginResultPacket>(json);

                    if (packet.success)
                    {
                        UnityMainThreadDispatcher.Instance.Enqueue(() =>
                        {
                            uiManager.SetChatEnable(true);
                            
                            SendUdpConnect();
                        });
                    }
                    else
                    {
                        Debug.Log(packet.message);
                    }

                    break;
                }

                case PacketType.CHAT:
                case PacketType.SYSTEM:
                {
                    ChatPacket packet =
                        JsonConvert.DeserializeObject<ChatPacket>(json);

                    string finalMessage = "";

                    switch (packet.type)
                    {
                        case PacketType.CHAT:

                            finalMessage =
                                $"[{packet.nickname}] {packet.message}";

                            break;

                        case PacketType.SYSTEM:

                            finalMessage =
                                $"<color=yellow>{packet.message}</color>";

                            break;
                    }

                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        uiManager.AddMessage(finalMessage);
                    });

                    break;
                }
                
                case PacketType.SPAWN:
                {
                    SpawnPacket packet =
                        JsonConvert.DeserializeObject<SpawnPacket>(json);

                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        playerManager.CreatePlayer(
                            packet.nickname,
                            new Vector3(
                                packet.x,
                                packet.y,
                                packet.z),
                            packet.rotY,
                            packet.isMove);
                    });

                    break;
                }

                case PacketType.MOVE:
                {
                    MoveBroadcastPacket packet =
                        JsonConvert.DeserializeObject<MoveBroadcastPacket>(json);

                    UnityMainThreadDispatcher.Instance.Enqueue(() =>
                    {
                        playerManager.MovePlayer(
                            packet.nickname,
                            new Vector3(
                                packet.position.X,
                                packet.position.Y,
                                packet.position.Z),
                            packet.rotY,
                            packet.isMove,
                            packet.tick);
                    });

                    break;
                }

                default:
                {
                    Debug.Log($"Unknown Packet : {type}");
                    break;
                }
            }
        }
        catch (IOException)
        {
            Debug.Log("Disconnected");
            break;
        }
        catch (SocketException)
        {
            Debug.Log("Socket Closed");
            break;
        }
        catch (Exception e)
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

        myNickname = id;

        LoginPacket packet = new();

        packet.type = PacketType.LOGIN;
        packet.id = id;
        packet.password = pw;

        SendPacket(packet);
    }
    
    static void ReadFull(NetworkStream stream, byte[] buffer, int size)
    {
        int offset = 0;

        while(offset < size)
        {
            int read =
                stream.Read(
                    buffer,
                    offset,
                    size - offset);

            if(read <= 0)
            {
                throw new Exception(
                    "Client Disconnected");
            }

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
    
    public void SendMove(float moveX, float moveY, float moveZ, float rotY, bool isMove)
    {
        MovePacket packet = new();

        packet.type = PacketType.MOVE;
        
        // Input
        packet.moveX = moveX;
        packet.moveY = moveY;
        packet.moveZ = moveZ;

        // Rotation
        packet.rotY = rotY;

        // Animation
        packet.isMove = isMove;

        string json =
            JsonConvert.SerializeObject(packet);
        
        Debug.Log($"[CLIENT SEND JSON] {json}");
        

        byte[] data =
            Encoding.UTF8.GetBytes(json);

        udp.Send(data, data.Length);
    }
    
    public void SendUdpConnect()
    {
        UdpConnectPacket packet = new();

        packet.type = PacketType.UDP_CONNECT;

        packet.nickname = myNickname;

        string json = JsonConvert.SerializeObject(packet);

        byte[] data = Encoding.UTF8.GetBytes(json);

        udp.Send(data, data.Length);

        Debug.Log($"UDP CONNECT SEND : {json}");
    }
    
    public bool IsConnected()
    {
        return socket != null &&
               socket.Connected &&
               stream != null;
    }
    
    void UdpReceiveLoop()
    {
        IPEndPoint remoteEP =
            new IPEndPoint(IPAddress.Any, 0);

        while(isRunning)
        {
            try
            {
                byte[] data =
                    udp.Receive(ref remoteEP);

                string json =
                    Encoding.UTF8.GetString(data);

                Debug.Log(
                    $"UDP RECV : {json}");

                JObject obj =
                    JObject.Parse(json);

                PacketType type =
                    (PacketType)obj["type"]
                        .Value<int>();

                switch(type)
                {
                    case PacketType.MOVE:
                    {
                        MoveBroadcastPacket packet = 
                            JsonConvert.DeserializeObject<MoveBroadcastPacket>(json);
                        
                        PlayerController player = playerManager.GetPlayer(packet.nickname);

                        if (player == null)
                            break;

                        /*// 오래된 패킷 무시
                        if(packet.tick <= player.lastReceivedTick)
                        {
                            break;
                        }*/

                        player.lastReceivedTick = packet.tick;
                        
                        UnityMainThreadDispatcher
                            .Instance
                            .Enqueue(() =>
                            {
                                playerManager.MovePlayer(
                                    packet.nickname,
                                    new Vector3(
                                        packet.position.X,
                                        packet.position.Y,
                                        packet.position.Z),
                                    packet.rotY,
                                    packet.isMove,
                                    packet.tick);
                            });

                        break;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}