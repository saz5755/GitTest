using System.Net.Sockets;

public class ClientSession
{
    public TcpClient client;

    public NetworkStream stream;

    public string nickname;
}