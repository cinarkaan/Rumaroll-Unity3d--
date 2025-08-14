using UnityEngine;

public class CubeSimulator
{
    // The surface order of cube at the start of game
    // [0]=Bottom, [1]=Top, [2]=Front, [3]=Back, [4]=Left, [5]=Right.
    public int[] faceIndices = new int[] { 0, 1, 2, 3, 4, 5 };

    // It make to rolling simulatiıon that is indicated way and updated surface order of cube.
    public void Roll(Vector3 dir)
    {
        int[] newOrder = new int[6];
        // Instance mapping: (This mapping can be implemented by physical rotation of your cube.)
        if (dir == Vector3.forward) // Moving to +z direction
        {
            newOrder[0] = faceIndices[2];  
            newOrder[1] = faceIndices[3];  
            newOrder[2] = faceIndices[1];  
            newOrder[3] = faceIndices[0];  
            newOrder[4] = faceIndices[4];  
            newOrder[5] = faceIndices[5];  
        }
        else if (dir == Vector3.back) // Moving to -z direction
        {
            newOrder[0] = faceIndices[3]; 
            newOrder[1] = faceIndices[2]; 
            newOrder[2] = faceIndices[0]; 
            newOrder[3] = faceIndices[1]; 
            newOrder[4] = faceIndices[4];
            newOrder[5] = faceIndices[5];
        }
        else if (dir == Vector3.left) // Moving to -x direction
        {
            newOrder[0] = faceIndices[4];
            newOrder[1] = faceIndices[5];
            newOrder[2] = faceIndices[2];
            newOrder[3] = faceIndices[3];
            newOrder[4] = faceIndices[1];
            newOrder[5] = faceIndices[0];
        }
        else if (dir == Vector3.right) // Moving to +x direction
        {
            newOrder[0] = faceIndices[5];
            newOrder[1] = faceIndices[4];
            newOrder[2] = faceIndices[2];
            newOrder[3] = faceIndices[3];
            newOrder[4] = faceIndices[0];
            newOrder[5] = faceIndices[1];
        }
        faceIndices = newOrder;
    }

    public void Reset ()
    {
        faceIndices = new int[] { 0, 1, 2, 3, 4, 5};
    }

}
