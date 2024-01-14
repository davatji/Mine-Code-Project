using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapContainer : MonoBehaviour
{
    public GameObject mapLayer;
    public GameObject cell;
    public List<GameObject> layers;
    public int layerIndex;
    public int totalLayer;
    void Awake()
    {
        byte[,,] mapBluePrint = DataHolder.mapBluePrint;

        //setting the amount of layer to be the y Dimension of the simplified map blue print
        totalLayer = mapBluePrint.GetLength(1);


        //instantiating layers based on the amount of the layer and for each layer, creating cells and put them on the array that each layer script has.
        //modify the cells on each layer based on the sliced array that represents the 2d map of the layer.
        for (int y = 0; y < mapBluePrint.GetLength(1); y++){
            
            GameObject layerInstance = Instantiate(mapLayer, transform); 
            layers.Add(layerInstance);
            layerInstance.name = $"Layer {y}";
            
            for (int x = 0; x < mapBluePrint.GetLength(0); x++){
                for (int z = 0; z < mapBluePrint.GetLength(2); z++){
                    
                    float adjustedX = (float)x / 48 * 450;
                    float adjustedZ = (float)z / 32 * 300;

                    GameObject cellInstance = Instantiate(cell, layerInstance.transform);
                    layerInstance.GetComponent<GameLayerManager>().cells[x, z] = cellInstance;

                    cellInstance.transform.localPosition = new Vector3(-225 + 9.375f / 2 + adjustedX, -150 + 9.375f / 2 + adjustedZ, 0);
                    cellInstance.name = $"{x},{z}";

                    //for each cells, will check its corresponding byte in simplified 3D map (which consists of only byte 0, 1, 30, and 31)
                    //color the cell according to the byte 
                    Color32 color;
                    switch (mapBluePrint[x, y, z])
                    {
                        case 0:
                            color = new Color32(255, 255, 255, 255);
                            break;
                        case 30:
                            color = new Color32(0, 161, 174, 255);
                            break;
                        case 31:
                            color = new Color32(161, 0, 207, 255);
                            break;
                        default:
                            color = new Color32(75, 75, 75, 255);
                            break;
                    }
                    cellInstance.GetComponent<Image>().color = color;
                }
            }
            //will initially only activate the current layer (which in normal case is set on the spawn position layer OR the first layer is spawn pos is not set)
            if (y == DataHolder.spawnPosLayerIndex){
                layerIndex = y;
                layerInstance.SetActive(true);
            }
            else{
                layerInstance.SetActive(false);
            }
        }

        if (layerIndex == 0){
            layers[0].SetActive(true);
        }
    }
}
