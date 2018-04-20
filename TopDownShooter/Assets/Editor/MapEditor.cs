using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Mapgenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Mapgenerator map = target as Mapgenerator;
        map.GenerateMap();
    }

}
