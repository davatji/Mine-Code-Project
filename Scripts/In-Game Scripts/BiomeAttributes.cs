using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Biome", menuName = "Biome")]
public class BiomeAttributes : ScriptableObject
{
    // instance variable of the script which will vary for every biome. the variaton reflects the difference in world generation algorithm.
    public float mappingScale;
    public int mappingOffset;
    public Node[] nodes; //nodes array, whose element represents the nodes pass in getVoxel function. contains attributes essential for determining the details of nodes distribution
}


[Serializable]
public class Node
{
    //node class. applied biome nodes will be iterated in the procedural terrain generation function based on properties as below:
    public string name;
    public byte voxelID;
    public int rarity;
    public float maxHeight;
    public float minHeight;
    public float nodeScale;
    public float nodeOffset;
}