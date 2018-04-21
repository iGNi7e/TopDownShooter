using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Отрисовка в реальном времени метода GenerateMap()
[CustomEditor(typeof(Mapgenerator))]
public class MapEditor : Editor {

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        Mapgenerator map = target as Mapgenerator;
        if (DrawDefaultInspector())
        {
            map.GenerateMap();
        }

        if(GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }

}
