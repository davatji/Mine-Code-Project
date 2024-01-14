using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddLayerScript : MonoBehaviour
{
    void Start()
    //initially bind the function from generator script to this game object since the function belongs to a different prefab and cannot be bound directly
    {
        GeneratorScript generatorScript = GetComponentInParent<GeneratorScript>();
        Button button = GetComponent<Button>();
        button.onClick.AddListener(generatorScript.AddEmptyLayer);
    }
}
