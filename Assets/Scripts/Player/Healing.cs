using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Healing : MonoBehaviour
{
    public float unsavedHealing;

    public bool InHealerRange;

    [SerializeField] private HealthSystem target;

    Coroutine OverhealRoutine;

    [SerializeField] public float maxOverheal = 120f;

    [SerializeField] public float OverhealTime = 60f;

    void OnEnable(){
        Player.instance.input.Player.Interact.performed += Heal;
    }

    void OnDisable(){
        Player.instance.input.Player.Interact.performed -= Heal;
    }

    void Heal(InputAction.CallbackContext context){
        if(unsavedHealing > 0 && InHealerRange){
            if(target.health + unsavedHealing > target.originalMaxHealth){ 
                target.SetMaxHealth(maxOverheal, 0, true);
                if (OverhealRoutine != null) StopCoroutine(OverhealRoutine);
                OverhealRoutine = StartCoroutine(OverhealTimer(unsavedHealing+target.health));
            }
            target.AddHealth(unsavedHealing, 0, true);
            unsavedHealing = 0;
        }
    }
    
    IEnumerator OverhealTimer(float OverhealAmount){
        float timer = 0;
        OverhealAmount = Mathf.Min(OverhealAmount, maxOverheal);
        while(timer < OverhealTime){
            timer += Time.deltaTime;
            target.SetMaxHealth(Mathf.Lerp(OverhealAmount, target.originalMaxHealth, timer/OverhealTime), 0, true);
            yield return new WaitForEndOfFrame();
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Healer")) InHealerRange = true;
    }

    void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Healer")) InHealerRange = false;
    }
}