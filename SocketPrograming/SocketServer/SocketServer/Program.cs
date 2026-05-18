using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

class Program
{
    static TcpListener server;
    public static UdpClient udpServer;
    
    // static List<TcpClient> clients = new List<TcpClient>();

    public static List<ClientSession> sessions = new();
    
    static void Main()
    {
        PacketHandler.Init();
        RoomManager.Init();
        ServerTick.Start();
        
        server = new TcpListener(IPAddress.Any, 5000);

        server.Start();
        
        Console.WriteLine("TCP Chat Server Start");
        
        udpServer = new UdpClient(6000);
        
        // UDP 수신 스레드
        Thread udpThread = new Thread(UdpReceiveLoop);

        udpThread.IsBackground = true;

        udpThread.Start();

        Console.WriteLine("UDP Server Start");
        
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            
            ClientSession session = new();
            
            session.client = client;
            session.stream = client.GetStream();
            
            SessionManager.Add(session);
            
            Thread thread = new Thread(HandleClient);

            thread.IsBackground = true;
            
            thread.Start(session);
        }
    }
    
    static void UdpReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while(true)
        {
            try
            {
                byte[] data = udpServer.Receive(ref remoteEP);

                string json = Encoding.UTF8.GetString(data);

                Console.WriteLine($"UDP RECV : {json}");

                PacketHandler.HandleUDP(remoteEP, json);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    

    /*static void HandleClient(object obj)
    {
        // TcpClient client = (TcpClient)obj;
        
        ClientSession session =
            (ClientSession)obj;

        NetworkStream stream =
            session.stream;

        // NetworkStream stream = client.GetStream();
        while (true)
        {
            try
            {
                byte[] lengthBuffer = new byte[4];

                ReadFull(stream, lengthBuffer, 4);
                
                int length = BitConverter.ToInt32(lengthBuffer, 0);
                
                if(length <= 0 || length > 1024 * 32)
                {
                    throw new Exception($"Invalid Packet Size : {length}");
                }

                byte[] dataBuffer = new byte[length];

                ReadFull(stream, dataBuffer, length);

                string json = Encoding.UTF8.GetString(dataBuffer);
                
                try
                {
                    PacketHandler.Handle(session, json);
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Packet Error : {e}");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);

                // 룸에서 제거
                if(session.player != null)
                {
                    if(session.player.room != null)
                    {
                        session.player.room.Leave(session.player);
                    }
                }

                // 세션 제거
                SessionManager.Remove(session);

                // 소켓 종료
                session.stream?.Close();
                session.client?.Close();

                Console.WriteLine("Client Disconnect");

                break;
            }
        }
    }*/
    
    static void HandleClient(object obj)
    {
        ClientSession session = (ClientSession)obj;

        NetworkStream stream = session.stream;

        byte[] lengthBuffer = new byte[4];

        while (true)
        {
            try
            {
                // 패킷 길이 수신
                ReadFull(stream, lengthBuffer, 4);

                int length = BitConverter.ToInt32(lengthBuffer, 0);

                // 비정상 패킷 차단
                if(length <= 0 || length > 1024 * 32)
                {
                    throw new Exception(
                        $"Invalid Packet Size : {length}");
                }

                // 패킷 데이터 수신
                byte[] dataBuffer =
                    new byte[length];

                ReadFull(stream, dataBuffer, length);

                // JSON 변환
                string json =
                    Encoding.UTF8.GetString(dataBuffer);

                Console.WriteLine($"TCP RECV : {json}");

                // 패킷 처리
                try
                {
                    PacketHandler.Handle(session, json);
                }
                catch(Exception e)
                {
                    Console.WriteLine(
                        $"Packet Handle Error : {e}");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Client Error : {e.Message}");

                DisconnectSession(session);

                break;
            }
        }
    }
    
    static void DisconnectSession(ClientSession session)
    {
        if(session == null)
            return;

        try
        {
            // 룸 제거
            if(session.player != null)
            {
                if(session.player.room != null)
                {
                    session.player.room.Leave(session.player);
                }
            }

            // 세션 제거
            SessionManager.Remove(session);

            // 소켓 종료
            session.stream?.Close();

            session.client?.Close();

            Console.WriteLine(
                $"Client Disconnect : " +
                $"{session.player?.nickname}");
        }
        catch(Exception e)
        {
            Console.WriteLine(
                $"Disconnect Error : {e}");
        }
    }
    
    
    // 로그인 검증
    static bool IsNicknameExist(string nickname)
    {
        foreach (var session in sessions)
        {
            if (session.player.nickname == nickname)
                return true;
        }

        return false;
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
    
}