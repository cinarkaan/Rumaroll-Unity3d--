using System.Collections.Generic;
using UnityEngine;

public class AspectController : MonoBehaviour
{

    public Transform target; // Player cube
    public float distance = 1.74f; // Range with the player


    private List<Vector3> offsets;

    public Vector3[] dirs { get; private set; }

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

        dirs = new Vector3[4];

        dirs[0] = Vector3.forward;
        dirs[1] = Vector3.right;
        dirs[2] = Vector3.back;
        dirs[3] = Vector3.left;
    }
    public void pivotAspect ()
    {
        targetOffset = offsets[index];

        transform.position = Vector3.SmoothDamp(transform.position, targetOffset + target.position, ref currentVelocity, 0.25f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.position - transform.position),
            1 - Mathf.Exp(-10f * Time.deltaTime));
    }
    public void rightSwipe()
    {
        Vector3 temp = dirs[3];
        for (int i = 2; i >= 0; i--)
            dirs[i + 1] = dirs[i];

        dirs[0] = temp;
    }
    public void leftSwipe ()
    {
        Vector3 temp = dirs[0];
        for (int i = 0; i <= 2; i++)
            dirs[i] = dirs[i + 1];

        dirs[3] = temp;
    }

}
