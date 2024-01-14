using UnityEngine;
using UnityEngine.UI;

public class GeneratorBackScript : MonoBehaviour
{
    void Start()
    {
        /*we want to activate the previous menu through this button. however, 
        since we cannot find the previous menu through find function due to it being inactive
        and the generator script has information about what previous menu gameobject is, the only way to activate
        the previous menu is to refer to the generatorScript and bind the back button in the layer UI with function
        to activate the previous menu through generatorScript */
        GeneratorScript generatorScript = GetComponentInParent<GeneratorScript>();
        Button button = GetComponent<Button>();
        
        button.onClick.AddListener(() => generatorScript.previousMenu.SetActive(true));
        button.onClick.AddListener(generatorScript.Kill);
    }
}
