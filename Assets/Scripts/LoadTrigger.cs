using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadTrigger : Trigger
{
    [SerializeField] string sceneName;

    public override void TriggerEnter(Collider2D collider)
    {
        base.TriggerEnter(collider);
        SceneLoader.Instance.LoadScene(sceneName);
    }
}
