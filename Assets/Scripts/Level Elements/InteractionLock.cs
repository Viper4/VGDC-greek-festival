using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class InteractionLock : MonoBehaviour
{
    private int numInteracts = 0;
    [SerializeField] private int interactsForUnlock = 1;
    [SerializeField] private UnityEvent onUnlock;
    private bool unlocked = false;

    public void AddInteract(int amount)
    {
        numInteracts += amount;
        if(numInteracts >= interactsForUnlock)
        {
            Unlock();
        }
    }

    public void Unlock()
    {
        if (!unlocked)
        {
            onUnlock?.Invoke();
            unlocked = true;
        }
    }
}
