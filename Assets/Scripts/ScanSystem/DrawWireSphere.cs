using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawWireSphere : MonoBehaviour
{
    public float radius;
    [SerializeField] Color gizmoColor = Color.red;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
