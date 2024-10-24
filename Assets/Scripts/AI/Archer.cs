using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Enemy
{
    [Header("Archer")]
    [SerializeField] Transform launcher;
    [SerializeField] float launcherRotateSpeed = 180f;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject projectile;
    [SerializeField] float projectileSpeed = 5f;
    [SerializeField] float avoidDistance = 4f;
    [SerializeField] float followDistance = 6f;
    [SerializeField] bool strafe = false;

    private void Update()
    {
        base.MovementUpdate();
        if (targets.Count > 0)
        {
            if(selectedTarget == null)
                SelectTarget();
            
            RaycastHit2D targetHit = Physics2D.Linecast(firePoint.position, selectedTarget.position, collisionLayers);
            if(targetHit.transform == selectedTarget)
            {
                Vector2 targetDirection = selectedTarget.position - transform.position;
                launcher.rotation = Quaternion.RotateTowards(launcher.rotation, Quaternion.LookRotation(Vector3.forward, targetDirection), launcherRotateSpeed * Time.deltaTime);
                launcher.eulerAngles = new Vector3(0, launcher.eulerAngles.y, launcher.eulerAngles.z);

                if(attackTimer >= attackCooldown)
                {
                    attackTimer = 0;
                    Rigidbody2D projectileRB = Instantiate(projectile, firePoint.position, firePoint.rotation).GetComponent<Rigidbody2D>();
                    projectileRB.velocity = firePoint.up * projectileSpeed;
                    Physics2D.IgnoreCollision(_collider, projectileRB.GetComponent<Collider2D>());
                }

                float sqrDistance = targetDirection.sqrMagnitude;
                if (sqrDistance < avoidDistance * avoidDistance) 
                {
                    moveVelocity = -targetDirection;
                    moveVelocity.y = 0f;
                    moveVelocity = moveVelocity.normalized * runSpeed;
                }
                else if(sqrDistance > followDistance * followDistance)
                {
                    moveVelocity = targetDirection;
                    moveVelocity.y = 0f;
                    moveVelocity = moveVelocity.normalized * runSpeed;
                }
                else if(!strafe)
                {
                    moveVelocity = Vector2.zero;
                }

                if(CheckFall(moveVelocity).transform == null)
                {
                    moveVelocity = Vector2.zero;
                }

                Vector3 oldRight = transform.right;
                transform.right = new Vector2(targetDirection.x, 0).normalized;
                if(oldRight != transform.right)
                {
                    launcher.rotation *= Quaternion.Euler(0, 180, 0);
                }
            }
            else
            {
                moveVelocity = Vector2.zero;
            }
            ApplyVelocity(new Vector2(moveVelocity.x, rb.velocity.y));
            attackTimer += Time.deltaTime;
        }
    }
}
