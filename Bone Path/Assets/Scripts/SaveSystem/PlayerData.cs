
[System.Serializable]

public class PlayerData
{

    public int health;
    public float soul;
    public float[] position = new float[3];

    public PlayerData(Player player)
    {
        health = player.health;
        soul = player.soul;
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
    }

}
