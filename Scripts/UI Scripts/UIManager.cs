using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static readonly Color32 enabledColor = new Color32(255, 255, 255, 255);
    public static readonly Color32 disabledColor = new Color32(110, 110, 110, 255);
    public GameObject editFramework;
    public void ExitApplication(){
        Application.Quit();
    }    

    //instantiating a new generator and a new empty layer. called when "generate new" button is being pressed in mine maps menu
    public void GenerateNewEditFramework(){
        GameObject editFrameWorkInst = Instantiate(editFramework);
        GeneratorScript generatorScript = editFrameWorkInst.GetComponent<GeneratorScript>();
        generatorScript.AddEmptyLayer();
        generatorScript.GetPreviousMenu("Mine Maps");
    }

    //instantiating a generator and instantiating layers based on the length of y dimension that the gridMap has. called when editing an existing map
    /*when pressing an existing map, the file will seek for its corresponding file I/O and read all the string to convert it to 3D byte array.
    The 3D byte array then will be sliced and each slice will be used to instantiate layer
    */
    public void LoadEditFrameWork(byte[,,] gridMap){
        GameObject editFrameWorkInst = Instantiate(editFramework);
        GeneratorScript generatorScript = editFrameWorkInst.GetComponent<GeneratorScript>();
        for (int y = 0; y < gridMap.GetLength(1); y++){
            byte[,] sliceMap = new byte[GeneratorScript.gridLength, GeneratorScript.gridWidth];
            for (int x = 0; x < gridMap.GetLength(0); x++){
                for (int z = 0; z < gridMap.GetLength(2); z++){
                    sliceMap[x, z] = gridMap[x, y, z];
                }
            }
            generatorScript.AddLoadedLayer(sliceMap);
        }
        generatorScript.GetPreviousMenu("Edit Maps");
    }
}

