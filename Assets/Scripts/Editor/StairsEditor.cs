using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Stairs))]
public class StairsEditor : Editor
{
    Stairs selectedStairs;

    [SerializeField] GameObject stepPrefab;
    [SerializeField] int numSteps = 5;
    [SerializeField] Vector2 stepScale = Vector2.one;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        stepPrefab = (GameObject)EditorGUILayout.ObjectField("Step Prefab", stepPrefab, typeof(GameObject), false);
        numSteps = EditorGUILayout.IntField("Num Steps", numSteps);
        stepScale = EditorGUILayout.Vector2Field("Step Scale", stepScale);

        if (selectedStairs == null)
        {
            selectedStairs = (Stairs)target;
        }

        if (GUILayout.Button("Generate Stairs"))
        {
            selectedStairs.GenerateStairs(stepPrefab, numSteps, stepScale);
            EditorUtility.SetDirty(selectedStairs.transform);
        }
    }
}
