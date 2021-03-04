
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
 
public class EffectNumericalDisplayScript : MonoBehaviour {
    public enum NumberType {
        Damage,
        Healing
    }
 
    //　ダメージポイント表示用プレハブ
    [SerializeField]
    private GameObject damagePointText;
    //　回復ポイント表示用プレハブ
    [SerializeField]
    private GameObject healingPointText;
    //　ポイントの表示オフセット値
    [SerializeField]
    private Vector3 offset = new Vector3(0f, 0.8f, -0.5f);
 
    public void InstantiatePointText(NumberType numberType, Transform target, int point) {
        var rot = Quaternion.LookRotation(target.position - Camera.main.transform.position);
        if (numberType == NumberType.Damage) {
            var pointTextIns = Instantiate<GameObject>(damagePointText, target.position + offset, rot);
            pointTextIns.GetComponent<TextMeshPro>().text = point.ToString();
            Destroy(pointTextIns, 3f);
        } else if(numberType == NumberType.Healing) {
            var pointTextIns = Instantiate<GameObject>(healingPointText, target.position + offset, rot);
            pointTextIns.GetComponent<TextMeshPro>().text = point.ToString();
            Destroy(pointTextIns, 3f);
        }
    }
}