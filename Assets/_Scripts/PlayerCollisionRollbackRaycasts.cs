using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerCollisionRollbackRaycasts : NetworkBehaviour
{
    [Serializable]
    public class State
    {
        public Vector3 Position;
        public Quaternion Rotation; // currently rotation is only saved so it can be determined if the player is hit from behind or not
        public Vector3 Direction;
        public Vector3 CameraHolderPos;
        public Vector3 GunTipPosition;
        public int Tick;
    }
    [SerializeField] public List<State> PastStates = new List<State>(); 
    public List<int> PastTicks = new List<int>();
    //public List<Vector3> PastStates = new List<Vector3>();
    [SerializeField] private GameObject fakeCapsule;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform gunTip;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (!IsServer)
        {
            fakeCapsule.SetActive(true);
            enabled = false;
            return;
        }

        fakeCapsule.GetComponent<FakeCollider>().PlayerOwner = gameObject;
        fakeCapsule.transform.parent = null;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        //Players.Add(Owner.ClientId, this);

        TimeManager.OnTick += OnTick;

    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        //Players.Remove(Owner.ClientId);
    }
   
    private void OnTick()
    {

        //if (PastStates.Count > TimeManager.TickRate)
        if (PastStates.Count > 1000)
            PastStates.RemoveAt(0);

        //add position every tike
        PastStates.Add(new State() { 
            Position = transform.position ,
            Rotation = transform.rotation ,
            Direction = cameraHolder.forward,
            CameraHolderPos = cameraHolder.position,
            GunTipPosition = gunTip.position ,
            Tick = (int)TimeManager.Tick 
        });

        //PastStates.Add(transform.position);
        //PastTicks.Add((int)TimeManager.Tick );
    }
    public void DisableFakeCollider()
    {
        fakeCapsule.SetActive(false);

    }
    public void EnableFakeCollider(Vector3 position, Quaternion rotation)
    {
        fakeCapsule.transform.position = position;
        fakeCapsule.transform.rotation = rotation;
        fakeCapsule.SetActive(true);
    }

}