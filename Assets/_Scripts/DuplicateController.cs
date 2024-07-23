using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DuplicateController : MonoBehaviour
{
    public int PlayerNumber;
    public bool IsFirstItterationDuplicateNOTWORKINGYET;
    public Transform CameraHolder;
    public Transform GunTip;
    //public OneShotAudioHolder OneShotAudioHolder;
    public AudioSource FallingAudioSource;

    [Header("Player Meshes")]
    [SerializeField] MeshRenderer bodyMesh;
    [SerializeField] MeshRenderer headMesh;
    public void SetupColour(Material mat)
    {
        bodyMesh.material = mat;
        //headMesh.material = mat;
    }
}
