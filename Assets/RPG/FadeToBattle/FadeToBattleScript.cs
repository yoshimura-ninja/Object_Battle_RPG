using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class FadeToBattleScript : MonoBehaviour {
 
    [SerializeField]
    private Material material;
    //　フェードAmountの到達値
    private float destinationAmount;
 
    private void Start() {
        //　スタート時に初期化
        material.SetFloat("_Amount", 0f);
    }
    //　カメラに取り付けると呼ばれる
    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, material);
    }
}