[System.Serializable]
public class MovePacket : Packet
{
    public float x;
    public float y;
    public float z;

    public float rotY;

    public bool isMove;

    public float moveX;
    public float moveZ;

}