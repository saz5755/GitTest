using System.Numerics;
using System.Threading;

class ServerTick
{
    static Thread tickThread;

    public static void Start()
    {
        tickThread = new Thread(TickLoop);

        tickThread.IsBackground = true;

        tickThread.Start();
    }

    static void TickLoop()
    {
        while(true)
        {
            Tick();

            Thread.Sleep(50);
        }
    }

    static void Tick()
    {
        foreach(var session in SessionManager.GetSessions())
        {
            if(session.player == null) continue;
            if(session.player.room == null) continue;
            if(session.player.session == null) continue;

            UpdatePlayer(session.player);
        }
    }

    static void UpdatePlayer(Player player)
    {
        if(player.room == null)
            return;

        MoveBroadcastPacket packet = new();

        packet.type = PacketType.MOVE;

        packet.nickname = player.nickname;

        packet.x = player.position.X;
        packet.y = player.position.Y;
        packet.z = player.position.Z;

        packet.rotY = player.rotY;

        packet.isMove = player.isMove;

        foreach(var target in player.room.GetPlayers())
        {
            if(target == player)
                continue;

            if(target.session == null)
                continue;

            if(target.session.udpEndPoint == null)
                continue;

            UdpSender.Send(
                target.session.udpEndPoint,
                packet);
        }
    }
}