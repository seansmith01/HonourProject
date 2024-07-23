using FishNet.Component.Prediction;
using FishNet.Component.Transforming;
using FishNet.Transporting;
using FishNet.Object;
using System.Buffers.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.UIElements;
using static FishNet.Component.Transforming.NetworkTransform;
using FishNet.Managing.Timing;
using FishNet;

public class CustomPrection : NetworkBehaviour
{
    private CharacterController characterController;
    private Quaternion predictedRotation;
    private float moveSpeed = 5f;
    private float rotationSpeed = 180f;


    private float maxFallSpeed;
    private float maxGroundSpeed;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner || IsHost)
        {
            enabled = false;
            return;
        }
        GetComponent<PredictedObject>().enabled = false;
        characterController = GetComponent<CharacterController>();

        maxGroundSpeed = GetComponent<CSPMotor>().MoveSpeed;
        maxFallSpeed = GetComponent<CSPMotor>().MaxFallSpeed;

       //predictedPosition = transform.position;
       //predictedRotation = transform.rotation;

        // Initialize previousPositionDelta to the current position to avoid NaN issues

        GetComponent<NetworkTransform>().OnDataReceived += OnDataReceivedChanged;

    }

    float previousTickReceived;
    float currentTickReceived;
    bool hasTeleported;

    public List<Vector3> receivedDifferences = new List<Vector3>();
    
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        //base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        //if (base.TimeManager != null)
            //base.TimeManager.OnTick -= TimeManager_OnTick;
    }
    Vector3 currentPositionAtDataReceived;
    Vector3 previousReceivedPosition;
    Vector3 currentReceivedPosition;
    [SerializeField] float smoothDuration;
    [SerializeField] float predictionMultiplier;
    [SerializeField] LayerMask groundLayer;
    //private void TimeManager_OnTick() { }
        public float YVelocity = 0;
    private void FixedUpdate()
    {
        if (IsOwner || IsServer)
        {
            return;
        }
        float height = characterController.bounds.size.y;
        Debug.DrawRay(transform.position, -transform.up * height / 2, Color.blue);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, height / 2, groundLayer))
        {
            // Grounded
            print("Hitting:" + hit.transform.gameObject.name);
            YVelocity = 0;
        }
        else
        {
            // In Air
            YVelocity = -maxFallSpeed;
        }
    }
    [SerializeField] int TrialAndError;
    void OnDataReceivedChanged(TransformData prev, TransformData current)
    {
        if (IsOwner)
        {
            return;
        }

        previousTickReceived = prev.Tick;
        currentTickReceived = current.Tick;
        //Debug.Log("prev:" + prev.Tick + ",current:" + current.Tick + ",Server Tick: "+TimeManager.Tick);

        previousReceivedPosition = prev.Position;
        currentReceivedPosition = current.Position;

        currentPositionAtDataReceived = transform.position;

        receivedDifferences.Add(currentReceivedPosition - previousReceivedPosition);


        // save list of 2 differences
        if (receivedDifferences.Count > 2)
        {
            receivedDifferences.RemoveAt(0);
        }

        if (Vector3.Distance(currentReceivedPosition, previousReceivedPosition) > 20)
        {
            //currentPositionAtDataReceived = previousReceivedPosition;
            hasTeleported = true;
        }

        if (hasTeleported)
        {
            float ping = GetPing();



            Vector3 XZvelocity;
            Vector3 displacement;
            Vector3 predictedTeleportedPos;
            Vector3 interpDiff;
            Vector3 diff = receivedDifferences[0];

            //diff = previousDifference; // should always be the previously saved 

            XZvelocity = new Vector3(diff.x, 0, diff.z) / ((currentTickReceived - previousTickReceived) / TimeManager.TickRate);
            displacement = XZvelocity * ((TimeManager.Tick - currentTickReceived) / TimeManager.TickRate + (ping / 2));


            //Vector3 predictedPosNonTeleport = currentReceivedPosition + new Vector3(displacement.x, 0, displacement.z);
            print(", transform.position: " + transform.position);
            Vector3 predictedPosNonTeleport = transform.position + new Vector3(displacement.x, 0, displacement.z);


            interpDiff = predictedPosNonTeleport - transform.position;

            print(",interpDiff: " + interpDiff + " Displacement: " + displacement + " Velocity: " + XZvelocity);

            predictedTeleportedPos = currentReceivedPosition + new Vector3(displacement.x, 0, displacement.z);

            transform.position = predictedTeleportedPos - interpDiff;

            currentPositionAtDataReceived = transform.position;
            hasTeleported = false;
            print("transform.position before:" + transform.position + " ,  predictedPos: " + predictedTeleportedPos + ",currentPositionAtDataReceived: " + currentPositionAtDataReceived);


            // until new message is received, make both in list be the previous difference between the states
            receivedDifferences.Add(receivedDifferences[0]);


            // save list of 2 differences
            if (receivedDifferences.Count > 2)
            {
               receivedDifferences.RemoveAt(0);
            }


        }
    }
    private void Update()
    {

        if (IsOwner || IsServer)
        {
            return;
        }

        float ping = GetPing();

        //Vector3 diff = currentReceivedPosition - previousReceivedPosition;
        Vector3 mostRecentDifference = receivedDifferences[1];
        Vector3 previousDifference = receivedDifferences[0];

        Vector3 XZvelocity;
        Vector3 displacement;
        Vector3 predictedPos;

        Vector3 diff = mostRecentDifference;
        Vector3 interpDiff = Vector3.zero;

        XZvelocity = new Vector3(diff.x, 0, diff.z) / ((currentTickReceived - previousTickReceived) / TimeManager.TickRate);
        displacement = XZvelocity * ((TimeManager.Tick - currentTickReceived) / TimeManager.TickRate + (ping / 2));


        predictedPos = currentReceivedPosition + new Vector3(displacement.x, 0, displacement.z);


        transform.position += new Vector3(0, YVelocity, 0);

        float timeLeft = (TimeManager.Tick - currentTickReceived) / TimeManager.TickRate;
        timeLeft = timeLeft / smoothDuration;


        if (timeLeft <= 1 & timeLeft >= 0)
        {
            transform.position = Vector3.Lerp(currentPositionAtDataReceived, predictedPos, timeLeft);
        }
        else
        {
            transform.position = predictedPos - interpDiff;
        }




    }

    private static float GetPing()
    {
        float ping;
        TimeManager tm = InstanceFinder.TimeManager;
        if (tm == null)
        {
            ping = 0;
        }
        else
        {
            ping = (float)tm.RoundTripTime;
        }

        ping *= 0.001f; // make it into milliseconds
        return ping;
    }
}
