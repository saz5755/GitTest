using System.Numerics;

public class MoveBroadcastPacket : Packet
{
    public int tick;
    
    public string nickname;
    
    public Vector3 position;

    public float rotY;
    
    public bool isMove;
    
    public float moveX;
    public float moveY;
    public float moveZ;
}