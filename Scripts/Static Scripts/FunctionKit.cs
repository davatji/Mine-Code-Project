using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


//shared general purpose class
public static class FunctionKit{

    //reading csv files containing bytes in string and converting it to 1 dimensional array 
    public static byte[] ParseFileToFlatArray(string path){

        string content = File.ReadAllText(path);
        string[] bytesInString = content.Split(",");
        byte[] bytes = new byte[bytesInString.Length];
        for (int i = 0; i < bytesInString.Length; i++){
            bytes[i] = byte.Parse(bytesInString[i]);
        }
        return bytes;
        
    }

    //converting 1D array to 3D array based on dimensions given
    public static byte[,,] FlatArrayTo3DArray(byte[] flatArray, int xDimension, int yDimension, int zDimension){
        if (xDimension * yDimension * zDimension != flatArray.Length){
            Debug.Log("Invalid flat array to convert");
            return null;
        }

        int x = 0;
        int y = 0;
        int z = 0;
        
        byte[,,] volumeGrid = new byte[xDimension, yDimension, zDimension];  
        for (int i = 0; i < flatArray.Length; i++){
            if (z == zDimension){
                z = 0;
                y++;
            }
            if (y == yDimension){
                y = 0;
                x++;
            }
            volumeGrid[x, y, z] = flatArray[i];
            z++;
        }
        return volumeGrid;
    }

    //converting 3D array into 1D array
    public static byte[] Flatten3DArray(byte[,,] volumeArray){
        int flattenedArrayLength = volumeArray.GetLength(0) * volumeArray.GetLength(1) * volumeArray.GetLength(2);
        byte[] flattenedArray = new byte[flattenedArrayLength];

        int index = 0;
        for (int x = 0; x < volumeArray.GetLength(0); x++){
            for (int y = 0; y < volumeArray.GetLength(1); y++){
                for (int z = 0; z < volumeArray.GetLength(2); z++){
                    flattenedArray[index] = volumeArray[x, y, z];
                    index++;
                }
            }
        }
        return flattenedArray;
    }

    //(used so that the original default array (which consists of only byte 1) won't be changed due to unintended reference issue)
    public static byte[,] Copy2DByteArray(byte[,] originalArray){
        int iDimension = originalArray.GetLength(0);
        int jDimension = originalArray.GetLength(1); 
        byte[,] copiedArray = new byte[iDimension, jDimension];
        Array.Copy(originalArray, copiedArray, iDimension * jDimension);
        return copiedArray; 
    } 

    //multiply the array by "scalar" on x, y, or z dimension scale
    public static byte[,,] Enlargen3DByteArray(byte[,,] originalArray, int xScale, int yScale, int zScale){
        int oX = originalArray.GetLength(0);
        int oY = originalArray.GetLength(1);
        int oZ = originalArray.GetLength(2);

        int eX = oX * xScale;
        int eY = oY * yScale;
        int eZ = oZ * zScale;

        byte[,,] enlargenedArray = new byte[eX, eY, eZ];
        for (int x = 0; x < eX; x++){
            for (int y = 0; y < eY; y++){
                for (int z = 0; z < eZ; z++){
                    enlargenedArray[x, y, z] = originalArray[x / xScale, y / yScale, z / zScale];
                }
            }
        }
        return enlargenedArray;
    }

    //simplify the elements of the array by changing all the bytes into default byte, except for certain bytes
    public static byte[,,] Simplify3DByteArray(byte[,,] originalArray, byte[] byteExceptions, byte defaultByte){

        int iDimension = originalArray.GetLength(0);
        int jDimension = originalArray.GetLength(1);
        int kDimension = originalArray.GetLength(2);

        //creating a new array instead so that the function doesn't modify the original array
        byte[,,] newArray = new byte[iDimension, jDimension, kDimension];

        for (int i = 0; i < iDimension; i++){
            for (int j = 0; j < jDimension; j++){
                for (int k = 0; k < kDimension; k++){

                    byte byteToCheck = originalArray[i, j, k];
                    if (byteExceptions.Contains(byteToCheck)){
                        newArray[i, j, k] = byteToCheck;
                    }
                    else{
                        newArray[i, j, k] = defaultByte;
                    }
                }
            }
        }
        return newArray;
    }

    //monolithic function that process the 3D array whose product are information necessary for world generation. passed into static class DataHolder
    //created as monolithic to optimize the performance 

