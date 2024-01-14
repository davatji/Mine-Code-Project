using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    public GameObject atomicGrid;
    public Transform gridContainer;
    public Color32[] colors;
    //2D array that defines the 2D map of the layer, which is editable by pressing grid cells 
    public byte[,] layerMap;
    public int layerIndex;

    //static variable that stores the information about what block ID is currently being used to "color" the grid. made static so that
    //the change of block ID will be reflected in all layers
    private static byte currentByte = 0;
    private GeneratorScript generatorScript;
    public void Awake(){
        generatorScript = GetComponentInParent<GeneratorScript>();
    }
    public void Populate(byte[,] sliceArray)
    {
        layerMap = sliceArray;
    }

    //embodiment of the layerMap, which is essentially creating cells with each cell represents each byte of layerMap byte 2D array with unique color.
    public void Decorate()
    {
        for (int x = 0; x < GeneratorScript.gridLength; x++){
            for (int z = 0; z < GeneratorScript.gridWidth; z++){
                GameObject atomicGridInstance = Instantiate(atomicGrid, gridContainer);
                atomicGridInstance.GetComponent<Image>().color = colors[layerMap[x, z]];
                atomicGridInstance.transform.localPosition = new Vector3(x * 30 - 720 + 15, z * 30 - 480 + 15, 0);
                atomicGridInstance.name = $"{x},{z}, Layer: {layerIndex}";
            }
        }
    }

    //this function will be bound with every cell. if a cell is being clicked through event trigger, this function will be executed with argument about the gameobject data being passed
    //this function will change the element of 2D array that the cell represents with the currentByte value and changes the cell color accordingly
    public void ToggleCell(BaseEventData baseEventData){
        PointerEventData pointerEventData = baseEventData as PointerEventData;
        GameObject cell = pointerEventData.selectedObject;
    
        string[] indexes = cell.name.Split(",");
        int xIndex = int.Parse(indexes[0]);
        int zIndex = int.Parse(indexes[1]);

        Image imageComp = cell.GetComponent<Image>();
        
        //if the currentByte is spawn byte (only one can exist in the generatoe):
        if (currentByte == 30 && layerMap[xIndex, zIndex] != 30){
            
            //check for the previous last spawn cell info, which is a cell that has been overwritten by previous spawn byte.
            //if it exists, restore the overwritten cell state
            if (generatorScript.lastSpawnCellInfo != null){
                int x = generatorScript.lastSpawnCellInfo.gridPos.x;
                int z = generatorScript.lastSpawnCellInfo.gridPos.z;
                generatorScript.lastSpawnCellInfo.imageComp.color = colors[generatorScript.lastSpawnCellInfo.blockID];
                generatorScript.lastSpawnCellInfo.layerMap[x, z] = generatorScript.lastSpawnCellInfo.blockID;
            }
            //the current cell information that will be overwritten will be stored in case that the spawn byte will change anytime and we need to restore
            //the cell state as if it was never been overwritten by spawn byte 
            CellInfo currentCellInfo = new CellInfo(layerMap, new GridPos(xIndex, zIndex), layerMap[xIndex, zIndex], imageComp);
            generatorScript.lastSpawnCellInfo = currentCellInfo;
        }
        else if (layerMap[xIndex, zIndex] == 30 && currentByte != 30){
            //if the spawn byte is overwritten by non-spawn byte, we do not care about the last non-spawn byte that the spawn byte overwrote
            generatorScript.lastSpawnCellInfo = null;
        }
        
        layerMap[xIndex, zIndex] = currentByte;
        imageComp.color = colors[currentByte];
    }
    public void ClearGrid(){

        //iterate through all the element of the layerMap array and cells and change it to 0 (air) with its color correspondance (white)
        for (int i = 0; i < GeneratorScript.gridLength; i++){
            for (int j = 0; j < GeneratorScript.gridWidth; j++){

                //if one of the cell / array element is spawn byte, delete the previous overwritten cell information since it is no longer relevant
                if (layerMap[i, j] == 30){
                    generatorScript.lastSpawnCellInfo = null;
                }
                layerMap[i, j] = 0;
                GameObject tempGameObject = GameObject.Find($"{i},{j}, Layer: {layerIndex}");
                tempGameObject.GetComponent<Image>().color = colors[0];
            }
        }        
    }

    //changing byte 1 to byte 0 within the layermap element along with its color correspondance and vica versa
    public void Inverse(){
        for (int i = 0; i < GeneratorScript.gridLength; i++){
            for (int j = 0; j < GeneratorScript.gridWidth; j++){
                if (layerMap[i, j] == 1){
                    layerMap[i, j] = 0;
                    GameObject tempGameObject = GameObject.Find($"{i},{j}, Layer: {layerIndex}");
                    tempGameObject.GetComponent<Image>().color = colors[0];
                }
                else if (layerMap[i, j] == 0){
                    layerMap[i, j] = 1;
                    GameObject tempGameObject = GameObject.Find($"{i},{j}, Layer: {layerIndex}");
                    tempGameObject.GetComponent<Image>().color = colors[1];
                } 
            }
        }        
    }

    //changing currentByte, bound with the block option UI
    public void ChangeBlock(byte blockID){
        currentByte = blockID;
    }

    //destroying the generator and getting back to the previous menu that generatorScript has
    public void DestroyGenerator(bool activatePreviousMenu){
        generatorScript.previousMenu.SetActive(activatePreviousMenu);
        Destroy(generatorScript.gameObject);
    }
}
