using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class NailAttack : MonoBehaviour
{
    void OnEnable(){
        Player.playerInput.Player.Nail.performed += NailAttackScript;
    }

    void NailAttackScript(InputAction.CallbackContext context){
        Attack();
    }

    [SerializeField] private Transform attackTransform;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask attackableLayer;
    RaycastHit2D[] hits;
    [SerializeField] private float NailCooldown = 0.3f;

    public void Attack(){
        hits = Physics2D.CircleCastAll(attackTransform.position, attackRange, transform.right, 0f, attackableLayer);
    }
}
