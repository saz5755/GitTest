using System.Net;
using Newtonsoft.Json;

class MoveHandler
{
    public static void Handle(ClientSession session, string json)
    {
        if (session.player == null)
            return;

        if (session.player.room == null)
            return;

        MovePacket packet =
            JsonConvert.DeserializeObject<MovePacket>(json);

        // 서버 상태 갱신
        session.player.position.X = packet.x;
        session.player.position.Y = packet.y;
        session.player.position.Z = packet.z;
        
        session.player.rotY = packet.rotY;
        
        session.player.isMove = packet.isMove;
        
        session.player.moveX = packet.moveX;
        session.player.moveZ = packet.moveZ;

        
        // Tick System 만들면 삭제 예정
        /*
        // 브로드캐스트 패킷 생성
        MoveBroadcastPacket sendPacket = new();

        sendPacket.type = PacketType.MOVE;

        sendPacket.nickname =
            session.player.nickname;

        sendPacket.x = packet.x;
        sendPacket.y = packet.y;
        sendPacket.z = packet.z;
        
        sendPacket.rotY = packet.rotY;
        
        sendPacket.isMove = packet.isMove;


        // Room 내부 전송
        session.player.room.Broadcast(sendPacket);*/
    }

    public static void HandleUDP(IPEndPoint remoteEP, string json)
    {
        ClientSession session =
            SessionManager.FindByUDP(remoteEP);

        if(session == null)
        {
            Console.WriteLine(
                $"UDP Session Not Found : {remoteEP}");

            return;
        }

        if(session.player == null)
            return;

        if(session.player.room == null)
            return;

        MovePacket packet =
            JsonConvert.DeserializeObject<MovePacket>(json);

        // 서버 상태 갱신
        session.player.position.X = packet.x;
        session.player.position.Y = packet.y;
        session.player.position.Z = packet.z;

        session.player.rotY = packet.rotY;

        session.player.isMove = packet.isMove;

        session.player.moveX = packet.moveX;
        session.player.moveZ = packet.moveZ;

        Console.WriteLine(
            $"UDP MOVE : {session.player.nickname}");
    }
}