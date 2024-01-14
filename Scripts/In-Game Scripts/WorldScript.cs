using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class World : MonoBehaviour
{
    public Material material;
    public Transform player;

    //render distance is essentially a range for iterating surrounding chunks
    public int playerRenderDistance = 5;

    /*
    we need to reference an instance of biome attribute (currently default biome), which stores essential information that influences the
    algorithm of the world generation.
    this is particularly done to give the ability to implement different biomes in different region of chunks (serializing biome algorithm customization)
    */
    public BiomeAttributes biome;
    public BlockType[] blockTypes;
    private ChunkScript[,] chunks = new ChunkScript[DataHolder.worldLengthInChunk, DataHolder.worldWidthInChunk];
    private List<ChunkScript> activeChunks = new List<ChunkScript>();
    private List<ChunkScript> chunksQueueToGenerate = new List<ChunkScript>(); 
    private ChunkCoord currChunkCoord;
    
    public float unscaledDeltaTime;

    void Start(){
        //setting the initial unscaledDeltaTime to 0.02 second in case that the debugging screen happen to refer to the world before the first frame is done (preventing null value)
        unscaledDeltaTime = 0.02f;

        SpawnPlayer(DataHolder.globalSpawnPos);
        currChunkCoord = Vector3ToChunkCoord(player.position);

        GenerateWorld();
    }
    void Update(){
        //storing the current frame unscaledDeltaTime in public variable so that it can be referred from debugging screen script
        unscaledDeltaTime = Time.unscaledDeltaTime;

        //optimization: instead of generating chunks all at once at certain frame, try to distribute it to next frames by adding the instantiated chunk to the queue
        // for generating its data and rendering.
        if (chunksQueueToGenerate.Count != 0){
            chunksQueueToGenerate[0].GenerateChunk();
            chunksQueueToGenerate.RemoveAt(0);
        }
        //optimization: only updating the chunks when the player moves from one chunk to another instead of each frame
        if (!currChunkCoord.Equals(Vector3ToChunkCoord(player.position))){
            UpdateChunks();
            currChunkCoord = Vector3ToChunkCoord(player.position);
        }        
    }

    //built-in camera is treated as player, and spawning the player is initially setting the position of the camera
    void SpawnPlayer(Vector3 globalPos){
    
        player.position = globalPos;
    }

    //based on the player spawn position, the chunk coordinates will be determined and iterate the surrounding chunks to be generated
    void GenerateWorld(){
        ChunkCoord currentChunkCoord = Vector3ToChunkCoord(player.position);
        for (int x = 0; x < currentChunkCoord.x + playerRenderDistance + 1; x++){
            for (int z = currentChunkCoord.z - playerRenderDistance; z < currentChunkCoord.z + playerRenderDistance + 1; z++){
                if (IsChunkInWorld(x, z)){
                    InstantiateChunk(x, z);
                    chunks[x, z].GenerateChunk();
                }
            }
        }
    }

    //will iterate through player current chunk position. the range of iteration is based on the player render distance
    /*
    The iterated chunk will be checked
    1. If it is null, instantiate a new chunk script, initally set to be active, and add the script to the queue to generate a new gameobject and populate chunk data
    2. If it is inactive and the game object hasn't been generated, activate it and add them to queue for game object and chunk data generation
    3. If it is inactive but has been generated, just activate it
    */
    /*note that the chunk has 5 state: 
    1. active with its gameobject and data defined, 
    2. active with its gameobject and data undefined (still in queue),
    3. null, 
    4. inactive and its game object and data undefined 
    5. inactive but its game object and data defined
    */
    void UpdateChunks(){
        List<ChunkScript> noLongerActiveChunks = new List<ChunkScript>(activeChunks);
        ChunkCoord currentChunkCoord = Vector3ToChunkCoord(player.position);
        for (int x = currentChunkCoord.x - playerRenderDistance; x < currentChunkCoord.x + playerRenderDistance + 1; x++){
            for (int z = currentChunkCoord.z - playerRenderDistance; z < currentChunkCoord.z + playerRenderDistance + 1; z++){
                if (IsChunkInWorld(x, z)){
                    if (chunks[x, z] == null){
                        InstantiateChunk(x, z);
                        chunksQueueToGenerate.Add(chunks[x, z]);
                    }
                    else if (!chunks[x, z].active && chunks[x, z].chunkObject == null){
                        chunks[x, z].active = true;
                        chunksQueueToGenerate.Add(chunks[x, z]);

                        activeChunks.Add(chunks[x, z]);
                    }
                    else if (!chunks[x, z].active){
                        chunks[x, z].active = true;
                        chunks[x, z].chunkObject.SetActive(true);
                        
                        activeChunks.Add(chunks[x, z]);
                    }
                    //at this point, the checked chunk is guaranteed to be still active
                    else{
                        noLongerActiveChunks.Remove(chunks[x, z]);
                    }
                }
            }
        }

        /*will check for previously active chunks that is outside of the player view distance. this will:
        1. if the chunk game object and its data hasn't been defined (still in queue), remove it from queue and disactivate it
        2. if the chunk game object has been defined, disactivate the chunk object
        */

        foreach (ChunkScript chunk in noLongerActiveChunks){
            if (chunk.chunkObject == null){
                chunksQueueToGenerate.Remove(chunk);
            }
            else{
                chunk.chunkObject.SetActive(false);
            }
            chunk.active = false;
            activeChunks.Remove(chunk);
        }
    }
    
    //instantiating a new chunk and assigning it into the chunk array
    void InstantiateChunk(int x, int z){
        chunks[x, z] = new ChunkScript(this, x, z);
        activeChunks.Add(chunks[x, z]);
    }

    //generalized function that receives global position of a voxel and returns the corresponding block type in byte
    //this function will essentially implement passes to a block in given coordinates
    public byte GetVoxel(Vector3 position){
        
        byte voxelID;

        //ABSOLUTE PASS
        if (!IsVoxelInWorld(position)){
            return 0;
        }

        //PSEUDO-BLUEPRINT PASS
        voxelID = DataHolder.worldBluePrint[(int)position.x, (int)position.y, (int)position.z];

        
        //if the block is exposed to the air, apply perlin noise to enhance the world generation algorithm so that the tunnel would be more natural-looking
        //currently disabled

        // //3D perlin noise PASS (for tunnel skin)
        // bool applyPerlinNoise = false;

        // try{
        //     if ((voxelID != 0) &&
        //     (DataHolder.worldBluePrint[(int)position.x + 1, (int)position.y, (int)position.z] == 0 ||
        //     DataHolder.worldBluePrint[(int)position.x - 1, (int)position.y, (int)position.z] == 0 ||
        //     DataHolder.worldBluePrint[(int)position.x, (int)position.y, (int)position.z + 1] == 0 ||
        //     DataHolder.worldBluePrint[(int)position.x, (int)position.y, (int)position.z - 1] == 0 ||
        //     DataHolder.worldBluePrint[(int)position.x, (int)position.y - 1, (int)position.z] == 0)){
        //         applyPerlinNoise = true;
        //     }
        // }
        // catch(IndexOutOfRangeException){
        //     applyPerlinNoise = true;
        // }

        // if (applyPerlinNoise && NoiseScript.GetPerlinNoise3D(position, 5, 0, 0.5f, DataHolder.worldLengthInChunk, DataHolder.worldWidthInChunk)){
        //     voxelID = 0;
        // }
        
        //NODES PASS (only applied to stone)

        //if the block is stone, will check whether it will be overwritten with a node, given its max min height and rarity
        if (voxelID == 1)
        {
            foreach (Node node in biome.nodes){
                //adding additional attribute of node to limit the height / depth of node distribution
                float normalizedRarity = (float)node.rarity / 100;
                float threshold = (float)Math.Pow((double)normalizedRarity, 0.5);
                if (NoiseScript.GetPerlinNoise3D(position, node.nodeScale, node.nodeOffset, threshold, DataHolder.worldLengthInChunk, DataHolder.worldWidthInChunk)){
                    voxelID = node.voxelID;
                }
            }
        }
        
        return voxelID;
    }

    //handy functions that help providing ease to the world logic
    public bool IsChunkInWorld(int coorX, int coorZ){
        if (coorX >= 0 && coorX < DataHolder.worldLengthInChunk && coorZ >= 0 && coorZ < DataHolder.worldWidthInChunk){
            return true;
        }
        return false;
    }
    public bool IsVoxelInWorld(Vector3 position){
        if (position.x >= 0 && position.x < DataHolder.worldLengthInVoxel && position.y >= 0 && position.y < DataHolder.worldHeightInVoxel && position.z >= 0 && position.z < DataHolder.worldWidthInVoxel){
            return true;
        }
        return false;
    }
    public ChunkCoord Vector3ToChunkCoord(Vector3 globalPos){
        int x = (int)(globalPos.x / DataHolder.chunkSideInVoxel);
        int z = (int)(globalPos.z / DataHolder.chunkSideInVoxel);
        return new ChunkCoord(x, z);
    }
    public bool IsSolidVoxelGlobal(Vector3 globalPos){
        //converting chunk global position to local position within the chunk voxel map

        Tuple<ChunkCoord, Vector3Int> packedLocalPos = GetLocalPos(globalPos);
        int chunkX = packedLocalPos.Item1.x;
        int chunkZ = packedLocalPos.Item1.z;

        int localCoordX = packedLocalPos.Item2.x;
        int localCoordZ = packedLocalPos.Item2.z;

        byte blockID = chunks[chunkX, chunkZ].chunkVoxelsMap[localCoordX, (int)globalPos.y, localCoordZ];

        return blockTypes[blockID].isSolid;
    }
    public Tuple<ChunkCoord, Vector3Int> GetLocalPos(Vector3 globalPos){
        int chunkCoordX = (int)globalPos.x / DataHolder.chunkSideInVoxel;
        int chunkCoordZ = (int)globalPos.z / DataHolder.chunkSideInVoxel;
        int localCoordX = (int)globalPos.x % DataHolder.chunkSideInVoxel;
        int localCoordZ = (int)globalPos.z % DataHolder.chunkSideInVoxel;

        ChunkCoord chunkCoord = new ChunkCoord(chunkCoordX, chunkCoordZ);
        Vector3Int localCoord = new Vector3Int(localCoordX, (int)globalPos.y, localCoordZ);

        return new Tuple<ChunkCoord, Vector3Int>(chunkCoord, localCoord);  
    }
    public void ToggleWorldTimeScale(bool pause){
        if (pause){
            Time.timeScale = 0;
        }
        else{
            Time.timeScale = 1;
        }
    }
}
public class ChunkCoord{
    public int x;
    public int z; 
    public ChunkCoord(int x, int z){
        this.x = x;
        this.z = z;
    }
}

//This is a serializable array whose elements can be added through high level interface in the unity workspace 
//make it possible to add new block with its necessary information through the inspector
[Serializable]
public class BlockType{
    public string blockName;
    public bool isSolid;
    public int backSurface;
    public int frontSurface;
    public int topSurface;
    public int bottomSurface;
    public int leftSurface;
    public int rightSurface;

    public int GetPositionIndex(int faceIndex){
        switch (faceIndex){
            case 0:
                return backSurface;
            case 1:
                return frontSurface;
            case 2: 
                return topSurface;
            case 3:
                return bottomSurface;
            case 4:
                return leftSurface;
            case 5:
                return rightSurface;
            default:
                return 0; 
        }
    }    
}
