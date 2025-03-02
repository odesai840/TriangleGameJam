using UnityEngine;
using UnityEngine.SceneManagement;

public class Monster : MonoBehaviour
{
    public float moveSpeed = .1f;       // Initial speed
    public float speedIncreaseRate = 0.01f; // How much speed increases per second
    public float rotationSpeed = 200f; // Rotation speed
    public string playerTag = "Player"; // Tag for identifying the player

    private Transform player;

    void Start()
    {
        // Load saved position & speed from GameSession
        transform.position = MonsterData.enemyPosition;
        transform.rotation = MonsterData.enemyRotation;
        moveSpeed = MonsterData.enemySpeed;

        // Find player in the scene
        GameObject playerObj = GameObject.FindWithTag(playerTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found! Make sure your player has the 'Player' tag.");
        }
    }

    void Update()
    {
        SaveEnemyState();

        if (player == null) return;

        // Rotate towards player
        Vector2 direction = player.position - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetAngle, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Move towards player
        transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);

        // Increase speed over time
        moveSpeed += speedIncreaseRate * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            //SaveEnemyState(); // Save position & speed before switching scenes
            SceneManager.LoadScene("Title");
        }
    }

    void SaveEnemyState()
    {
        // Store values in GameSession (which resets when the game ends)
        MonsterData.enemyPosition = transform.position;
        MonsterData.enemyRotation = transform.rotation;
        MonsterData.enemySpeed = moveSpeed;
    }
}
