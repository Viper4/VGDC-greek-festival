using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class SaveableEntity : MonoBehaviour
{
    [SerializeField] string uniqueId = "";
    static Dictionary<string, SaveableEntity> globalLookup = new Dictionary<string, SaveableEntity>();

    public string UniqueId => uniqueId;

    // Used to capture state of the gameobject on which the saveableEntity is attached
    public object CaptureState()
    {
        Dictionary<string, object> state = new Dictionary<string, object>();
        foreach (ISaveable saveable in GetComponents<ISaveable>())
        {
            state[saveable.GetType().ToString()] = saveable.CaptureState();
        }
        return state;
    }

    // Used to restore state of the gameobject on which the saveableEntity is attached
    public void RestoreState(object state)
    {
        Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
        foreach (ISaveable saveable in GetComponents<ISaveable>())
        {
            string id = saveable.GetType().ToString();

            if (stateDict.ContainsKey(id))
                saveable.RestoreState(stateDict[id]);
        }
    }

    // To delete this saveable entity from the game
    public void Delete()
    {
        foreach (ISaveable saveable in GetComponents<ISaveable>())
        {
            saveable.Delete();
        }
    }

#if UNITY_EDITOR
    // Update method used for generating UUID of the SavableEntity
    private void Update()
    {
        // don't execute in playmode
        if (Application.IsPlaying(gameObject)) return;

        // don't generate Id for prefabs (prefab scene will have path as null)
        if (String.IsNullOrEmpty(gameObject.scene.path)) return;

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty property = serializedObject.FindProperty("uniqueId");

        if (String.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
        {
            property.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }

        globalLookup[property.stringValue] = this;
    }
#endif

    private bool IsUnique(string candidate)
    {
        if (!globalLookup.ContainsKey(candidate)) return true;

        if (globalLookup[candidate] == this) return true;

        // Handle scene unloading cases
        if (globalLookup[candidate] == null)
        {
            globalLookup.Remove(candidate);
            return true;
        }

        // Handle edge cases like designer manually changing the UUID
        if (globalLookup[candidate].UniqueId != candidate)
        {
            globalLookup.Remove(candidate);
            return true;
        }

        return false;
    }
}