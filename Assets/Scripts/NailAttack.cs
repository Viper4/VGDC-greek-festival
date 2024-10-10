using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
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

    [SerializeField] private Cooldown cooldown;

    [SerializeField] Collider2D NailHitbox;

    [SerializeField] LayerMask HitLayers;
    Collider2D[]hits = new Collider2D[10];

    [SerializeField] public float NailKnockbackDuration = .5f;

    private int HitsTally = 0;

    public Player targetScript;
    [SerializeField] public float NailKnockback = 1f;
    [SerializeField] public float NailDrag = 1f;
    Coroutine KnockbackRoutine;
    public void Attack(){
        
        if (cooldown.IsCoolingDown) return;

        ContactFilter2D Filter = new ContactFilter2D(){layerMask=HitLayers, useLayerMask=true};
        NailHitbox.OverlapCollider(Filter, hits);

        for(int i=0; i < NailHitbox.OverlapCollider(Filter, hits); i++){
            //Debug.Log(hits[i].name);
            HitsTally += 1;
        }

        if (HitsTally != 0){
            Vector2 direction = -(NailHitbox.transform.position - transform.position).normalized;
            direction.y = 0;
            if (KnockbackRoutine != null) StopCoroutine(KnockbackRoutine);
            KnockbackRoutine = StartCoroutine(targetScript.ApplyNailKnockback(direction * NailKnockback, NailKnockbackDuration, NailDrag));
        }
        
        cooldown.StartCooldown();
        HitsTally = 0;
    }
}
