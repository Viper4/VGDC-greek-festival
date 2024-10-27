using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class InteractionLock : MonoBehaviour
{
    int numInteracts = 0;
    [SerializeField] int interactsForUnlock = 1;
    [SerializeField] UnityEvent onUnlock;
    bool unlocked = false;

    public void AddInteract()
    {
        numInteracts++;
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
