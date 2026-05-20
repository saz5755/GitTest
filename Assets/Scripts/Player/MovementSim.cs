using System.Numerics;

static class MovementSim
{
    public static Vector3 Simulate(
        Vector3 pos,
        Vector3 dir,
        float speed,
        float tickDelta)
    {
        if (dir.Length() > 1f) 
            dir = Vector3.Normalize(dir);


        return pos + dir * speed * tickDelta;
    }
}