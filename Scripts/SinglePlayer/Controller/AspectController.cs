using System.Collections.Generic;
using UnityEngine;

public class AspectController : MonoBehaviour
{

    public Transform target; // Player cube
    public float distance = 1.74f; // Range with the player

    private List<Vector3> offsets;

    public Vector3[] Dirs { get; private set; }

    public Vector3 targetOffset;

    public Quaternion targetRotation;

    public int index = 0;

    private Vector3 currentVelocity = Vector3.zero;

    private void Start()
    {
        offsets = new List<Vector3>
        {
            new Vector3(-distance, 2.42f, -distance),
            new Vector3(distance, 2.42f, -distance),
            new Vector3(distance, 2.42f, distance),
            new Vector3(-distance, 2.42f, distance)
        };

        Dirs = new Vector3[4];

        Dirs[0] = Vector3.forward;
        Dirs[1] = Vector3.right;
        Dirs[2] = Vector3.back;
        Dirs[3] = Vector3.left;
    }
    public void PivotAspect ()
    {
        targetOffset = offsets[index];

        transform.position = Vector3.SmoothDamp(transform.position, targetOffset + target.position, ref currentVelocity, 0.25f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position),
            1 - Mathf.Exp(-10f * Time.deltaTime));
    }
    public void RightSwipe()
    {
        Vector3 temp = Dirs[3];
        for (int i = 2; i >= 0; i--)
            Dirs[i + 1] = Dirs[i];

        Dirs[0] = temp;
    }
    public void LeftSwipe ()
    {
        Vector3 temp = Dirs[0];
        for (int i = 0; i <= 2; i++)
            Dirs[i] = Dirs[i + 1];

        Dirs[3] = temp;
    }

}
