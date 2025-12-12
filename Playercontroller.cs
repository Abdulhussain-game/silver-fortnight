using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float laneDistance = 2.5f;            // distance between lanes
    public float forwardSpeed = 7f;              // forward movement speed
    public float laneChangeSpeed = 10f;          // horizontal smoothing
    public float jumpForce = 7f;
    public float gravity = 20f;
    public float slideDuration = 1.0f;

    CharacterController controller;
    int currentLane = 1; // 0 left, 1 center, 2 right
    Vector3 moveDirection = Vector3.zero;
    float verticalVelocity = 0f;
    bool isSliding = false;
    Vector3 originalCenter;
    float originalHeight;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalCenter = controller.center;
        originalHeight = controller.height;
    }

    void Update()
    {
        // always move forward in z
        Vector3 forwardMove = transform.forward * forwardSpeed;

        // input (keyboard) — also useful for testing in editor
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            MoveLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            MoveLane(1);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
            Jump();

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            StartCoroutine(Slide());

        // touch swipe for mobile (simple)
        HandleTouchInput();

        // compute target X based on lane
        float targetX = (currentLane - 1) * laneDistance;
        Vector3 targetPosition = new Vector3(targetX, 0, transform.position.z);

        // horizontal movement (smooth)
        Vector3 desired = new Vector3(targetX, transform.position.y, transform.position.z);
        Vector3 smoothed = Vector3.Lerp(transform.position, desired, Time.deltaTime * laneChangeSpeed);
        transform.position = new Vector3(smoothed.x, transform.position.y, smoothed.z);

        // vertical movement (gravity + jump)
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0) verticalVelocity = -1f; // small downward force to keep grounded
        }
        verticalVelocity -= gravity * Time.deltaTime;

        // apply movement
        Vector3 movement = forwardMove * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;

        controller.Move(movement);
    }

    void MoveLane(int delta)
    {
        currentLane = Mathf.Clamp(currentLane + delta, 0, 2);
    }

    void Jump()
    {
        if (controller.isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;
        }
    }

    IEnumerator Slide()
    {
        if (isSliding || !controller.isGrounded) yield break;
        isSliding = true;
        // lower character controller to simulate slide
        controller.height = originalHeight / 2f;
        controller.center = originalCenter - new Vector3(0, originalHeight / 4f, 0);
        // optionally play slide animation here
        yield return new WaitForSeconds(slideDuration);
        controller.height = originalHeight;
        controller.center = originalCenter;
        isSliding = false;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            // notify game manager
            GameManager.Instance.PlayerHit();
            // disable player movement to avoid multiple hits
            enabled = false;
        }
    }
}
