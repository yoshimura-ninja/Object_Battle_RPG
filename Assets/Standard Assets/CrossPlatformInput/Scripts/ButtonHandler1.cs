using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace UnityStandardAssets.CrossPlatformInput
{
    public class ButtonHandler : MonoBehaviour
    {

        [SerializeField]
        private GameObject SelectCharacterPanel;
        
        public string Name;

        void OnEnable()
        {

        }

        public void SetDownState()
        {
            //CrossPlatformInputManager.SetButtonDown("Cancel");
            //Debug.Log("押してるよ");
        }


        public void SetUpState()
        {
            CrossPlatformInputManager.SetButtonUp(Name);
            
        }


        public void SetAxisPositiveState()
        {
            CrossPlatformInputManager.SetAxisPositive(Name);
        }


        public void SetAxisNeutralState()
        {
            CrossPlatformInputManager.SetAxisZero(Name);
        }


        public void SetAxisNegativeState()
        {
            CrossPlatformInputManager.SetAxisNegative(Name);
        }

        public void Update()
        {

        }
        public void OnClick()
        {
            //Debug.Log(SelectCharacterPanel.name + " activeInHierarchy : " + SelectCharacterPanel.activeInHierarchy);
            if(SelectCharacterPanel.activeInHierarchy==true)
            {
            CrossPlatformInputManager.SetButtonDown("Cancel");
            CrossPlatformInputManager.SetButtonUp("Cancel");
            //Debug.Log("押してるよ");
            }
        }
    }
}
