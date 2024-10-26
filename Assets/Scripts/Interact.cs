using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
public class Interact : Trigger
{
    public UnityEvent OnInteract;
    void OnEnable()
    {
        Player.playerInput.Player.Interact.performed += DoInteract;
    }

    private void OnDisable()
    {
        Player.playerInput.Player.Interact.performed -= DoInteract;
    }

    void DoInteract(InputAction.CallbackContext context){
        if(collidersInTrigger > 0) OnInteract?.Invoke();
    }
}