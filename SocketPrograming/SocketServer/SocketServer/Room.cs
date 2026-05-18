public class Room
{
    public int roomId;

    List<Player> players = new();

    object locker = new();

    
    public List<Player> GetPlayers()
    {
        lock(locker)
        {
            return new List<Player>(players);
        }
    }
    
    public void Enter(Player player)
    {
        lock (locker)
        {
            players.Add(player);
        }

        player.room = this;

        BroadcastSystem(
            $"{player.nickname} joined");
    }

    public void Leave(Player player)
    {
        lock (locker)
        {
            players.Remove(player);
        }

        player.room = null;

        Console.WriteLine(
            $"{player.nickname} leave room {roomId}");

        BroadcastSystem(
            $"{player.nickname} left");
    }

    public void Broadcast(object packet)
    {
        List<Player> copied;

        lock (locker)
        {
            copied = new List<Player>(players);
        }

        foreach (var player in copied)
        {
            if(player == null) continue;

            if(player.session == null) continue;
            
            ServerSender.SendPacket(player.session, packet);
        }
    }

    void BroadcastSystem(string msg)
    {
        ChatPacket packet = new();

        packet.type = PacketType.SYSTEM;
        packet.nickname = "SYSTEM";
        packet.message = msg;

        Broadcast(packet);
    }
}