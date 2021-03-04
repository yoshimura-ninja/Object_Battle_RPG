﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class BattleManager : MonoBehaviour {
 
    public enum CommandMode {
        SelectCommand,
        SelectDirectAttacker,
        SelectMagic,
        SelectMagicAttackTarget,
        SelectUseMagicOnAlliesTarget,
        SelectItem,
        SelectRecoveryItemTarget
    }
    //　味方パーティーのコマンドパネル
    [SerializeField]
    private Transform commandPanel = null;
    //　戦闘用キャラクター選択ボタンプレハブ
    [SerializeField]
    private GameObject battleCharacterButton = null;
    //　SelectCharacterPanel
    [SerializeField]
    private Transform selectCharacterPanel = null;
    //　魔法やアイテム選択パネル
    [SerializeField]
    private Transform magicOrItemPanel = null;
    //　魔法やアイテム選択パネルのContent
    private Transform magicOrItemPanelContent = null;
    //　BattleItemPanelButtonプレハブ
    [SerializeField]
    private GameObject battleItemPanelButton = null;
    //　BattleMagicPanelButtonプレハブ
    [SerializeField]
    private GameObject battleMagicPanelButton = null;
    //　最後に選択していたゲームオブジェクトをスタック
    private Stack<GameObject> selectedGameObjectStack = new Stack<GameObject>();
    
    //　MagicOrItemPanelでどの番号のボタンから上にスクロールするか
    [SerializeField]
    private int scrollDownButtonNum = 8;
    //　MagicOrItemPanelでどの番号のボタンから下にスクロールするか
    [SerializeField]
    private int scrollUpButtonNum = 10;
    
    //　ScrollManager
    private ScrollManager scrollManager;
    //ここまで

    //　メッセージパネルプレハブ
    [SerializeField]
    private GameObject messagePanel;
    //　BattleUI
    [SerializeField]
    private Transform battleUI;
    //　メッセージパネルインスタンス
    private GameObject messagePanelIns;
    
    //　戦闘データ
    [SerializeField]
    private BattleData battleData = null;
    //　キャラクターのベース位置
    [SerializeField]
    private Transform battleBasePosition;

    //　現在戦闘に参加しているキャラクター
    private List<GameObject> allCharacterList = new List<GameObject>();
 
    //　現在戦闘に参加している全キャラクター
    private List<GameObject> allCharacterInBattleList = new List<GameObject>();
    //　現在戦闘に参加している味方キャラクター
    private List<GameObject> allyCharacterInBattleList = new List<GameObject>();
    //　現在戦闘に参加している敵キャラクター
    private List<GameObject> enemyCharacterInBattleList = new List<GameObject>();
    //　現在の攻撃の順番
    private int currentAttackOrder;
    //　現在攻撃をしようとしている人が選択中
    private bool isChoosing;
    //　戦闘が開始しているかどうか
    private bool isStartBattle;
    //　戦闘シーンの最初の攻撃が始まるまでの待機時間
    [SerializeField]
    private float firstWaitingTime = 3f;
    //　戦闘シーンのキャラ移行時の間の時間
    [SerializeField]
    private float timeToNextCharacter = 1f;
    //　待ち時間
    private float waitTime;
    //　戦闘シーンの最初の攻撃が始まるまでの経過時間
    private float elapsedTime;
    //　戦闘が終了したかどうか
    private bool battleIsOver;
    //　現在のコマンド
    private CommandMode currentCommand;

    //　結果表示処理スクリプト
    [SerializeField]
    private BattleResult battleResult;

    //　デバッグ用敵パーティーリスト
    [SerializeField]
    private EnemyPartyStatusList enemyPartyStatusList = null;

    // Start is called before the first frame update
    void Start() {
        //　キャラクターインスタンスの親
        Transform charactersParent = new GameObject("Characters").transform;
        //　キャラクターを配置するTransform
        Transform characterTransform;
        //　同じ名前の敵がいた場合の処理に使うリスト
        List<string> enemyNameList = new List<string>();
    
        GameObject ins;
        CharacterBattleScript characterBattleScript;
        string characterName;

        ShowMessage("戦闘開始");

        //スクロール追記部分
        magicOrItemPanelContent = magicOrItemPanel.Find("Mask/Content");
        scrollManager = magicOrItemPanelContent.GetComponent<ScrollManager>();
    
        //　味方パーティーのプレハブをインスタンス化
        for (int i = 0; i < battleData.GetAllyPartyStatus().GetAllyGameObject().Count; i++) {
            characterTransform = battleBasePosition.Find("AllyPos" + i).transform;
            ins = Instantiate<GameObject>(battleData.GetAllyPartyStatus().GetAllyGameObject()[i], characterTransform.position, characterTransform.rotation, charactersParent);
            characterBattleScript = ins.GetComponent<CharacterBattleScript>();
            ins.name = characterBattleScript.GetCharacterStatus().GetCharacterName();
            if (characterBattleScript.GetCharacterStatus().GetHp() > 0) {
                allyCharacterInBattleList.Add(ins);
                allCharacterList.Add(ins);
            }
        }
        if (battleData.GetEnemyPartyStatus() == null) {
            Debug.LogError("敵パーティーデータが設定されていません。");
            battleData.SetEnemyPartyStatus(enemyPartyStatusList.GetPartyMembersList().Find(enemyPartyStatus => enemyPartyStatus.GetPartyName() == "EnemyGroup1"));
        }
        //　敵パーティーのプレハブをインスタンス化
        for (int i = 0; i < battleData.GetEnemyPartyStatus().GetEnemyGameObjectList().Count; i++) {
            Debug.Log(battleData.GetEnemyPartyStatus());
            characterTransform = battleBasePosition.Find("EnemyPos" + i).transform;
            ins = Instantiate<GameObject>(battleData.GetEnemyPartyStatus().GetEnemyGameObjectList()[i], characterTransform.position, characterTransform.rotation, charactersParent);
            //　既に同じ敵が存在したら文字を付加する
            characterName = ins.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetCharacterName();
            if (!enemyNameList.Contains(characterName)) {
                ins.name = characterName + 'A';
            } else {
                ins.name = characterName + (char)('A' + enemyNameList.Count(enemyName => enemyName == characterName));
            }
            enemyNameList.Add(characterName);
            enemyCharacterInBattleList.Add(ins);
            allCharacterList.Add(ins);
        }
        //　キャラクターリストをキャラクターの素早さの高い順に並べ替え
        allCharacterList = allCharacterList.OrderByDescending(character => character.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetAgility()).ToList<GameObject>();
        //　現在の戦闘
        allCharacterInBattleList = allCharacterList.ToList<GameObject>();
        //　確認の為並べ替えたリストを表示
        foreach (var character in allCharacterInBattleList) {
            Debug.Log(character.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetCharacterName() + " : " + character.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetAgility());
        }
        //　戦闘前の待ち時間を設定
        waitTime = firstWaitingTime;
        //　ランダム値のシードの設定
        Random.InitState((int)Time.time);
    }
    // Update is called once per frame
    void Update() {
    
        //　戦闘が終了していたらこれ以降何もしない
        if (battleIsOver) {
            return;
        }
        //スクロール追記部分
        //　選択解除された時（マウスでUI外をクリックした）は現在のモードによって無理やり選択させる
        if (EventSystem.current.currentSelectedGameObject == null) {
            if (currentCommand == CommandMode.SelectCommand) {
                EventSystem.current.SetSelectedGameObject(commandPanel.GetChild(1).gameObject);
            } else if (currentCommand == CommandMode.SelectDirectAttacker) {
                EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
            } else if (currentCommand == CommandMode.SelectMagic) {
                scrollManager.Reset();
                EventSystem.current.SetSelectedGameObject(magicOrItemPanelContent.GetChild(0).gameObject);
            } else if (currentCommand == CommandMode.SelectMagicAttackTarget) {
                EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
            } else if (currentCommand == CommandMode.SelectUseMagicOnAlliesTarget) {
                EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
            } else if (currentCommand == CommandMode.SelectItem) {
                scrollManager.Reset();
                EventSystem.current.SetSelectedGameObject(magicOrItemPanelContent.GetChild(0).gameObject);
            } else if (currentCommand == CommandMode.SelectRecoveryItemTarget) {
                EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
            }
        }
    
        //　戦闘開始
        if (isStartBattle) {
            //　現在のキャラクターの攻撃が終わっている
            if (!isChoosing) {
                elapsedTime += Time.deltaTime;
                if (elapsedTime < waitTime) {
                    return;
                }
                elapsedTime = 0f;
                isChoosing = true;
    
                //　キャラクターの攻撃の選択に移る
                MakeAttackChoise(allCharacterInBattleList[currentAttackOrder]);
                //　次のキャラクターのターンにする
                currentAttackOrder++;
                //　全員攻撃が終わったら最初から
                if (currentAttackOrder >= allCharacterInBattleList.Count) {
                    currentAttackOrder = 0;
                }
                } else {
                //　キャンセルボタンを押した時の処理
                //if (Input.GetButtonDown("Cancel")) {
                if (UnityStandardAssets.CrossPlatformInput.CrossPlatformInputManager.GetButtonDown("Cancel")){
                    if (currentCommand == CommandMode.SelectDirectAttacker) {
                        // キャラクター選択ボタンがあれば全て削除
                        for (int i = selectCharacterPanel.transform.childCount - 1; i >= 0; i--) {
                            Destroy(selectCharacterPanel.transform.GetChild(i).gameObject);
                        }
        
                        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = false;
                        selectCharacterPanel.gameObject.SetActive(false);
                        commandPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectCommand;
                    } else if (currentCommand == CommandMode.SelectMagic) {
                        // magicOrItemPanelにボタンがあれば全て削除
                        for (int i = magicOrItemPanelContent.transform.childCount - 1; i >= 0; i--) {
                            Destroy(magicOrItemPanelContent.transform.GetChild(i).gameObject);
                        }
                        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = false;
                        magicOrItemPanel.gameObject.SetActive(false);
                        commandPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectCommand;
                    } else if (currentCommand == CommandMode.SelectMagicAttackTarget) {
                        // selectCharacterPanelにボタンがあれば全て削除
                        for (int i = selectCharacterPanel.transform.childCount - 1; i >= 0; i--) {
                            Destroy(selectCharacterPanel.transform.GetChild(i).gameObject);
                        }
                        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = false;
                        selectCharacterPanel.gameObject.SetActive(false);
                        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectMagic;
                    } else if (currentCommand == CommandMode.SelectUseMagicOnAlliesTarget) {
                        // selectCharacterPanelにボタンがあれば全て削除
                        for (int i = selectCharacterPanel.transform.childCount - 1; i >= 0; i--) {
                            Destroy(selectCharacterPanel.transform.GetChild(i).gameObject);
                        }
                        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = false;
                        selectCharacterPanel.gameObject.SetActive(false);
                        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectMagic;
                    } else if (currentCommand == CommandMode.SelectItem) {
                        // magicOrItemPanelにボタンがあれば全て削除
                        for (int i = magicOrItemPanelContent.transform.childCount - 1; i >= 0; i--) {
                            Destroy(magicOrItemPanelContent.transform.GetChild(i).gameObject);
                        }
                        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = false;
                        magicOrItemPanel.gameObject.SetActive(false);
                        commandPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectCommand;
                    } else if (currentCommand == CommandMode.SelectRecoveryItemTarget) {
                        // selectCharacterPanelにボタンがあれば全て削除
                        for (int i = selectCharacterPanel.transform.childCount - 1; i >= 0; i--) {
                            Destroy(selectCharacterPanel.transform.GetChild(i).gameObject);
                        }
                        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = false;
                        selectCharacterPanel.gameObject.SetActive(false);
                        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = true;
                        EventSystem.current.SetSelectedGameObject(selectedGameObjectStack.Pop());
                        currentCommand = CommandMode.SelectItem;
                    }
                }
            }
        } else {
            //Debug.Log("経過時間： " + elapsedTime);
            //　戦闘前の待機
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime) {
                //　2回目以降はキャラ間の時間を設定
                waitTime = timeToNextCharacter;
                //　最初のキャラクターの待ち時間は0にする為にあらかじめ条件をクリアさせておく
                elapsedTime = timeToNextCharacter;
                isStartBattle = true;
            }
        }
    }
    //　キャラクターの攻撃の選択処理
    public void MakeAttackChoise(GameObject character) {
        CharacterStatus characterStatus = character.GetComponent<CharacterBattleScript>().GetCharacterStatus();
        //　EnemyStatusにキャスト出来る場合は敵の攻撃処理
        if (characterStatus as EnemyStatus != null) {
            //Debug.Log(character.gameObject.name + "のターンです");
            EnemyAttack(character);
        } else {
            //Debug.Log(characterStatus.GetCharacterName() + "のターンです");
            AllyAttack(character);
        }
    }
    //　味方の攻撃処理
    public void AllyAttack(GameObject character) {

        character.transform.Find("Marker/Image2").gameObject.SetActive(true);
    
        currentCommand = CommandMode.SelectCommand;
    
        // キャラクター選択ボタンがあれば全て削除
        for (int i = selectCharacterPanel.transform.childCount - 1; i >= 0; i--) {
            Destroy(selectCharacterPanel.transform.GetChild(i).gameObject);
        }
    
        // 魔法やアイテムパネルの子要素のContentにボタンがあれば全て削除
        for (int i = magicOrItemPanelContent.transform.childCount - 1; i >= 0; i--) {
            Destroy(magicOrItemPanelContent.transform.GetChild(i).gameObject);
        }
    
        commandPanel.GetComponent<CanvasGroup>().interactable = true;
        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = false;
        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = false;
    
        //　キャラクターがガード状態であればガードを解く
        if (character.GetComponent<Animator>().GetBool("Guard")) {
            character.GetComponent<CharacterBattleScript>().UnlockGuard();
        }
    
        //　キャラクターの名前を表示
        commandPanel.Find("CharacterName/Text").GetComponent<Text>().text = character.gameObject.name;
    
        var characterSkill = character.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetSkillList();
        //　持っているスキルに応じてコマンドボタンの表示
        if (characterSkill.Exists(skill => skill.GetSkillType() == Skill.Type.DirectAttack)) {
            var directAttackButtonObj = commandPanel.Find("DirectAttack").gameObject;
            var directAttackButton = directAttackButtonObj.GetComponent<Button>();
            directAttackButton.onClick.RemoveAllListeners();
            directAttackButtonObj.GetComponent<Button>().onClick.AddListener(() => SelectDirectAttacker(character));
            directAttackButtonObj.SetActive(true);
        } else {
            commandPanel.Find("DirectAttack").gameObject.SetActive(false);
        }
        if (characterSkill.Exists(skill => skill.GetSkillType() == Skill.Type.Guard)) {
            var guardButtonObj = commandPanel.Find("Guard").gameObject;
            var guardButton = guardButtonObj.GetComponent<Button>();
            guardButton.onClick.RemoveAllListeners();
            guardButton.onClick.AddListener(() => Guard(character));
            guardButtonObj.SetActive(true);
        } else {
            commandPanel.Find("Guard").gameObject.SetActive(false);
        }
        if (characterSkill.Exists(skill => skill.GetSkillType() == Skill.Type.Item)) {
            var itemButtonObj = commandPanel.Find("Item").gameObject;
            var itemButton = itemButtonObj.GetComponent<Button>();
            itemButton.onClick.RemoveAllListeners();
            itemButton.onClick.AddListener(() => SelectItem(character));
            commandPanel.Find("Item").gameObject.SetActive(true);
        } else {
            commandPanel.Find("Item").gameObject.SetActive(false);
        }
        if (characterSkill.Exists(skill => skill.GetSkillType() == Skill.Type.MagicAttack)
            || characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.IncreaseAttackPowerMagic)
            || characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.IncreaseDefencePowerMagic)
            || characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.NumbnessRecoveryMagic)
            || characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.PoisonnouRecoveryMagic)
            || characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.RecoveryMagic)) {
    
            var magicButtonObj = commandPanel.Find("Magic").gameObject;
            var magicButton = magicButtonObj.GetComponent<Button>();
            magicButton.onClick.RemoveAllListeners();
            magicButton.onClick.AddListener(() => SelectMagic(character));
    
            magicButtonObj.SetActive(true);
        } else {
            commandPanel.Find("Magic").gameObject.SetActive(false);
        }
        if (characterSkill.Exists(skill => skill.GetSkillType() == Skill.Type.GetAway)) {
            var getAwayButtonObj = commandPanel.Find("GetAway").gameObject;
            var getAwayButton = getAwayButtonObj.GetComponent<Button>();
            getAwayButton.onClick.RemoveAllListeners();
            getAwayButton.onClick.AddListener(() => GetAway(character));
            getAwayButtonObj.SetActive(true);
        } else {
            commandPanel.Find("GetAway").gameObject.SetActive(false);
        }
    
        EventSystem.current.SetSelectedGameObject(commandPanel.transform.GetChild(1).gameObject);
        commandPanel.gameObject.SetActive(true);
    }

    //　キャラクター選択
    public void SelectDirectAttacker(GameObject attackCharacter) {
        currentCommand = CommandMode.SelectDirectAttacker;
        commandPanel.GetComponent<CanvasGroup>().interactable = false;
        selectedGameObjectStack.Push(EventSystem.current.currentSelectedGameObject);
    
        GameObject battleCharacterButtonIns;
    
        foreach (var enemy in enemyCharacterInBattleList) {
            battleCharacterButtonIns = Instantiate<GameObject>(battleCharacterButton, selectCharacterPanel);
            battleCharacterButtonIns.transform.Find("Text").GetComponent<Text>().text = enemy.gameObject.name;
            battleCharacterButtonIns.GetComponent<Button>().onClick.AddListener(() => DirectAttack(attackCharacter, enemy));
        }
    
        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = true;
        EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
        selectCharacterPanel.gameObject.SetActive(true);

        Debug.Log("SelectDirectAttackerメソッド終了");
    }
    //　直接攻撃
    public void DirectAttack(GameObject attackCharacter, GameObject attackTarget) {
        //　攻撃するキャラのDirectAttackスキルを取得する
        var characterSkill = attackCharacter.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetSkillList();
        Skill directAtatck = characterSkill.Find(skill => skill.GetSkillType() == Skill.Type.DirectAttack);
        attackCharacter.GetComponent<CharacterBattleScript>().ChooseAttackOptions(CharacterBattleScript.BattleState.DirectAttack, attackTarget, directAtatck);
        commandPanel.gameObject.SetActive(false);
        selectCharacterPanel.gameObject.SetActive(false);
        Debug.Log("DirectAttackメソッド終了");
    }
    //　防御
    public void Guard(GameObject guardCharacter) {
        guardCharacter.GetComponent<CharacterBattleScript>().Guard();
        commandPanel.gameObject.SetActive(false);
        ChangeNextChara();
    }
    //　使用する魔法の選択
    public void SelectMagic(GameObject character) {
        currentCommand = CommandMode.SelectMagic;
        commandPanel.GetComponent<CanvasGroup>().interactable = false;
        selectedGameObjectStack.Push(EventSystem.current.currentSelectedGameObject);
    
        GameObject battleMagicPanelButtonIns;
        var skillList = character.GetComponent<CharacterBattleScript>().GetCharacterStatus().GetSkillList();
    
        //　MagicOrItemPanelのスクロール値の初期化
        scrollManager.Reset();
        int battleMagicPanelButtonNum = 0;
    
        foreach (var skill in skillList) {
            if (skill.GetSkillType() == Skill.Type.MagicAttack
                || skill.GetSkillType() == Skill.Type.RecoveryMagic
                || skill.GetSkillType() == Skill.Type.IncreaseAttackPowerMagic
                || skill.GetSkillType() == Skill.Type.IncreaseDefencePowerMagic
                || skill.GetSkillType() == Skill.Type.NumbnessRecoveryMagic
                || skill.GetSkillType() == Skill.Type.PoisonnouRecoveryMagic
                ) {
                battleMagicPanelButtonIns = Instantiate<GameObject>(battleMagicPanelButton, magicOrItemPanelContent);
                battleMagicPanelButtonIns.transform.Find("MagicName").GetComponent<Text>().text = skill.GetKanjiName();
                battleMagicPanelButtonIns.transform.Find("AmountToUseMagicPoints").GetComponent<Text>().text = ((Magic)skill).GetAmountToUseMagicPoints().ToString();
    
                //　指定した番号のアイテムパネルボタンにアイテムスクロール用スクリプトを取り付ける
                if (battleMagicPanelButtonNum != 0
                    && (battleMagicPanelButtonNum % scrollDownButtonNum == 0
                    || battleMagicPanelButtonNum % (scrollDownButtonNum + 1) == 0)
                    ) {
                    //　アイテムスクロールスクリプトの取り付けて設定値のセット
                    battleMagicPanelButtonIns.AddComponent<ScrollDownScript>();
                } else if(battleMagicPanelButtonNum != 0
                    && (battleMagicPanelButtonNum % scrollUpButtonNum == 0
                    || battleMagicPanelButtonNum % (scrollUpButtonNum + 1) == 0)
                    ) {
                    battleMagicPanelButtonIns.AddComponent<ScrollUpScript>();
                }
    
                //　MPが足りない時はボタンを押しても何もせず魔法の名前を暗くする
                if (character.GetComponent<CharacterBattleScript>().GetMp() < ((Magic)skill).GetAmountToUseMagicPoints()) {
                    battleMagicPanelButtonIns.transform.Find("MagicName").GetComponent<Text>().color = new Color(0.4f, 0.4f, 0.4f);
                } else {
                    battleMagicPanelButtonIns.GetComponent<Button>().onClick.AddListener(() => SelectUseMagicTarget(character, skill));
                }
                //　ボタン番号を足す
                battleMagicPanelButtonNum++;
    
                if (battleMagicPanelButtonNum == scrollUpButtonNum + 2) {
                    battleMagicPanelButtonNum = 2;
                }
            }
        }    
        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = true;
        EventSystem.current.SetSelectedGameObject(magicOrItemPanelContent.GetChild(0).gameObject);
        magicOrItemPanel.gameObject.SetActive(true);  
    }
    //　魔法を使う相手の選択
    public void SelectUseMagicTarget(GameObject user, Skill skill) {
        currentCommand = CommandMode.SelectUseMagicOnAlliesTarget;
        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = false;
        selectedGameObjectStack.Push(EventSystem.current.currentSelectedGameObject);
    
        GameObject battleCharacterButtonIns;
    
        if (skill.GetSkillType() == Skill.Type.MagicAttack) {
            foreach (var enemy in enemyCharacterInBattleList) {
                battleCharacterButtonIns = Instantiate<GameObject>(battleCharacterButton, selectCharacterPanel);
                battleCharacterButtonIns.transform.Find("Text").GetComponent<Text>().text = enemy.gameObject.name;
                battleCharacterButtonIns.GetComponent<Button>().onClick.AddListener(() => UseMagic(user, enemy, skill));
            }
        } else {
            foreach (var allyCharacter in allyCharacterInBattleList) {
                battleCharacterButtonIns = Instantiate<GameObject>(battleCharacterButton, selectCharacterPanel);
                battleCharacterButtonIns.transform.Find("Text").GetComponent<Text>().text = allyCharacter.gameObject.name;
                battleCharacterButtonIns.GetComponent<Button>().onClick.AddListener(() => UseMagic(user, allyCharacter, skill));
            }
        }
    
        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = true;
        EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
        selectCharacterPanel.gameObject.SetActive(true);
    }
    //　魔法を使う
    public void UseMagic(GameObject user, GameObject targetCharacter, Skill skill) {
        CharacterBattleScript.BattleState battleState = CharacterBattleScript.BattleState.Idle;
        //　魔法を使う相手のCharacterBattleScriptを取得しておく
        var targetCharacterBattleScript = targetCharacter.GetComponent<CharacterBattleScript>();
    
        //　使う魔法の種類の設定と対象に使う必要がない場合の処理
        if (skill.GetSkillType() == Skill.Type.MagicAttack) {
            battleState = CharacterBattleScript.BattleState.MagicAttack;
        } else if (skill.GetSkillType() == Skill.Type.RecoveryMagic) {
            if (targetCharacterBattleScript.GetHp() == targetCharacterBattleScript.GetCharacterStatus().GetMaxHp()) {
                ShowMessage(targetCharacter.name + "は全快です。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.Healing;
        } else if (skill.GetSkillType() == Skill.Type.IncreaseAttackPowerMagic) {
            if (targetCharacterBattleScript.IsIncreasePower()) {
                ShowMessage("既に攻撃力を上げています。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.IncreaseAttackPowerMagic;
        } else if (skill.GetSkillType() == Skill.Type.IncreaseDefencePowerMagic) {
            if (targetCharacterBattleScript.IsIncreaseStrikingStrength()) {
                ShowMessage("既に防御力を上げています。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.IncreaseDefencePowerMagic;
        } else if (skill.GetSkillType() == Skill.Type.NumbnessRecoveryMagic) {
            if (!targetCharacterBattleScript.IsNumbness()) {
                ShowMessage(targetCharacter.name + "は痺れ状態ではありません。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.NumbnessRecoveryMagic;
        } else if (skill.GetSkillType() == Skill.Type.PoisonnouRecoveryMagic) {
            if (!targetCharacterBattleScript.IsPoison()) {
                ShowMessage(targetCharacter.name + "は毒状態ではありません。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.PoisonnouRecoveryMagic;
        }
        user.GetComponent<CharacterBattleScript>().ChooseAttackOptions(battleState, targetCharacter, skill);
        commandPanel.gameObject.SetActive(false);
        magicOrItemPanel.gameObject.SetActive(false);
        selectCharacterPanel.gameObject.SetActive(false);
    }
    //　使用するアイテムの選択
    public void SelectItem(GameObject character) {
    
        var itemDictionary = ((AllyStatus)character.GetComponent<CharacterBattleScript>().GetCharacterStatus()).GetItemDictionary();
    
        //　MagicOrItemPanelのスクロール値の初期化
        scrollManager.Reset();
        var battleItemPanelButtonNum = 0;
    
        GameObject battleItemPanelButtonIns;
    
        foreach (var item in itemDictionary.Keys) {
            if (item.GetItemType() == Item.Type.HPRecovery
                || item.GetItemType() == Item.Type.MPRecovery
                || item.GetItemType() == Item.Type.NumbnessRecovery
                || item.GetItemType() == Item.Type.PoisonRecovery
                ) {
                battleItemPanelButtonIns = Instantiate<GameObject>(battleItemPanelButton, magicOrItemPanelContent);
                battleItemPanelButtonIns.transform.Find("ItemName").GetComponent<Text>().text = item.GetKanjiName();
                battleItemPanelButtonIns.transform.Find("Num").GetComponent<Text>().text = ((AllyStatus)character.GetComponent<CharacterBattleScript>().GetCharacterStatus()).GetItemNum(item).ToString();
                battleItemPanelButtonIns.GetComponent<Button>().onClick.AddListener(() => SelectItemTarget(character, item));
    
                //　指定した番号のアイテムパネルボタンにアイテムスクロール用スクリプトを取り付ける
                if (battleItemPanelButtonNum != 0
                    && (battleItemPanelButtonNum % scrollDownButtonNum == 0
                    || battleItemPanelButtonNum % (scrollDownButtonNum + 1) == 0)
                    ) {
                    //　アイテムスクロールスクリプトの取り付けて設定値のセット
                    battleItemPanelButtonIns.AddComponent<ScrollDownScript>();
                } else if (battleItemPanelButtonNum != 0
                    && (battleItemPanelButtonNum % scrollUpButtonNum == 0
                    || battleItemPanelButtonNum % (scrollUpButtonNum + 1) == 0)
                    ) {
                    battleItemPanelButtonIns.AddComponent<ScrollUpScript>();
                }
                //　ボタン番号を足す
                battleItemPanelButtonNum++;
    
                if (battleItemPanelButtonNum == scrollUpButtonNum + 2) {
                    battleItemPanelButtonNum = 2;
                }
            }
        }
    
        if (magicOrItemPanelContent.childCount > 0) {
            currentCommand = CommandMode.SelectItem;
            commandPanel.GetComponent<CanvasGroup>().interactable = false;
            selectedGameObjectStack.Push(EventSystem.current.currentSelectedGameObject);
    
            magicOrItemPanel.GetComponent<CanvasGroup>().interactable = true;
            EventSystem.current.SetSelectedGameObject(magicOrItemPanelContent.GetChild(0).gameObject);
            magicOrItemPanel.gameObject.SetActive(true);
        } else {
            ShowMessage("使えるアイテムがありません。");
        }
    }
    //　アイテムを使用する相手を選択
    public void SelectItemTarget(GameObject user, Item item) {
        currentCommand = CommandMode.SelectRecoveryItemTarget;
        magicOrItemPanel.GetComponent<CanvasGroup>().interactable = false;
        selectedGameObjectStack.Push(EventSystem.current.currentSelectedGameObject);
    
        GameObject battleCharacterButtonIns;
    
        foreach (var allyCharacter in allyCharacterInBattleList) {
            battleCharacterButtonIns = Instantiate<GameObject>(battleCharacterButton, selectCharacterPanel);
            battleCharacterButtonIns.transform.Find("Text").GetComponent<Text>().text = allyCharacter.gameObject.name;
            battleCharacterButtonIns.GetComponent<Button>().onClick.AddListener(() => UseItem(user, allyCharacter, item));
        }
    
        selectCharacterPanel.GetComponent<CanvasGroup>().interactable = true;
        EventSystem.current.SetSelectedGameObject(selectCharacterPanel.GetChild(0).gameObject);
        selectCharacterPanel.gameObject.SetActive(true);
    }
    //　アイテム使用
    public void UseItem(GameObject user, GameObject targetCharacter, Item item) {
        var userCharacterBattleScript = user.GetComponent<CharacterBattleScript>();
        var targetCharacterBattleScript = targetCharacter.GetComponent<CharacterBattleScript>();
        var skill = userCharacterBattleScript.GetCharacterStatus().GetSkillList().Find(skills => skills.GetSkillType() == Skill.Type.Item);
    
        CharacterBattleScript.BattleState battleState = CharacterBattleScript.BattleState.Idle;
    
        if (item.GetItemType() == Item.Type.HPRecovery) {
            if (targetCharacterBattleScript.GetHp() == targetCharacterBattleScript.GetCharacterStatus().GetMaxHp()) {
                ShowMessage(targetCharacter.name + "は全快です。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.UseHPRecoveryItem;
        } else if (item.GetItemType() == Item.Type.MPRecovery) {
            if (targetCharacterBattleScript.GetMp() == targetCharacterBattleScript.GetCharacterStatus().GetMaxMp()) {
                ShowMessage(targetCharacter.name + "はMP回復をする必要がありません。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.UseMPRecoveryItem;
        } else if (item.GetItemType() == Item.Type.NumbnessRecovery) {
            if (!targetCharacterBattleScript.IsNumbness()) {
                ShowMessage(targetCharacter.name + "は痺れ状態ではありません。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.UseNumbnessRecoveryItem;
        } else if (item.GetItemType() == Item.Type.PoisonRecovery) {
            if (!targetCharacterBattleScript.IsPoison()) {
                ShowMessage(targetCharacter.name + "は毒状態ではありません。");
                return;
            }
            battleState = CharacterBattleScript.BattleState.UsePoisonRecoveryItem;
        }
        userCharacterBattleScript.ChooseAttackOptions(battleState, targetCharacter, skill, item);
        commandPanel.gameObject.SetActive(false);
        magicOrItemPanel.gameObject.SetActive(false);
        selectCharacterPanel.gameObject.SetActive(false);
    }
    //　逃げる
    public void GetAway(GameObject character) {
        character.transform.Find("Marker/Image2").gameObject.SetActive(false);
    
        var randomValue = Random.value;
        if (0f <= randomValue && randomValue <= 0.8f) {
            //Debug.Log("逃げるのに成功した。");
            ShowMessage("逃げるのに成功した。");
            commandPanel.gameObject.SetActive(false);
            battleIsOver = true;
            //　戦闘終了
            battleResult.InitialProcessingOfRanAwayResult();
        } else {
            //Debug.Log("逃げるのに失敗した。");
            ShowMessage("逃げるのに失敗した。");
            commandPanel.gameObject.SetActive(false);
            ChangeNextChara();
        }
    }
 
    //　敵の攻撃処理
    public void EnemyAttack(GameObject character) {
        CharacterBattleScript characterBattleScript = character.GetComponent<CharacterBattleScript>();
        CharacterStatus characterStatus = characterBattleScript.GetCharacterStatus();
    
        if (characterStatus.GetSkillList().Count <= 0) {
            return;
        }
        //　敵がガード状態であればガードを解く
        if (character.GetComponent<Animator>().GetBool("Guard")) {
            character.GetComponent<CharacterBattleScript>().UnlockGuard();
        }
    
        //　敵の行動アルゴリズム
        int randomValue = (int)(Random.value * characterStatus.GetSkillList().Count);
        var nowSkill = characterStatus.GetSkillList()[randomValue];
    
        //　テスト用（特定のスキルで確認）
        //nowSkill = characterStatus.GetSkillList()[0];
    
        if (nowSkill.GetSkillType() == Skill.Type.DirectAttack) {
            var targetNum = (int)(Random.value * allyCharacterInBattleList.Count);
            //　攻撃相手のCharacterBattleScript
            characterBattleScript.ChooseAttackOptions(CharacterBattleScript.BattleState.DirectAttack, allyCharacterInBattleList[targetNum], nowSkill);
            Debug.Log(character.name + "は" + nowSkill.GetKanjiName() + "を行った");
        }
        else if (nowSkill.GetSkillType() == Skill.Type.MagicAttack) {
            var targetNum = (int)(Random.value * allyCharacterInBattleList.Count);
            if (characterBattleScript.GetMp() >= ((Magic)nowSkill).GetAmountToUseMagicPoints()) {
                //　攻撃相手のCharacterBattleScript
                characterBattleScript.ChooseAttackOptions(CharacterBattleScript.BattleState.MagicAttack, allyCharacterInBattleList[targetNum], nowSkill);
                Debug.Log(character.name + "は" + nowSkill.GetKanjiName() + "を行った");
            } else {
                ShowMessage("MPが足りない！");
                //　MPが足りない場合は直接攻撃を行う
                characterBattleScript.ChooseAttackOptions(CharacterBattleScript.BattleState.DirectAttack, allyCharacterInBattleList[targetNum], characterStatus.GetSkillList().Find(skill => skill.GetSkillType() == Skill.Type.DirectAttack));
                Debug.Log(character.name + "は攻撃を行った");
            }
        }
        else if (nowSkill.GetSkillType() == Skill.Type.RecoveryMagic) {
            if (characterBattleScript.GetMp() >= ((Magic)nowSkill).GetAmountToUseMagicPoints()) {
                var targetNum = (int)(Random.value * enemyCharacterInBattleList.Count);
                //　回復相手のCharacterBattleScript
                characterBattleScript.ChooseAttackOptions(CharacterBattleScript.BattleState.Healing, enemyCharacterInBattleList[targetNum], nowSkill);
                Debug.Log(character.name + "は" + nowSkill.GetKanjiName() + "を行った");
            } else {
                ShowMessage("MPが足りない！");
                var targetNum = (int)(Random.value * allyCharacterInBattleList.Count);
                //　MPが足りない場合は直接攻撃を行う
                characterBattleScript.ChooseAttackOptions(CharacterBattleScript.BattleState.DirectAttack, allyCharacterInBattleList[targetNum], characterStatus.GetSkillList().Find(skill => skill.GetSkillType() == Skill.Type.DirectAttack));
                Debug.Log(character.name + "は攻撃を行った");
            }
        }
        else if (nowSkill.GetSkillType() == Skill.Type.Guard) {
            characterBattleScript.Guard();
            // Guardアニメはboolなのでアニメーション遷移させたらすぐに次のキャラクターに移行させる
            ChangeNextChara();
            ShowMessage(character.name + "は" + nowSkill.GetKanjiName() + "を行った");
        }
       Debug.Log("敵の攻撃終了"); 
    }
    //　次のキャラクターに移行
    public void ChangeNextChara() {
        isChoosing = false;
        Debug.Log("次のキャラへ移行"); 
    }

    public void DeleteAllCharacterInBattleList(GameObject deleteObj) {
        var deleteObjNum = allCharacterInBattleList.IndexOf(deleteObj);
        allCharacterInBattleList.Remove(deleteObj);
        if(deleteObjNum < currentAttackOrder) {
            currentAttackOrder--;
        }
        //　全員攻撃が終わったら最初から
        if (currentAttackOrder >= allCharacterInBattleList.Count) {
            currentAttackOrder = 0;
        }
    }
    public void DeleteAllyCharacterInBattleList(GameObject deleteObj) {
        allyCharacterInBattleList.Remove(deleteObj);
        if (allyCharacterInBattleList.Count == 0) {
            //Debug.Log("味方が全滅");
            ShowMessage("味方が全滅");
            battleIsOver = true;
            CharacterBattleScript characterBattleScript;
            foreach (var character in allCharacterList) {
                //　味方キャラクターの戦闘で増減したHPとMPを通常のステータスに反映させる
                characterBattleScript = character.GetComponent<CharacterBattleScript>();
                if (characterBattleScript.GetCharacterStatus() as AllyStatus != null) {
                    characterBattleScript.GetCharacterStatus().SetHp(characterBattleScript.GetHp());
                    characterBattleScript.GetCharacterStatus().SetMp(characterBattleScript.GetMp());
                    characterBattleScript.GetCharacterStatus().SetNumbness(characterBattleScript.IsNumbness());
                    characterBattleScript.GetCharacterStatus().SetPoisonState(characterBattleScript.IsPoison());
                }
            }
            //　敗戦時の結果表示
            battleResult.InitialProcessingOfDefeatResult();
        }
    }
    public void DeleteEnemyCharacterInBattleList(GameObject deleteObj) {
        enemyCharacterInBattleList.Remove(deleteObj);
        if (enemyCharacterInBattleList.Count == 0) {
            //Debug.Log("敵が全滅");
            ShowMessage("敵が全滅");
            battleIsOver = true;
            CharacterBattleScript characterBattleScript;
            foreach (var character in allCharacterList) {
                //　味方キャラクターの戦闘で増減したHPとMPを通常のステータスに反映させる
                characterBattleScript = character.GetComponent<CharacterBattleScript>();
                if (characterBattleScript.GetCharacterStatus() as AllyStatus != null) {
                    characterBattleScript.GetCharacterStatus().SetHp(characterBattleScript.GetHp());
                    characterBattleScript.GetCharacterStatus().SetMp(characterBattleScript.GetMp());
                    characterBattleScript.GetCharacterStatus().SetNumbness(characterBattleScript.IsNumbness());
                    characterBattleScript.GetCharacterStatus().SetPoisonState(characterBattleScript.IsPoison());
                }
            }
            //　勝利時の結果表示
            battleResult.InitialProcessingOfVictoryResult(allCharacterList, allyCharacterInBattleList);
        }
    }
    //　メッセージ表示
    public void ShowMessage(string message) {
        if(messagePanelIns != null) {
            Destroy(messagePanelIns);
        }
        messagePanelIns = Instantiate<GameObject>(messagePanel, battleUI);
        messagePanelIns.transform.Find("Text").GetComponent<Text>().text = message;
    }
}

 
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class BattleManager : MonoBehaviour
{
    //　戦闘データ
    [SerializeField]
    private BattleData battleData = null;
    //　キャラクターのベース位置
    [SerializeField]
    private Transform battleBasePosition;
    //　現在戦闘に参加しているキャラクター
    private List<GameObject> allCharacterList = new List<GameObject>();
 
    // Start is called before the first frame update
    void Start() {
        Transform characterTransform;
        List<GameObject> instances = new List<GameObject>();
        GameObject ins;
        //　味方パーティーのプレハブをインスタンス化
        for (int i = 0; i < battleData.GetAllyPartyStatus().GetAllyGameObject().Count; i++) {
            characterTransform = battleBasePosition.Find("AllyPos" + i).transform;
            ins = Instantiate<GameObject>(battleData.GetAllyPartyStatus().GetAllyGameObject()[i], characterTransform.position, characterTransform.rotation);
            allCharacterList.Add(ins);
        }
        //　敵パーティーのプレハブをインスタンス化
        for (int i = 0; i < battleData.GetEnemyPartyStatus().GetEnemyGameObjectList().Count; i++) {
            characterTransform = battleBasePosition.Find("EnemyPos" + i).transform;
            ins = Instantiate<GameObject>(battleData.GetEnemyPartyStatus().GetEnemyGameObjectList()[i], characterTransform.position, characterTransform.rotation);
            allCharacterList.Add(ins);
        }
    }
}*/