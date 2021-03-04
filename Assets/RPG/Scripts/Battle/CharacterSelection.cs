using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class CharacterSelection : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerDownHandler {
    private GameObject characterMarker;
 
    private void Start() {
        characterMarker = GameObject.Find("Characters" + transform.Find("Text").GetComponent<Text>().text).transform.Find("Marker/Image").gameObject;
        if(EventSystem.current.currentSelectedGameObject == this.gameObject) {
            characterMarker.SetActive(true);
        }
    }
 
    private void OnDestroy() {
        //　characterMarkerがnullでなければマーカーを非表示
        if (characterMarker != null) {
            characterMarker.SetActive(false);
        }
    }
 
    public void OnSelect(BaseEventData eventData) {
        characterMarker.SetActive(true);
    }
 
    public void OnDeselect(BaseEventData eventData) {
        if (characterMarker != null) {
            characterMarker.SetActive(false);
        }
    }
 
    public void OnSubmit(BaseEventData eventData) {
        characterMarker.SetActive(false);
    }
 
    public void OnPointerDown(PointerEventData eventData) {
        characterMarker.SetActive(false);
    }
}