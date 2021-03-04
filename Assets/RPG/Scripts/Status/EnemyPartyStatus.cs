using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
[Serializable]
[CreateAssetMenu(fileName = "EnemyPartyStatus", menuName = "CreateEnemyPartyStatus")]
public class EnemyPartyStatus : ScriptableObject
{
    [SerializeField]
    private string partyName = null;
    [SerializeField]
    private List<GameObject> partyMembers = null;

    //オリジナル追加部分
    [SerializeField]
    private List<EnemyStatus> partyMembersStatus = null;
 
    
    public string GetPartyName() {
        return partyName;
    }
 
    public List<GameObject> GetEnemyGameObjectList() {
        return partyMembers;
    }
    //オリジナル追加部分
    public List<EnemyStatus> GetEnemyStatus(){
        return partyMembersStatus;
    }
}