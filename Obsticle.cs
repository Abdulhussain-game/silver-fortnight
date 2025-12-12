using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float moveSpeed = 7f;

    void Update()
    {
        // move obstacle backward (player moves forward)
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

        // destroy if far behind camera / player
        if (transform.position.z < -20f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // optional: if hit by player which may have trigger
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.PlayerHit();
        }
    }
}
