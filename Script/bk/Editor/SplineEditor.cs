using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tr_CarAgent_spline))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tr_CarAgent_spline agent = (Tr_CarAgent_spline)target;
        if (GUILayout.Button("SplineContainer���玩���擾"))
        {
            var container = GameObject.Find("SplineContainer");
            if (container != null)
            {
                var children = new Transform[container.transform.childCount];
                for (int i = 0; i < children.Length; i++)
                    children[i] = container.transform.GetChild(i);

                agent.splinePoints = children;
                Debug.Log($"SplinePoints �� {children.Length} �����ݒ肵�܂���");
            }
            else
            {
                Debug.LogWarning("SplineContainer ��������܂���ł����I");
            }
        }
    }
}
