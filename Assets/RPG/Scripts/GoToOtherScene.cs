﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
 
public class GoToOtherScene : MonoBehaviour
{
    private LoadSceneManager sceneManager;
    //　どのシーンへ遷移するか
    [SerializeField]
    private SceneMovementData.SceneType scene = SceneMovementData.SceneType.FirstVillage;
    //　シーン遷移中かどうか
    private bool isTransition;

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
 
    private void Awake() {
        sceneManager = FindObjectOfType<LoadSceneManager>();
    }
 
    private void OnTriggerEnter(Collider col) {
        //　次のシーンへ遷移途中でない時
        if(col.tag == "Player" && !isTransition) {
            isTransition = true;
            sceneManager.GoToNextScene(scene);
        }    
    }
    //　フェードをした後にシーン読み込み
    IEnumerator FadeAndLoadScene(SceneMovementData.SceneType scene) {
        //　フェードUIのインスタンス化
        fadeInstance = Instantiate<GameObject>(fadePrefab);
        fadeImage = fadeInstance.GetComponentInChildren<Image>();
        //　フェードアウト処理
        yield return StartCoroutine(Fade(1f));
 
        //　シーンの読み込み
        if (scene == SceneMovementData.SceneType.FirstVillage) {
            yield return StartCoroutine(LoadScene("Village"));
        }
        else if (scene == SceneMovementData.SceneType.FirstVillageToWorldMap) {
            yield return StartCoroutine(LoadScene("WorldMap"));
        }
        else if(scene == SceneMovementData.SceneType.WorldMapToBattle) {
        yield return StartCoroutine(LoadScene("Battle"));
        }
 
        //　フェードUIのインスタンス化
        fadeInstance = Instantiate<GameObject>(fadePrefab);
        fadeImage = fadeInstance.GetComponentInChildren<Image>();
        fadeImage.color = new Color(0f, 0f, 0f, 1f);
 
        //　フェードイン処理
        yield return StartCoroutine(Fade(0f));
 
        Destroy(fadeInstance);

        isTransition = false;
    }

    //下記はオリジナル追記部分
    IEnumerator Fade(float alpha) {
        var fadeImageAlpha = fadeImage.color.a;
 
        while (Mathf.Abs(fadeImageAlpha - alpha) > 0.01f) {
            fadeImageAlpha = Mathf.Lerp(fadeImageAlpha, alpha, fadeSpeed * Time.deltaTime);
            fadeImage.color = new Color(0f, 0f, 0f, fadeImageAlpha);
            yield return null;
        }
    }
    //　実際にシーンを読み込む処理
    IEnumerator LoadScene(string sceneName) {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
 
        while (!async.isDone) {
            yield return null;
        }
    }
}   