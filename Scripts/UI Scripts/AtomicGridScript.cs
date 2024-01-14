using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AtomicGridScript : MonoBehaviour
{
    //initially bind the function from generator script to this game object since the function belongs to a different prefab and cannot be bound directly
    /*using event trigger so that the function will be executed WITH the button gameobject information passed into that function. this is necessary
    since when toggling button, we need to change the button color.
    */
    void Start()
    {
        GameObject parentObject = gameObject.transform.parent.gameObject;
        LayerManager layerManager = parentObject.GetComponentInParent<LayerManager>();
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry pointerClickEntry = eventTrigger.triggers.Find(entry => entry.eventID == EventTriggerType.PointerClick);
        pointerClickEntry.callback.AddListener(layerManager.ToggleCell);
    }
}
