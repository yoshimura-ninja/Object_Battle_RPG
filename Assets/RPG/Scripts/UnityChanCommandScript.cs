
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Input;

public class UnityChanCommandScript : MonoBehaviour
{
    private LoadSceneManager sceneManager;
    //　コマンド用UI
    [SerializeField]
    private GameObject commandUI = null;
    private UnityChanScript unityChanScript;

    // Start is called before the first frame update
    void Start()
    {
        sceneManager = GameObject.Find("SceneManager").GetComponent<LoadSceneManager>();
        unityChanScript = GetComponent<UnityChanScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(sceneManager.IsTransition()
            || unityChanScript.GetState() == UnityChanScript.State.Talk
            ) {
            return;
        }
        //　コマンドUIの表示・非表示の切り替え
        if(Input.GetButtonDown("Menu")) {
            //　コマンド
            if (!commandUI.activeSelf) {
                //　ユニティちゃんをコマンド状態にする
                unityChanScript.SetState(UnityChanScript.State.Command);
            } else {
                ExitCommand();
            }
            //　コマンドUIのオン・オフ
            commandUI.SetActive(!commandUI.activeSelf);
        }
    }
    //　CommandScriptから呼び出すコマンド画面の終了
    public void ExitCommand() {
        EventSystem.current.SetSelectedGameObject(null);
        unityChanScript.SetState(UnityChanScript.State.Normal);
    }
}
