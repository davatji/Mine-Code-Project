using UnityEngine;
using TMPro;

public class CurrentLayerText : MonoBehaviour
{
    //this will be called in generatorscript everytime the user changes its layer. the value passed would be the generatorScript current layer index after incremented
    public void UpdateCurrentLayerText(string updatedText)
    {
        GetComponent<TextMeshProUGUI>().text = updatedText;
    }
}
