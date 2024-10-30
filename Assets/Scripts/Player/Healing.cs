using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Healing : MonoBehaviour
{
    public float unsavedHealing;
    private bool inHealerRange;
    [SerializeField] private HealthSystem target;
    private Coroutine overhealRoutine;

    [SerializeField] private float maxOverheal = 120f;

    [SerializeField] private float overhealTime = 60f;

    void OnEnable(){
        Player.instance.input.Player.Interact.performed += Heal;
    }

    void OnDisable(){
        Player.instance.input.Player.Interact.performed -= Heal;
    }

    void Heal(InputAction.CallbackContext context){
        if(unsavedHealing > 0 && inHealerRange)
        {
            if(target.health + unsavedHealing > target.originalMaxHealth)
            { 
                target.SetMaxHealth(maxOverheal, 0, true);
                if (overhealRoutine != null) 
                    StopCoroutine(overhealRoutine);
                overhealRoutine = StartCoroutine(OverhealTimer(unsavedHealing+target.health));
            }
            target.AddHealth(unsavedHealing, 0, true);
            unsavedHealing = 0;
        }
    }
    
    IEnumerator OverhealTimer(float OverhealAmount){
        float timer = 0;
        OverhealAmount = Mathf.Min(OverhealAmount, maxOverheal);
        while(timer < overhealTime)
        {
            timer += Time.deltaTime;
            target.SetMaxHealth(Mathf.Lerp(OverhealAmount, target.originalMaxHealth, timer / overhealTime), 0, true);
            yield return new WaitForEndOfFrame();
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Healer"))
            inHealerRange = true;
    }

    void OnTriggerExit2D(Collider2D other){
        if (other.CompareTag("Healer"))
            inHealerRange = false;
    }
}