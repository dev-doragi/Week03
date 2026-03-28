using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleDrawer : MonoBehaviour
{
    public float radius = 1f;
    public int segments = 64;

    void Start()
    {
        LineRenderer lr = GetComponent<LineRenderer>();
        lr.positionCount = segments;
        lr.loop = true;
        for (int i = 0; i < segments; i++)
        {
            float a = 2 * Mathf.PI * i / segments;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0));
        }
    }
}