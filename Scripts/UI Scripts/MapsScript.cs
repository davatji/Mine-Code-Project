using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;

public class MapsScript : MonoBehaviour
{
    //referencing mostly gameobjects and their component to edit them / get information from them
    public GameObject unavailPrompt;
    public TextMeshProUGUI textComp;
    public GameObject mapButton;
    public GameObject image;
    public GameObject rightButton;
    public GameObject leftButton;
    public GameObject scaleUI;
    public TMP_InputField scaleXField;
    public TMP_InputField scaleYField;
    public TMP_InputField scaleZField;
    public WorldTypeRadio worldTypeRadio;
    public GameObject loadingScreen;
    
    private int currentIndex;

    /*the concept here is that the map script will initially have its current index as 0. the current index corresponds to the playerprefs that contains the map name.
    whenever the map menu UI is initialized/activated/refreshed (using load function), the current index is set to 0 and the corresponding map name would follow the index, except that
    if playerpref is empty, the corresponding map name would be set to default, which is "No Saved Map". 
    
    the currentIndex can be incremented and decremented, and the corresponding map name would follow and be shown on the map button. the map name is the same as the file name that stores
    the 3D byte array that contains the map information. So that whenever the map button is clicked, it will see the currentIndex and get the map name through accessing the string playerpref
    by current index. the map name then will be used as a path to open file I/O to access the map 3D array and then creating world / generator based on that array
    
    There are getint and getstring playerprefs. the getint playerprefs will provide the information about the amount of maps that the application has. by default, it is zero
    The getstring playerprefs is trying to mimic the list data structure. it is essentially a dictionary with key: number in form of string datastructure that acts as the index of the value

    Whenever a new map is created, the getint playerprefs will increment and the previous value of getint playerprefs will be used as the key for new getstring value (map name),
    so that in getstring playerprefs, map name can be accessed by its index.

    */

 
    private void Start(){
        currentIndex = 0;
        UpdateText();
    }
    public void OnEnable(){
        currentIndex = 0;
        UpdateText();
    }
    public void Load()
    {
        currentIndex = 0;
        UpdateText();
    }
    public void Increment(){
        if (currentIndex < PlayerPrefs.GetInt("Map Count") - 1){
            currentIndex += 1;
            UpdateText();
        }
    }
    public void Decrement(){
        if (currentIndex > 0){
            currentIndex -= 1;
            UpdateText();  
        }
    }

    //update text will be called whenever initializing, activating, or refreshing the map UI or when incrmeenting or decrementing the layer index
    //this function will visually reflect the value of currentIndex by showing the name of the map that corresponds to the currentIndex on the button
    //and setting the map button function argument to the right map file path
    private void UpdateText(){
    
        if (currentIndex >= PlayerPrefs.GetInt("Map Count") - 1){
            rightButton.GetComponent<Image>().color = UIManager.disabledColor;
        }
        else{
            rightButton.GetComponent<Image>().color = UIManager.enabledColor;
        }
        if (currentIndex == 0){
            leftButton.GetComponent<Image>().color = UIManager.disabledColor;
        }
        else{
            leftButton.GetComponent<Image>().color = UIManager.enabledColor;

        }
        string mapName = PlayerPrefs.GetString(currentIndex.ToString(), "No Saved Map");
        textComp.text = mapName;

        if (PlayerPrefs.GetInt("Map Count") == 0){
            mapButton.GetComponent<Image>().color = UIManager.disabledColor;
            image.GetComponent<Image>().color = UIManager.disabledColor;
        }
        else{
            mapButton.GetComponent<Image>().color = UIManager.enabledColor;
            image.GetComponent<Image>().color = UIManager.enabledColor;        
        }
    }

    //based on the current map name, open the file I/O consisting of flat array and converting it to 3D array to instantiate a generator
    public void Open(){
        if (textComp.text == "No Saved Map"){
            return;
        }

        string directoryPath = "World Data";
        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }
        string path = $"{directoryPath}\\{textComp.text}.txt";
        
        byte[] flatArray = FunctionKit.ParseFileToFlatArray(path);
        int yDimension = flatArray.Length / GeneratorScript.gridLength / GeneratorScript.gridWidth;
        byte[,,] volumeGrid = FunctionKit.FlatArrayTo3DArray(flatArray, GeneratorScript.gridLength, yDimension, GeneratorScript.gridWidth);

        GameObject parentObject = GameObject.Find("Main Camera");
        UIManager uIManager = parentObject.GetComponent<UIManager>();
        uIManager.LoadEditFrameWork(volumeGrid);

    }

    //used to generate the world. activating scale UI to prompt the user about the scale of the world on each dimension
    public void ActivateScaleUI(){
        if (PlayerPrefs.GetInt("Map Count") != 0){
            scaleUI.SetActive(true);
        }
    }

    //bound to the generate world button. will process the file corresponds to the map name and use them to generate the world
    public void LoadWorld(){
        Instantiate(loadingScreen);
        if (textComp.text == "No Saved Map"){
            return;
        }
        string directoryPath = "World Data";
        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }

        string path = $"{directoryPath}\\{textComp.text}.txt";

        string worldType = worldTypeRadio.options[worldTypeRadio.currentOption].gameObject.name;
        if (worldType == "Tunnel"){
            byte[] flatArray = FunctionKit.ParseFileToFlatArray(path);
            int yDimension = flatArray.Length / GeneratorScript.gridLength / GeneratorScript.gridWidth;
            byte[,,] volumeGrid = FunctionKit.FlatArrayTo3DArray(flatArray, GeneratorScript.gridLength, yDimension, GeneratorScript.gridWidth);

            byte[,,] simplifiedMap = FunctionKit.Simplify3DByteArray(volumeGrid, new byte[3]{0, 30, 31}, 1);

            int xScale = int.Parse(scaleXField.text);
            int yScale = int.Parse(scaleYField.text);
            int zScale = int.Parse(scaleZField.text);
            
            Tuple<byte[,,], Vector3, List<Vector3>, List<Vector3Int>> packedInitialWorldInformation = FunctionKit.ProcessMap(volumeGrid, xScale, yScale, zScale);
            byte[,,] enlargenedVolumeGrid = packedInitialWorldInformation.Item1;
            Vector3 spawnGlobalPos = packedInitialWorldInformation.Item2 + new Vector3(0, 2, 0);
            List<Vector3> posObj = packedInitialWorldInformation.Item3; 
            List<Vector3Int> simplifiedPosObj = packedInitialWorldInformation.Item4;

            byte[,,] extendedVolumeGrid = FunctionKit.Extend3DByteArray(enlargenedVolumeGrid, 0, new Vector3Int(0, 10, 0));
            byte[,,] liftedVolumeGrid = FunctionKit.Lift3DByteArray(extendedVolumeGrid, 2, 5);
            int spawnPosLayerIndex = (int)spawnGlobalPos.y / yScale;

            DataHolder.EvaluateWorldInformation(liftedVolumeGrid, simplifiedMap, spawnGlobalPos, spawnPosLayerIndex, posObj, simplifiedPosObj);
            SceneManager.LoadScene(1);
        }
        else{
            unavailPrompt.SetActive(true);   
        }
    }

    //delete all the playerprefs and world data file
    public void DeleteData(){
        int count = PlayerPrefs.GetInt("Map Count");
        for (int i = 0; i < count; i++){
            
            string prePath = "World Data\\";
            string subPath = PlayerPrefs.GetString(i.ToString());
            string extension = ".txt";

            string path = prePath + subPath + extension;
            if (File.Exists(path)){
                File.Delete(path);
            }
        }
        PlayerPrefs.DeleteAll();
    }
}
