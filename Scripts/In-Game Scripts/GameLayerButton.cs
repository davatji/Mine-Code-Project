using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//the logic to increment or decrement the in-game map layer 
public class GameLayerButton : MonoBehaviour
{
    public MapContainer mapContainer;

    //pair here is to refer to the pair button script, since the action of a button sometime influence the pair thus reference is needed
    public GameLayerButton pair;
    public LayerTextIndicatorScript textScript;
    Image image;

    void Start()
    {
        //if the current layer is 0, disable the down button. if the current layer is last layer, disable the up button
        image = GetComponent<Image>();
        if (mapContainer.layerIndex == 0 && gameObject.name == "Button Down"){
            DisableButton();
        }
        if (mapContainer.layerIndex == mapContainer.totalLayer - 1 && gameObject.name == "Button Up"){
            DisableButton();
        }
    }

    //increment the layer and change the activated in-game layer according to the incremented layer
    public void Increment()
    {
        
        if (mapContainer.layerIndex < mapContainer.totalLayer - 1){
            if (mapContainer.layerIndex == 0){
                pair.EnableButton();
            }
            mapContainer.layers[mapContainer.layerIndex].SetActive(false);
            mapContainer.layerIndex += 1;
            mapContainer.layers[mapContainer.layerIndex].SetActive(true);
            if (mapContainer.layerIndex == mapContainer.totalLayer - 1){
                DisableButton();
            }
            else{
                EnableButton();
            }
        }        
        //textScript powers the layer indicator game object, and will be updated everytime the layer is incremented or decremented
        textScript.UpdateText();
    }

    //same logic as increment, but instead it decrements
    public void Decrement(){
        if (mapContainer.layerIndex > 0){
            if (mapContainer.layerIndex == mapContainer.totalLayer - 1){
                pair.EnableButton();
            }
            mapContainer.layers[mapContainer.layerIndex].SetActive(false);
            mapContainer.layerIndex -= 1;
            mapContainer.layers[mapContainer.layerIndex].SetActive(true);
            if (mapContainer.layerIndex == 0){
                DisableButton();
            }
            else{
                EnableButton();
            }
        }      
        textScript.UpdateText();  
    }
    void DisableButton(){
        image.color = UIManager.disabledColor;
    }
    void EnableButton(){
        image.color = UIManager.enabledColor;
    }
}

