
using System.Numerics;

public class Player
{
    public int playerId;

    public string nickname;

    public Room room;
    
    public ClientSession session;

    // 실제 상태(State)
    /*
    public float x;
    public float y;
    public float z;
    */

    public Vector3 position;
    
    public float rotY;

    public bool isMove;

    public int hp = 100;
    
    // 입력(Input)
    public float moveX;
    public float moveZ;
}