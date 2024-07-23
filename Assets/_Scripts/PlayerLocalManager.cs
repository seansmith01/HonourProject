using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet.Object;
using FishNet.Example.Prediction.CharacterControllers;

public class PlayerLocalManager : NetworkBehaviour
{
    public bool IsReady;
    [SerializeField] Material[] materials;
    [SerializeField] MeshRenderer meshRenderer;
    DuplicateManager duplicateManager;
    [SerializeField] GameObject fakeCollier;
    int ID;
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        ID = NetworkObject.OwnerId;

        StartCoroutine(WaitToEnableCPSMotor());

        gameObject.name = "Player: " + ID;

        gameObject.layer = LayerMask.NameToLayer("Player" + ID);
        fakeCollier.layer = LayerMask.NameToLayer("FakeCollider" + ID);
        fakeCollier.name = "FakeCollider:" + gameObject.name;
       

        SetColour(materials[ID]);

        

        // If the player is the host, they are ready instanlty
        if (base.IsServer && OwnerId == 0)
        {
            IsReady = true;
        }
        // Else, set the player on the server side to ready
        if(!base.IsServer)
        {
            StartCoroutine(WaitToTellServerWeAreReady());
        }

    }

    
    [ServerRpc] 
    void SetIsReadyRPC()
    {
        IsReady = true;
    }
    IEnumerator WaitToTellServerWeAreReady()
    {
        yield return new WaitForSeconds(0);
        SetIsReadyRPC();
    }
    IEnumerator WaitToEnableCPSMotor()
    {
        yield return new WaitForSeconds(1);
        GetComponent<CSPMotor>().enabled = true;

    }

    void SetColour(Material mat)
    {
        meshRenderer.material = mat;
    }

    public Material GetMaterial(int ID)
    {
        return materials[ID];
    }
}
