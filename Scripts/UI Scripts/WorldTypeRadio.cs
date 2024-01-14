using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class WorldTypeRadio : MonoBehaviour
{

    //creating pseudo radiobutton, by grouping the option gameobjects on the array and allowing only one of it to be activated.
    //each gameobject will have its index representation, which reflects the chosen value of the radio button
    //the radio button is used to determine the world type
    public Image[] options;
    public int currentOption = 0;

    void Start()
    {
        for (int i = 0; i < options.Length; i++){
            Image option = options[i];
            if (i == currentOption){
                option.color = UIManager.enabledColor;
            }
            else{
                option.color = UIManager.disabledColor;
            }
        }
    }

    public void ChangeOption(int index){
        options[currentOption].color = UIManager.disabledColor;
        currentOption = index;
        options[currentOption].color = UIManager.enabledColor;
    }
    
}