    //this function process 3D map to multiply it by scale and create natural looking tunnel curve using perlin noise, randomness, and mathematical calculation around the air block
    public static Tuple<byte[,,], Vector3, List<Vector3>, List<Vector3Int>> ProcessMap(byte[,,] originalMap, int xScale, int yScale, int zScale){
        int xLength = originalMap.GetLength(0);
        int yLength = originalMap.GetLength(1);
        int zLength = originalMap.GetLength(2);

        int eX = xLength * xScale;
        int eY = yLength * yScale;
        int eZ = zLength * zScale;

        int radiusX = (xScale - 1) / 2;
        int radiusY = yScale - 1;
        int radiusZ = (zScale - 1) / 2;

        //try to create simple pseudo random algorithm to vary the seed, so that the pseudo random instance for each world generation is different,
        //creating "unique" randomness for each world
        int seed = eX * eY * eZ / 3; 
        System.Random random = new System.Random(Seed: seed);

        byte[,,] curvedMap = new byte[eX, eY, eZ];
        List<Vector3Int> airBlockPos = new List<Vector3Int>();

        Vector3 spawnGlobalPos = new Vector3(0, 0, 0);
        bool searchSpawnPos = true;

        List<Vector3> locatePosMission = new List<Vector3>();

        //simplified locate position mission is used to mark the in-game map about the location
        List<Vector3Int> locatePosMissionSimplified = new List<Vector3Int>();

        for (int x = 0; x < xLength; x++){
            for (int y = 0; y < yLength; y++){
                for (int z = 0; z < zLength; z++){
                    
                    byte byteSample = originalMap[x, y, z];

                    float perlinThresHold = 0;
                    float randomThresHold = 0;

                    /*try to search for the spawn block or nearest 3D air block to determine player's spawn position. if the spawn location is found through spawn byte (30),
                    stop finding the spawn position through air block. however, if the spawn pos is found through air block, still continuing finding the spawn pos
                    through spawn byte*/
                    if (byteSample == 30){
                        //changing the spawn block to air block since the player must spawn in air block
                        byteSample = 0;
                        originalMap[x, y, z] = 0;
                        Vector3 clusterCenterPos = new Vector3(x * xScale + xScale / 2, y * yScale, z * zScale + zScale / 2);
                        spawnGlobalPos = clusterCenterPos;
                        searchSpawnPos = false;
                    }

                    /*the algorithm of spawn location finding differs according to y scale. if y scale is bigger than one, the block candidate must
                    be an air block. if y scale is exactly one, the block candidate must be an air block AND the block above it as well, making sure
                    that the player won't suffocate in the walls*/
                    if (searchSpawnPos && yScale > 1 && byteSample == 0){
                        Vector3 clusterCenterPos = new Vector3(x * xScale + xScale / 2, y * yScale, z * zScale + zScale / 2);
                        spawnGlobalPos = clusterCenterPos;
                        searchSpawnPos = false;
                    }
                    else if (searchSpawnPos && yScale == 1 && byteSample == 0 && originalMap[x, y + 1, z] == 0){
                        Vector3 clusterCenterPos = new Vector3(x * xScale + xScale / 2 , y * yScale, z * zScale + zScale / 2);
                        spawnGlobalPos = clusterCenterPos;
                        searchSpawnPos = false;
                    }

                    //for every byte 31 (byte representation of locate position mission), get its position and gather it in a list for task manager class
                    if (byteSample == 31){
                        byteSample = 0;
                        originalMap[x, y, z] = 0;
                        
                        locatePosMission.Add(new Vector3(x * xScale + xScale / 2, y * yScale + yScale / 2, z * zScale + zScale / 2));
                        locatePosMissionSimplified.Add(new Vector3Int(x, y, z));
                    }

                    //the threshold, which is correlated to block concentration within a cluster, differs according to type of the block.
                    //eg. diamond threshold is higher than sand, indicating that the concentration of diamond in a cluster is lower than sand 
                    if (byteSample <= 11 && byteSample >= 2){
                        perlinThresHold = 0.5f;
                        randomThresHold = 0.5f;
                    }
                    else if (byteSample >= 16 && byteSample <= 22 || byteSample == 25 || byteSample == 29){
                        perlinThresHold = 0.6f;
                        randomThresHold = 0.7f;
                    }
                    else if (byteSample != 0 && byteSample != 1){
                        perlinThresHold = 0.7f;
                        randomThresHold = 0.9f;
                    }
            
                    //carving tunnel algorithm

                    /*try to find the air block (byte 0) that at least are n blocks apart from the surrounding nearest solid block and iterate through that block surrounding.
                    surrounding block in certain radius will be converted to air, creating cylinder-shaped tunnel. for the rest of the byte beside stone (byte 1), 
                    apply the perlin noise and random to determine whether to fill the position with stone byte or the desired block*/
                    for (int dx = x * xScale; dx < (x + 1) * xScale; dx++){
                        for (int dy = y * yScale; dy < (y + 1) * yScale; dy++){
                            for (int dz = z * zScale; dz < (z + 1) * zScale; dz++){
                            
                                if (byteSample == 0){
                                    try
                                    {
                                        if (originalMap[(dx + radiusX) / xScale, y, z] == 0 
                                        && originalMap[(dx - radiusX) / xScale, y, z] == 0
                                        && originalMap[x, (dy + radiusY) / yScale, z] == 0
                                        && originalMap[x, y, (dz + radiusZ) / zScale] == 0 
                                        && originalMap[x, y, (dz - radiusZ) / zScale] == 0)
                                        {
                                            if (dx - radiusX < 0 || dz - radiusZ < 0){
                                                curvedMap[dx, dy, dz] = 1;
                                            }
                                            else{
                                                airBlockPos.Add(new Vector3Int(dx, dy, dz));
                                                curvedMap[dx, dy, dz] = byteSample;
                                            }
                                        }
                                        else{
                                            curvedMap[dx, dy, dz] = 1; //default block ID
                                        }
                                    }
                                    catch (IndexOutOfRangeException){
                                        curvedMap[dx, dy, dz] = 1;
                                    }
                                }
                                else if (byteSample == 1){
                                    curvedMap[dx, dy, dz] = byteSample;
                                }
                                else{
                                    if (NoiseScript.GetPerlinNoise3D(new Vector3(dx, dy, dz), scale: 5, offset: 0, perlinThresHold, eX / DataHolder.chunkSideInVoxel, eZ / DataHolder.chunkSideInVoxel)){
                                        curvedMap[dx, dy, dz] = byteSample;
                                    }
                                    else if ((float)random.Next(0, 100) / 100  >= randomThresHold){
                                        curvedMap[dx, dy, dz] = byteSample;
                                    }
                                    else{
                                        curvedMap[dx, dy, dz] = 1;
                                    }
                                }
                            }   
                        }
                    }
                }
            }
        }
        foreach(Vector3Int pos in airBlockPos){
            int radiusThresHold = 4;
            for (int rX = -radiusX; rX < radiusX + 1; rX++){
                for (int rY = 0; rY < radiusY + 1; rY++){
                    for (int rZ = -radiusZ; rZ < radiusZ + 1; rZ++){
                        if (curvedMap[pos.x + rX, pos.y + rY, pos.z + rZ] != 0 && Math.Sqrt(Math.Pow(rX, 2) + Math.Pow(rY - 3, 2) + Math.Pow(rZ, 2)) <= radiusThresHold){
                            curvedMap[pos.x + rX, pos.y + rY, pos.z + rZ] = 0;
                        }
                    }
                }
            }
        }

        //if searchSpawnPos is still true, implying that the spawn position hasn't been found, set the spawn position on the top of the world on (x, z): (0.5, 0.5)
        if (searchSpawnPos){
            spawnGlobalPos = new Vector3(0.5f, eY + 1, 0.5f);
        }
        return new Tuple<byte[,,], Vector3, List<Vector3>, List<Vector3Int>>(curvedMap, spawnGlobalPos, locatePosMission, locatePosMissionSimplified); 
    }

