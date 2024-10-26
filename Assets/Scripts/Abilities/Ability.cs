using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class Ability : MonoBehaviour
{
    public BaseMovement owner;
    public LayerMask hitLayers;
    public float damage = 1f;

    [HideInInspector] public float cooldown;
    float timer;

    public BaseMovement.KnockbackInfo knockback;

    public virtual void Start()
    {
        timer = cooldown;
    }

    public virtual void Update()
    {
        timer += Time.deltaTime;
    }

    public virtual void OnInput(InputAction.CallbackContext context)
    {
        if(timer >= cooldown)
        {
            timer = 0f;
            Use();
        }
    }

    public abstract void Use();
}
