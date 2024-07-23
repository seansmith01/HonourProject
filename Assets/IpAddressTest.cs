using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FishNet.Object;
using FishNet.Transporting.Tugboat;
using TMPro;


public class IpAddressTest : MonoBehaviour
{
    [SerializeField]
    GameObject[] disableGOs;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] Tugboat tugboat;
    // Start is called before the first frame update
    void Start()
    {
        //tugboat.GetComponent<Tugboat>();
        inputField.onEndEdit.AddListener(SubmitIP);
    }


    private void SubmitIP(string arg0)
    {
        tugboat.SetClientAddress(arg0);
        Debug.Log(arg0);
    }
    private void Update()
    {

        if(Input.GetKeyUp(KeyCode.K)) 
        { 
            for(int i = 0; i < disableGOs.Length; i++)
            {
                disableGOs[i].SetActive(false);
            }
        }
        //print(tugboat.GetClientAddress());
    }
}
