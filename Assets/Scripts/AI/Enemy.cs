using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Enemy : BaseMovement, ISaveable
{
    enum TargetMode
    {
        Closest,
        Farthest,
        HighestHealth,
        LowestHealth,
        Random,
    }
    [Header("Enemy")]
    [SerializeField] TargetMode targetMode = TargetMode.Closest;

    public float damage = 1f;
    public float attackCooldown = 1f;
    [HideInInspector] public float attackTimer = 0f;
    [HideInInspector] public List<Transform> targets = new List<Transform>();
    [HideInInspector] public Transform selectedTarget = null;

    private void FixedUpdate()
    {
        Vector2 newVelocity = Vector2.zero;
        if(moveVelocity.x != 0)
        {
            newVelocity.x = moveVelocity.x;
        }
        if(knockbackVelocity.y == 0)
        {
            newVelocity.y = rb.velocity.y;
        }
        newVelocity += knockbackVelocity;
        ApplyVelocity(newVelocity);
    }

    private void Update()
    {
        MovementUpdate();
        if (targets.Count > 0)
        {
            if(selectedTarget == null)
                SelectTarget();

            RaycastHit2D targetHit = Physics2D.Linecast(transform.position, selectedTarget.position, collisionLayers);
            if(targetHit.transform == selectedTarget)
            {
                moveVelocity = selectedTarget.position - transform.position;
                moveVelocity.y = 0f;
                moveVelocity = moveVelocity.normalized * runSpeed;
            }
            else
            {
                moveVelocity = Vector2.zero;
            }
        }
    }

    public void SelectTarget()
    {
        selectedTarget = targets[0];
        switch (targetMode)
        {
            case TargetMode.Closest:
                for (int i = 1; i < targets.Count; i++)
                {
                    if ((targets[i].position - transform.position).sqrMagnitude < (selectedTarget.position - transform.position).sqrMagnitude)
                    {
                        selectedTarget = targets[i];
                    }
                }
                break;
            case TargetMode.Farthest:
                for (int i = 1; i < targets.Count; i++)
                {
                    if ((targets[i].position - transform.position).sqrMagnitude > (selectedTarget.position - transform.position).sqrMagnitude)
                    {
                        selectedTarget = targets[i];
                    }
                }
                break;
            case TargetMode.HighestHealth:
                HealthSystem selectedHealthSystem = targets[0].GetComponent<HealthSystem>();
                for (int i = 1; i < targets.Count; i++)
                {
                    HealthSystem currentHealthSystem = targets[i].GetComponent<HealthSystem>();
                    if (currentHealthSystem.health > selectedHealthSystem.health)
                    {
                        selectedTarget = targets[i];
                        selectedHealthSystem = currentHealthSystem;
                    }
                }
                break;
            case TargetMode.LowestHealth:
                selectedHealthSystem = targets[0].GetComponent<HealthSystem>();
                for (int i = 1; i < targets.Count; i++)
                {
                    HealthSystem currentHealthSystem = targets[i].GetComponent<HealthSystem>();
                    if (currentHealthSystem.health > selectedHealthSystem.health)
                    {
                        selectedTarget = targets[i];
                        selectedHealthSystem = currentHealthSystem;
                    }
                }
                break;
            case TargetMode.Random:
                selectedTarget = targets[Random.Range(0, targets.Count)];
                break;
        }
    }

    public void OnTargetTriggerEnter(Collider2D targetCollider)
    {
        if(!targets.Contains(targetCollider.transform))
            targets.Add(targetCollider.transform);
    }

    public void OnTargetTriggerExit(Collider2D targetCollider)
    {
        targets.Remove(targetCollider.transform);
    }

    public override object CaptureState()
    {
        float[] position = { transform.position.x, transform.position.y, transform.position.z };
        float[] eulerAngles = { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z };
        float[] velocity = { rb.velocity.x, rb.velocity.y };
        float health = GetComponent<HealthSystem>().health;
        return new object[] { position, eulerAngles, velocity, health };
    }

    public override void RestoreState(object state)
    {
        object[] data = (object[])state;
        float[] position = (float[])data[0];
        float[] eulerAngles = (float[])data[1];
        float[] velocity = (float[])data[2];
        HealthSystem healthSystem = GetComponent<HealthSystem>();
        healthSystem.health = (float)data[3];
        healthSystem.HealthUpdate();
        transform.position = new Vector3(position[0], position[1], position[2]);
        transform.eulerAngles = new Vector3(eulerAngles[0], eulerAngles[1], eulerAngles[2]);
        rb.velocity = new Vector2(velocity[0], velocity[1]);
    }
}