    //extending array on certain dimension with certain block ID. purpose: to fill the top of the world with air block
    public static byte[,,] Extend3DByteArray(byte[,,] originalArray, byte blockID, Vector3Int extension){
        int xExtend = extension.x;
        int yExtend = extension.y;
        int zExtend = extension.z;
        
        int xLength = originalArray.GetLength(0);
        int yLength = originalArray.GetLength(1);
        int zLength = originalArray.GetLength(2);

        byte[,,] extendedArray = new byte[xLength + xExtend, yLength + yExtend, zLength + zExtend];
        for (int x = 0; x < xLength + xExtend; x++){
            for (int y = 0; y < yLength + yExtend; y++){
                for (int z = 0; z < zLength + zExtend; z++){
                    if (x < xLength && y < yLength && z < zLength){
                        extendedArray[x, y, z] = originalArray[x, y, z];
                    }
                    else{
                        extendedArray[x, y, z] = blockID;
                    }
                }
            }
        }
        return extendedArray;
    }
    //lift the current array by populating bytes below the array on y dimension. done to create the base layer filled with basalt block
    public static byte[,,] Lift3DByteArray(byte[,,] originalArray, int heightExt, byte byteFill){
        int xDimension = originalArray.GetLength(0);
        int yDimension = originalArray.GetLength(1) + heightExt;
        int zDimension = originalArray.GetLength(2);

        byte[,,] liftedByteArray = new byte[xDimension, yDimension, zDimension];

        for (int y = 0; y < heightExt; y++){
            for (int x = 0; x < xDimension; x++){
                for (int z = 0; z < zDimension; z++){
                    liftedByteArray[x, y, z] = byteFill;
                }
            }
        }
        for (int y = heightExt; y < yDimension; y++){
            for (int x = 0; x < xDimension; x++){
                for (int z = 0; z < zDimension; z++){
                    liftedByteArray[x, y, z] = originalArray[x, y - heightExt, z];
                }
            }
        }
        return liftedByteArray;
    }
    public static float GetVector3Length(Vector3 vector3){
        float x = vector3.x;
        float y = vector3.y;
        float z = vector3.z;

        float length = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
        return length;
    }
}