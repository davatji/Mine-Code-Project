using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class SaveAndGenerate : MonoBehaviour
{
    //referencing to mainly game objects and their component to edit them / get information from them
    public TMP_InputField inputField;
    public TMP_InputField scaleXField;
    public TMP_InputField scaleYField;
    public TMP_InputField scaleZField;
    public WorldTypeRadio worldTypeRadio;
    public GameObject unavailPrompt;
    public GameObject loadingScreen;
    private GeneratorScript generatorScript;
    private void Start()
    {
        generatorScript = GetComponentInParent<GeneratorScript>();
    }
    //whenever the save button is being pressed, create a new key-value pair with key: index in string and value: map name
    //further documentation on playerprefs logic can be read on MapScript class
    public void SaveGrid(){

        string directoryPath = "World Data";
        if (!Directory.Exists(directoryPath)){
            Directory.CreateDirectory(directoryPath);
        }
        string path = $"{directoryPath}\\{inputField.text}.txt";
        generatorScript.SaveLayersToFile(path);
        IncrementPlayerPrefs();
    }
    private void IncrementPlayerPrefs(){
        int getInt = PlayerPrefs.GetInt("Map Count", 0);
        PlayerPrefs.SetString(getInt.ToString(), inputField.text);
        PlayerPrefs.SetInt("Map Count", getInt + 1);
    }

    //generating the world, with logic exactly the same as the one in MapScript class LoadWorld function
    public void GenerateWorld(){
        
        Instantiate(loadingScreen);
        string worldType = worldTypeRadio.options[worldTypeRadio.currentOption].gameObject.name;
        if (worldType == "Tunnel"){
            byte[,,] volumeGrid = generatorScript.GetLayers();

            int xScale = int.Parse(scaleXField.text);
            int yScale = int.Parse(scaleYField.text);
            int zScale = int.Parse(scaleZField.text);

            byte[,,] simplifiedMap = FunctionKit.Simplify3DByteArray(volumeGrid, new byte[3]{0, 30, 31}, 1);
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
            //soon: incorporating another world type, which follows quite different world generation logic
        }
    }
}
