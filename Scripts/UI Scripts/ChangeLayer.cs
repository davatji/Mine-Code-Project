using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeLayer : MonoBehaviour
{
    public Image leftButton;
    public Image rightButton;
    GeneratorScript generatorScript;

    //the logic of changing layer
    /*
    IncrementCurrentLayer function is bound with right arrow button and will increment the index of current layer in the generator script instance.
    DecrementCurrentLayer function is bound with left arrow button and will decrement the index of current layer in the generator script instance

    both function will disactivate the previous active layer and activate the new layer based on incremented/decremented index

    the generator script will iterate through all layer everytime a new layer is made. if the iterated layer is the first layer, disable the left arrow button.
    if it is the last layer, disable the right arrow button. 
    */
    void Start()
    {
       generatorScript = GetComponentInParent<GeneratorScript>();
    }
    public void IncrementCurrentLayer(){
        int currentLayer = generatorScript.currentLayer;
        if (currentLayer != generatorScript.totalLayer)
        {
            generatorScript.currentLayer += 1;
            generatorScript.layers[currentLayer - 1].SetActive(false);
            generatorScript.layers[currentLayer].SetActive(true);
        }
    }
    public void DecrementCurrentLayer(){
        int currentLayer = generatorScript.currentLayer;
        if (currentLayer != 1){
            generatorScript.currentLayer -= 1;
            generatorScript.layers[currentLayer - 1].SetActive(false);
            generatorScript.layers[currentLayer - 2].SetActive(true);
        }
    }
    public void RightButtonIndicatorSetActive(bool toActive){
        if (toActive){
            rightButton.color = UIManager.enabledColor;
        }
        else{
            rightButton.color = UIManager.disabledColor;
        }
    }
    public void LeftButtonIndicatorSetActive(bool toActive){
        if (toActive){
            leftButton.color = UIManager.enabledColor;
        }
        else{
            leftButton.color = UIManager.disabledColor;
        }
    }
}
