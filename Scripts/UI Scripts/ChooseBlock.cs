using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChooseBlock : MonoBehaviour
{
    public byte blockID;
    void Start()
    {
        ////initially bind the function from generator script to this game object since the function expects byte argumen, which cannot be passed through the inspector
        LayerManager layerManager = GetComponentInParent<LayerManager>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() => layerManager.ChangeBlock(blockID));
    }
}
