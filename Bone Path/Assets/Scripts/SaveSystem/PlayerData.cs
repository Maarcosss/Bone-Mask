[System.Serializable]

/*Author: Marcos Isar
Date: 20 - Nov - 2025*/

public class PlayerData
{

    public int health;
    public float soul;
    public int coins;
    public float[] position = new float[3];

    //Copy player's stats and position into serializable data
    public PlayerData(Player player)
    {
        health = player.health;
        soul = player.soul;
        coins = player.coins;
        position[0] = player.transform.position.x;
        position[1] = player.transform.position.y;
        position[2] = player.transform.position.z;
    }

}
