using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class InstantiateWeapon : MonoBehaviour
{
    [SerializeField]
    private Transform equip;

    // Start is called before the first frame update
    void Start()
    {
        var characterStatus = (AllyStatus)GetComponent<CharacterBattleScript>().GetCharacterStatus();
        if (characterStatus.GetEquipWeapon() != null) {
            if (characterStatus.GetEquipWeapon().GetItemObject() != null) {
                Instantiate<GameObject>(characterStatus.GetEquipWeapon().GetItemObject(), equip.position, equip.rotation, equip);
            } else {
                Debug.LogWarning("装備武器のゲームオブジェクトが設定されていません。");
            }
        }    
    }
}