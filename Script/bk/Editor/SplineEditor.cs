using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tr_CarAgent_spline))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Tr_CarAgent_spline agent = (Tr_CarAgent_spline)target;
        if (GUILayout.Button("SplineContainerから自動取得"))
        {
            var container = GameObject.Find("SplineContainer");
            if (container != null)
            {
                var children = new Transform[container.transform.childCount];
                for (int i = 0; i < children.Length; i++)
                    children[i] = container.transform.GetChild(i);

                agent.splinePoints = children;
                Debug.Log($"SplinePoints を {children.Length} 個自動設定しました");
            }
            else
            {
                Debug.LogWarning("SplineContainer が見つかりませんでした！");
            }
        }
    }
}
