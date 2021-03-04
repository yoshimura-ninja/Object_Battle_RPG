using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Input;
//joystick用
using UnityStandardAssets.CrossPlatformInput;

public class EncountManager : MonoBehaviour
{
    //　LoadSceneManager
    private LoadSceneManager sceneManager;
    [SerializeField]
    private float encountMinTime = 3f;
    //　敵と遭遇するランダム時間
    [SerializeField]
    private float encountMaxTime = 30f;
    //　経過時間
    [SerializeField]
    private float elapsedTime;
    //　目的の時間
    [SerializeField]
    private float destinationTime;
    //　ユニティちゃん
    private Transform unityChanObjct;
    //　ユニティちゃんスクリプト
    private UnityChanScript unityChanScript;

    //　戦闘データ
    [SerializeField]
    private BattleData battleData = null;
    //　敵パーティーリスト
    [SerializeField]
    private EnemyPartyStatusList enemyPartyStatusList = null;
    //　ワールドマップフィールド
    [SerializeField]
    private Terrain worldMapField;
    
    [SerializeField]
    private SceneMovementData sceneMovementData = null;
 
    // Start is called before the first frame update
    void Start()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<LoadSceneManager>();
        SetDestinationTime();
        unityChanObjct = GameObject.FindWithTag("Player").transform;
        unityChanScript = unityChanObjct.GetComponent<UnityChanScript>();
    }
 
    // Update is called once per frame
    void Update()
    {   
        //mobile版joystick追加部分
        //　移動していない時は計測しない
        if (Mathf.Approximately(CrossPlatformInputManager.GetAxisRaw("Horizontal"), 0f)
            && Mathf.Approximately(CrossPlatformInputManager.GetAxisRaw("Vertical"), 0f)
            ) {
            return;
        }
        /*
        //joystick使わない版
        if (Mathf.Approximately(Input.GetAxis("Horizontal"), 0f)
            && Mathf.Approximately(Input.GetAxis("Vertical"), 0f)
            ) {
            return;
        }*/
        
        //　ユニティちゃんが何らかの行動をしていたら計測しない
        if(unityChanScript.GetState() == UnityChanScript.State.Talk
            || unityChanScript.GetState() == UnityChanScript.State.Command
            ) {
            return;
        }
        elapsedTime += Time.deltaTime;
        if(elapsedTime >= destinationTime) {
            Debug.Log("遭遇");
            
            //　ワールドマップ上のユニティちゃんの位置に応じて遭遇する敵を決定する
            if (-worldMapField.terrainData.size.x / 2 <= unityChanObjct.position.x && unityChanObjct.position.x <= 0f
                && 0f <= unityChanObjct.position.z && unityChanObjct.position.z <= worldMapField.terrainData.size.z / 2
                ) {
                    Debug.Log("1");
                battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup1"));
            } 
            else if (0f <= unityChanObjct.position.x && unityChanObjct.position.x <= worldMapField.terrainData.size.x / 2
                && 0 <= unityChanObjct.position.z && unityChanObjct.position.z <= worldMapField.terrainData.size.z / 2
                ) {
                    Debug.Log("2");
                battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup2"));
            } 
            else if (-worldMapField.terrainData.size.x / 2 <= unityChanObjct.position.x && unityChanObjct.position.x <= 0f
                && -worldMapField.terrainData.size.z / 2 <= unityChanObjct.position.z && unityChanObjct.position.z <= 0f
                ) {
                    Debug.Log("3");
                battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup3"));
            } 
            else if (0f <= unityChanObjct.position.x && unityChanObjct.position.x <= worldMapField.terrainData.size.x / 2
                && -worldMapField.terrainData.size.z / 2 <= unityChanObjct.position.z && unityChanObjct.position.z <= 0f
                ) {
                    Debug.Log("4");
                battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup4"));
            } 
            else {
                Debug.Log("なぞの場所");
                battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup1"));
            }
        
            //audioSource.Play();
            sceneMovementData.SetWorldMapPos(unityChanObjct.transform.position);
            sceneMovementData.SetWorldMapRot(unityChanObjct.transform.rotation);
            sceneManager.GoToNextScene(SceneMovementData.SceneType.WorldMapToBattle);
            elapsedTime = 0f;
            SetDestinationTime();
        }
    }
    //　次に敵と遭遇する時間
    public void SetDestinationTime() {
        destinationTime = Random.Range(encountMinTime, encountMaxTime);
    }
}