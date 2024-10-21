using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityLock : MonoBehaviour
{
    [SerializeField] int ability;

    public void Unlock()
    {
        Player.instance.transform.Find("Abilities").GetChild(ability).gameObject.SetActive(true);
    }

    public void Lock()
    {
        Player.instance.transform.Find("Abilities").GetChild(ability).gameObject.SetActive(false);
    }
}
