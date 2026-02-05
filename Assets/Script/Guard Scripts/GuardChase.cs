
using UnityEngine;
using System.Collections;

public class GuardChase : MonoBehaviour
{
    [SerializeField] float chaseSpeed = 10f;
    [SerializeField] float catchDistance = 2f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] Transform player = null;
    [SerializeField] bool isChase = false;

    private CharacterController controller;
    private GuardStationary guardStationary;
    private Vector3 velocity;
    private float gravity = -9.81f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        guardStationary = GetComponent<GuardStationary>();

        if (guardStationary == null)
        {
            Debug.LogError("GuardChase: GuardStationary component not found!", this);
        }

        if (player == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                player = found.transform;
            }
        }
    }

    private void Update()
    {
        if (controller.isGrounded)
        {
            velocity.y = -0.5f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);

        if (isChase == false)
            return;

        if (player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < catchDistance)
        {
            CatchPlayer();
            return;
        }

        MoveTowardPlayer();

    }

    public void StartChase(Transform player)
    {
        if (player != null)
        {
            this.player = player;
        }

        isChase = true;
    }

    public void StopChase()
    {
        isChase = false;
    }

    // Move toward player
    private void MoveTowardPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;

        // Move toward target
        controller.Move(direction * chaseSpeed * Time.deltaTime);

        // Smoothly rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    void CatchPlayer()
    {
        Debug.Log("Guard caught the player!");

        // Stop chasing immediately
        isChase = false;

        // Notify GuardStationary to sync state
        if (guardStationary != null)
        {
            guardStationary.StopChasing();
        }

        // Tell DeathManager that player died by guard catching
        // CheckpointManager will automatically restore guard position when loading checkpoint
        if (DeathManager.Instance != null)
        {
            DeathManager.Instance.OnDeath(DeathManager.DeathCause.GuardCaught);
        }
        else
        {
            Debug.LogError("Guard: DeathManager.Instance is null! Make sure DeathManager exists in scene.");
        }
    }
}


