using UnityEngine;

public static class MonsterData
{
    public static void Reset()
    {
        enemyPosition = new Vector2(0, -10f); // Default start position
        enemyRotation = Quaternion.identity;
        enemySpeed = .1f; // Default speed
    }

    public static Vector2 enemyPosition = new Vector2(0, -10f); // Default start position
    public static Quaternion enemyRotation = Quaternion.identity;
    public static float enemySpeed = .1f; // Default speed
}
