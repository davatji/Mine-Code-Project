using UnityEngine;


//voxel data script that arrange the vertices, triangle, and chunk generation-related information in proper data structure to provide ease in creating the chunk generation logic
public class VoxelDataScript
{
    public const int numberOfVertices = 6;
    public const int numberOfFaces = 6;
    public static readonly Vector3[] vertexIndices = new Vector3[8] {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f), // these array elements define the relative position of each vertex in a block / voxel
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };
    public static readonly int[,] voxelTriangles = new int[numberOfFaces, 4] {
        {0, 3, 1, 2}, //back
        {5, 6, 4, 7}, //front
        {3, 7, 2, 6}, //top 
        {1, 5, 0, 4}, //bottom
        {4, 7, 0, 3}, //left
        {1, 2, 5, 6} //right //this array element group indexes that defines a face
    };
    public static readonly Vector3[] faceCheck = new Vector3[numberOfFaces] {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };
    public static readonly Vector2[] uvsInformation = new Vector2[4] {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };
}
