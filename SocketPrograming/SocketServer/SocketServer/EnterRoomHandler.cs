using Newtonsoft.Json;

class EnterRoomHandler
{
    public static void Handle(ClientSession session, string json)
    {
        if (session.isLogin == false)
            return;

        EnterRoomPacket packet =
            JsonConvert.DeserializeObject<EnterRoomPacket>(json);

        Room room =
            RoomManager.GetRoom(packet.roomId);

        if (room == null)
        {
            Console.WriteLine($"Room Not Found : {packet.roomId}");
            return;
        }

        // 기존 방 나가기
        if (session.player.room != null)
        {
            session.player.room.Leave(session.player);
        }

        // 새 방 입장
        room.Enter(session.player);
        
        // 1. 기존 플레이어 목록 전송
        foreach(var player in room.GetPlayers())
        {
            SpawnPacket spawn = new();

            spawn.type = PacketType.SPAWN;

            spawn.nickname = player.nickname;

            spawn.x = player.position.X;
            spawn.y = player.position.Y;
            spawn.z = player.position.Z;

            ServerSender.SendPacket(session, spawn);
        }
        
        Console.WriteLine(
            $"{session.player.nickname} enter room {packet.roomId}");

        // 2. 새 플레이어를 전체에게 브로드캐스트
        SpawnPacket mySpawn = new();

        mySpawn.type = PacketType.SPAWN;

        mySpawn.nickname =
            session.player.nickname;

        mySpawn.x = 0;
        mySpawn.y = 0;
        mySpawn.z = 0;

        room.Broadcast(mySpawn);
    }
}