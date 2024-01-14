using UnityEngine;
using TMPro;

public class LayerTextIndicatorScript : MonoBehaviour
{
    public MapContainer mapContainer;
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        UpdateText();
    }
    //updating layer text indicator according to the current layer index
    public void UpdateText(){
        text.text = (mapContainer.layerIndex + 1).ToString();
    }
}
