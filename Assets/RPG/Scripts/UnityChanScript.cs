using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Input;
//joystick用
using UnityStandardAssets.CrossPlatformInput;

 
public class UnityChanScript : MonoBehaviour
{
    //追記部分
    public enum State
    {
        Command,
        Wait,
        Normal,
        Talk
    }

    private CharacterController characterController;
    private Animator animator;
    //　キャラクターの速度
    private Vector3 velocity;
    //　キャラクターの歩くスピード
    [SerializeField]
    private float walkSpeed = 2f;
    //　キャラクターの走るスピード
    [SerializeField]
    private float runSpeed = 4f;
    
    //　ユニティちゃんの状態(追記部分)
    private State state;
 
    // Start is called before the first frame update
    void Start()
    {
        //追記部分
        state = State.Normal;

        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }
 
    // Update is called once per frame
    void Update()
    {
        if(characterController.isGrounded) {
            velocity = Vector3.zero;

            //mobile版joystick追加部分
            // 右・左
            float x = CrossPlatformInputManager.GetAxisRaw("Horizontal");

            // 上・下
            float y = CrossPlatformInputManager.GetAxisRaw("Vertical");

            // 移動する向きを求める
            Vector2 direction = new Vector2 (x, y);
            //移動
            var input = new Vector3(x, 0f, y);

            //joystickを使わない版
            //var input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
                        
            if(input.magnitude > 0.1f) 
            {
                transform.LookAt(transform.position + input.normalized);
                animator.SetFloat("Speed", input.magnitude);
                if (input.magnitude > 0.5f) {
                    velocity += transform.forward * runSpeed;
                } else {
                    velocity += transform.forward * walkSpeed;
                }                
            } 
            else 
            {
                animator.SetFloat("Speed", 0f);
            }
        }
 
        velocity.y += Physics.gravity.y * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    //　状態変更と初期設定(追記部分)
    public void SetState(State state) {
        this.state = state;
    
        if(state == State.Talk) {
            velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
            //unityChanTalkScript.StartTalking();
        } else if(state == State.Command) {
            velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        } else if (state == State.Wait) {
            velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }
    }
    //追記部分
    public State GetState()
    {
    return state;
    }
}
