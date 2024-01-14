using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;

public class GeneratorScript : MonoBehaviour
{
    public static readonly int gridLength = 48;
    public static readonly int gridWidth = 32;
    //default slice map to populate the grid would be full of byte 1
    public static byte[,] defaultSliceMap = InitializeDefaultSliceMap();

    //this gameobject will be instantiated each time a new layer is added
    public GameObject layerTemplate;

    //holds information about previous menu, the menu where the generator script is instantiated. this is done so that when exiting the generator, the program knows
    //which menu to go back to
    public GameObject previousMenu;
    public List<GameObject> layers = new List<GameObject>();
    public int totalLayer = 0;
    public int currentLayer = 1;

    //last spawn cell info is used to store the information about the last cell grid previous information before it is overwritten by
    //the spawn byte. this is done in case the user wants to change the spawn cell grid, the program knows which cell, what byte and its color
    //to restore the cell
    public CellInfo lastSpawnCellInfo;

    //creating static function so that creating 2D byte array during instance variable initialization is possible
    private static byte[,] InitializeDefaultSliceMap(){
        byte[,] map = new byte[gridLength, gridWidth];
        for (int x = 0; x < gridLength; x++){
            for (int z = 0; z < gridWidth; z++){
                map[x, z] = 1;
            }
        }
        return map;
    }
    void Start(){
        gameObject.name = "Generator";
    }

    //getting previous menu, the menu the generator is instantiated
    public void GetPreviousMenu(string name){
        previousMenu = GameObject.Find(name);
        previousMenu.SetActive(false);
    }

    //subset of addloaded layer, which is initializing the layer UI with grid container that consists of empty layer representation
    //which is 2 dimensional grid filled with gray colored cells, representing byte 1 (stone)
    public void AddEmptyLayer(){
        byte[,] defaultSliceMapCopy = FunctionKit.Copy2DByteArray(defaultSliceMap);
        AddLoadedLayer(defaultSliceMapCopy);
    }
    public void AddLoadedLayer(byte[,] sliceMap)
    {
        //add loaded layer, instantiating a new map UI that represents one layer, with cells container namely grid that represents each cell
        totalLayer ++;

        GameObject layer = Instantiate(layerTemplate, gameObject.transform);
        layers.Add(layer);

        layer.GetComponentInChildren<CurrentLayerText>().UpdateCurrentLayerText(totalLayer.ToString());
        layer.name = $"Layer {totalLayer}";
        LayerManager layerManager = layer.GetComponent<LayerManager>();
        layerManager.layerIndex = totalLayer;
        layerManager.Populate(sliceMap);
        layerManager.Decorate();

        //setting the added layer to be inactive if it is not the current layer
        if (totalLayer != currentLayer){
            layer.SetActive(false);
        }

        //updating the state of the arrow button. setting the first and last layer button to be inactive
        for (int i = 0; i < layers.Count; i++){
            GameObject eachLayer = layers[i];
            eachLayer.GetComponentInChildren<CountText>().UpdateText(totalLayer.ToString());
            ChangeLayer changeLayer = eachLayer.GetComponentInChildren<ChangeLayer>();
            if (i == 0){
                changeLayer.LeftButtonIndicatorSetActive(false);
            }
            else{
                changeLayer.LeftButtonIndicatorSetActive(true);
            }
            if (i == layers.Count - 1){
                changeLayer.RightButtonIndicatorSetActive(false);
            }    
            else{
                changeLayer.RightButtonIndicatorSetActive(true);
            }
        }
    }

    //this function will be called when saving the world. will get all the 2D array information from every layer UI instance and combining it to become 3D grid
    public byte[,,] GetLayers(){
        byte[,,] threeDimensionalGrid = new byte[gridLength, transform.childCount, gridWidth];
        for (int y = 0; y < transform.childCount; y++){
            GameObject layer = transform.GetChild(y).gameObject;
            LayerManager layerManager = layer.GetComponent<LayerManager>();
            byte[,] layerMap = layerManager.layerMap;
            for (int x = 0; x < gridLength; x++){
                for (int z = 0; z < gridWidth; z++){
                    threeDimensionalGrid[x, y, z] = layerMap[x, z];
                }
            }
        }
        return threeDimensionalGrid;
    }

    //saving 3D array representing the world in block ID by flattening it into 1D array and writing it on file I/O
    public void SaveLayersToFile(string filePath){
        byte[,,] threeDimensionalGrid = GetLayers(); 
        byte[] flattenedArray = FunctionKit.Flatten3DArray(threeDimensionalGrid);

        string csvGridData = string.Join(",", flattenedArray);
        File.WriteAllText(filePath, csvGridData);
    }
    public void Kill(){
        Destroy(gameObject);
    }
}


//information about cell necessary to restore the cell state when a spawn cell is changed
public class CellInfo
{
    public byte[,] layerMap;
    public GridPos gridPos;
    public byte blockID;
    public Image imageComp;
    public CellInfo(byte[,] layerMap, GridPos gridPos, byte blockID, Image imageComp){
        this.layerMap = layerMap;
        this.gridPos = gridPos;
        this.blockID = blockID;
        this.imageComp = imageComp; 
    }
}

//grid position data structure that brings ease in identifying grid cell
public class GridPos
{
    public int x;
    public int z;
    public GridPos(int x, int z){
        this.x = x;
        this.z = z;      
    }
}
