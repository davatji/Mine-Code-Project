using UnityEngine;
using TMPro;

public class CountText : MonoBehaviour
{
    //this powers the layer count indicator and will be updated with string value current total layer everytime a new layer is being added
    public void UpdateText(string updatedText){
        GetComponent<TextMeshProUGUI>().text = updatedText;
    }
}
