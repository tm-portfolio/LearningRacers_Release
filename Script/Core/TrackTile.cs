using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTile : MonoBehaviour
{
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 2,
            transform.position + Vector3.up * 2 + transform.forward * 3
        );

#if UNITY_EDITOR
    UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, gameObject.name);
#endif
    }

}
