using System.Collections.Generic;
using UnityEngine;

//static class containing information necessary for world generation. acts as data holder that transmise world information from UI to in-game scene
public static class DataHolder
{
    public static readonly Vector3 topVoxel = new Vector3(0f, 1f, 0f);
    public static readonly Vector3 bottomVoxel = new Vector3(0f, -1f, 0f);
    public static readonly Vector3 frontVoxel = new Vector3(0f, 0f, 1f);
    public static readonly Vector3 backVoxel = new Vector3(0f, 0f, -1f);
    public static readonly Vector3 rightVoxel = new Vector3(1f, 0f, 0f);
    public static readonly Vector3 leftVoxel = new Vector3(-1f, 0f, 0f);
    public static byte[,,] worldBluePrint;
    public static byte[,,] mapBluePrint;
    public static readonly int texturePackWidthInBlock = 5;
    public static readonly int chunkSideInVoxel = 16;
    public static int worldLengthInVoxel;
    public static int worldHeightInVoxel;
    public static int worldWidthInVoxel;
    public static int worldLengthInChunk;
    public static int worldWidthInChunk; 
    public static int chunkHeightInVoxel;
    public static Vector3 globalSpawnPos;
    public static int spawnPosLayerIndex;
    public static List<Vector3> posObj;
    public static List<Vector3Int> simplifiedPosObj;

    //before the transition from UI to in-game scene, the world information received from the UI will be stored in this static class
    //this implies that the game scene relies heavily from this class 
    public static void EvaluateWorldInformation(byte[,,] volumeMap, byte[,,] simplifiedMap, Vector3 globalSpawnPos, int spawnPosLayerIndex, List<Vector3> posObj, List<Vector3Int> simplifiedPosObj){
        worldBluePrint = volumeMap;
        worldLengthInVoxel = volumeMap.GetLength(0);
        worldHeightInVoxel = volumeMap.GetLength(1);
        worldWidthInVoxel = volumeMap.GetLength(2);
        worldLengthInChunk = worldLengthInVoxel / chunkSideInVoxel;
        worldWidthInChunk = worldWidthInVoxel / chunkSideInVoxel; 
        chunkHeightInVoxel = worldHeightInVoxel;

        mapBluePrint = simplifiedMap;

        DataHolder.globalSpawnPos = globalSpawnPos;

        DataHolder.spawnPosLayerIndex = spawnPosLayerIndex;

        DataHolder.posObj = posObj;

        DataHolder.simplifiedPosObj = simplifiedPosObj;

    }
}
