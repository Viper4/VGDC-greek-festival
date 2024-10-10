using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private float NailCooldown = 0.3f;

    [SerializeField] Collider2D NailHitbox;

    [SerializeField] LayerMask HitLayers;
    Collider2D[]hits = new Collider2D[10];

    public void Attack(){
        ContactFilter2D Filter = new ContactFilter2D(){layerMask=HitLayers, useLayerMask=true};
        NailHitbox.OverlapCollider(Filter, hits);

        for(int i=0; i < NailHitbox.OverlapCollider(Filter, hits); i++){
            Debug.Log(hits[i].name);
        }
    }
}
