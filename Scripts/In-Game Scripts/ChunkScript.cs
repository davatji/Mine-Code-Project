using System;
using System.Collections.Generic;
using UnityEngine;


//will be a script component of pre-instantiated chunk game objects
public class ChunkScript
{
    public bool active;
    public GameObject chunkObject;
    public ChunkCoord chunkCoord;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    public World world;
    public byte[,,] chunkVoxelsMap = new byte[DataHolder.chunkSideInVoxel, DataHolder.chunkHeightInVoxel, DataHolder.chunkSideInVoxel];
    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    
    public ChunkScript(World _world, int coorX, int coorZ){
        world = _world;
        active = true;
        chunkCoord = new ChunkCoord(coorX, coorZ);
    }


    //generating chunk. not necessarily called when the chunk is instantiated. rather this function is done gradually
    // in a queue to decrease the amount of work that a single frame does    
    public void GenerateChunk(){

        //creating a new gameobject and setting its component. populating map and according to that map, create vertices -> faces to accumulate to become a mesh
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.name = $"Chunk {chunkCoord.x}, {chunkCoord.z}";

        //transforming the chunk to expected coordinates
        PositionateChunk(chunkCoord.x, chunkCoord.z);
        PopulateMap(chunkVoxelsMap);
        AddMeshToData();
        CreateVoxels();
        
    }
    void PopulateMap(byte[,,] voxelMap){

        //will refer to the world blueprint (part of procedural algorithm generation) and populate the map based on the received voxel ID
        for (int x = 0; x < voxelMap.GetLength(0); x++){
            for (int y = 0; y < voxelMap.GetLength(1); y++){
                for (int z = 0; z < voxelMap.GetLength(2); z++){
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + chunkObject.transform.position);
                }
            }
        }
    }
    void AddMeshToData(){
        for (int y = 0; y < DataHolder.chunkHeightInVoxel; y++){
            for (int x = 0; x < DataHolder.chunkSideInVoxel; x++){
                for (int z = 0; z < DataHolder.chunkSideInVoxel; z++){
                    if(chunkVoxelsMap[x, y, z] != 0){
                        AddVoxelInformation(new Vector3(x, y, z), chunkVoxelsMap);
                    }
                }
            }
        }
    }
    void AddVoxelInformation(Vector3 position, byte[,,] voxelMap){
        byte blockID = voxelMap[(int)position.x, (int)position.y, (int)position.z];
        BlockType blockType = world.blockTypes[blockID];

        //for each faces: check whether the voxel that it is facing is solid. the face will be rendered if and only if the faced block is not solid / transparent
        //this is done to not render unnecessary mesh inside the outer mesh (performance optimization)
        for (int i = 0; i < VoxelDataScript.voxelTriangles.GetLength(0); i++){

            Vector3 relativeFaceCheckPos = VoxelDataScript.faceCheck[i];
            Vector3 absoluteFaceCheckPos = relativeFaceCheckPos + position;
            bool blockState = IsSolidVoxel(absoluteFaceCheckPos, voxelMap);
            

            if (!blockState){
                //adding face triangles by combining the vertex local position / offset with the voxel absolute position (essentially positioning a new vertex in absolute position)
                int facePosition = blockType.GetPositionIndex(i);
                AddUvTexture(facePosition);
                for (int j = 0; j < VoxelDataScript.voxelTriangles.GetLength(1); j++){
                    int triangleIndex = VoxelDataScript.voxelTriangles[i, j];
                    vertices.Add(VoxelDataScript.vertexIndices[triangleIndex] + position);
                }

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                
                vertexIndex += 4;
                
            }
            // later on project for optimization: decrease the amount of vertices by inter-face vertices sharing
        }
    }
    bool IsVoxelInChunk(Vector3 position){
        if (position.x >= 0 && position.x < DataHolder.chunkSideInVoxel && position.y >= 0 && position.y < DataHolder.chunkHeightInVoxel && position.z >= 0 && position.z < DataHolder.chunkSideInVoxel){
            return true;
        }
        return false;
    }
    bool IsSolidVoxel(Vector3 position, byte[,,] voxelMap){        
        //if the voxel is outside the local chunk, will see on the global scale blueprint whether the corresponding block is solid
        //undefined voxels are essentialy air blocks according to the blueprint
    
        if(!IsVoxelInChunk(position)){
            return world.blockTypes[world.GetVoxel(position + chunkObject.transform.position)].isSolid;
        }
        else{
        //if the relative position is defined within the local scope of the current chunk, will check the map instead
            byte blockTypeIndex = voxelMap[(int)position.x, (int)position.y, (int)position.z];
            return world.blockTypes[blockTypeIndex].isSolid;
        }
    }

    //will carve the texture material according to block type and its face (through some fancy math)
    void AddUvTexture(int facePosition){
        float initX = (facePosition - 1) % DataHolder.texturePackWidthInBlock;
        float initY = 4 - ((facePosition - 1) / DataHolder.texturePackWidthInBlock);

        uvs.Add(new Vector2((float)initX / 5f, (float)initY / 5f));
        uvs.Add(new Vector2((float)initX / 5f, (float)(initY + 1) / 5f));
        uvs.Add(new Vector2((float)(initX + 1) / 5f, (float)initY / 5f));
        uvs.Add(new Vector2((float)(initX + 1) / 5f, (float)(initY + 1) / 5f));
        
    }
    void CreateVoxels(){
        //passing the list of vertices, triangles, and texture to the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
    void PositionateChunk(int coorX, int coorZ){
        Vector3 locationOffSet = new Vector3((float)coorX * DataHolder.chunkSideInVoxel, 0f, (float)coorZ * DataHolder.chunkSideInVoxel);
        chunkObject.transform.position = locationOffSet; 
    }
    
}
