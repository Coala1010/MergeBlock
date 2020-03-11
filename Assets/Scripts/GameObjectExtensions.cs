using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class GameObjectExtensions
{
#if UNITY_EDITOR
    [MenuItem("AShim/SortChildrenByName")]
    public static void SortChildrenByName()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            List<Transform> children = new List<Transform>();
            for (int i = obj.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = obj.transform.GetChild(i);
                children.Add(child);
                child.parent = null;
            }
            children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
            foreach (Transform child in children)
            {
                child.parent = obj.transform;
            }
        }
    }
#endif
}