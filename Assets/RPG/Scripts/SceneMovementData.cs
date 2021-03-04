using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
 
[Serializable]
[CreateAssetMenu(fileName = "SceneMovementData", menuName = "CreateSceneMovementData")]
public class SceneMovementData : ScriptableObject
{
    //オリジナル追記部分ここから

    //　シーン移動に関するデータファイル
    [SerializeField]
    private SceneMovementData sceneMovementData = null;
    //　フェードプレハブ
    [SerializeField]
    private GameObject fadePrefab = null;
    //　フェードインスタンス
    private GameObject fadeInstance;
    //　フェードの画像
    private Image fadeImage;
    [SerializeField]
    private float fadeSpeed = 5f;
    //ここまで
 
    public enum SceneType {
        StartGame,
        FirstVillage,
        FirstVillageToWorldMap,
        WorldMapToBattle,
        BattleToWorldMap
    }
 
    [SerializeField]
    private SceneType sceneType;

    //　ワールドマップ→戦闘シーンへ移行した時のワールドマップの位置情報
    private Vector3 worldMapPos;
    //　ワールドマップ→戦闘シーンへ移行した時のワールドマップの位置情報
    private Quaternion worldMapRot;
 
    public void OnEnable() {
        sceneType = SceneType.StartGame;
    }
 
    public void SetSceneType(SceneType scene) {
        sceneType = scene;
    }
 
    public SceneType GetSceneType() {
        return sceneType;
    }
    public void SetWorldMapPos(Vector3 pos) {
        worldMapPos = pos;
    }
    
    public Vector3 GetWorldMapPos() {
        return worldMapPos;
    }
    
    public void SetWorldMapRot(Quaternion rot) {
        worldMapRot = rot;
    }
    
    public Quaternion GetWorldMapRot() {
        return worldMapRot;
    }
}
 