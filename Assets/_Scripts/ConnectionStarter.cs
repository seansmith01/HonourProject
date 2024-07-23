using FishNet;
using FishNet.Transporting;
using FishNet.Transporting.Tugboat;
using System;
using UnityEditor;
using UnityEngine;

public class ConnectionStarter : MonoBehaviour
{
    [SerializeField] private ConnectionType connectionType;

    private Tugboat tugboat;
    bool isServer;
    private void Awake()
    {
        if (TryGetComponent(out Tugboat t))
        {
            tugboat = t;
        }
        else
        {
            Debug.LogError("Tugboat not found", gameObject);
            return;
        }
#if UNITY_EDITOR
        if (connectionType == ConnectionType.Host)
        {
            if (ParrelSync.ClonesManager.IsClone())
            {
                // start as client
                tugboat.StartConnection(false);
            }
            else
            {
                // start as server and client
                tugboat.StartConnection(true);
                tugboat.StartConnection(false);
            }

            return;
        }

        tugboat.StartConnection(false);

#endif
#if !UNITY_EDITOR
        tugboat.StartConnection(true);
#endif
    }

    private void Start()
    {
        InstanceFinder.ClientManager.OnClientConnectionState += OnClientConnectionState;

    }

    private void OnClientConnectionState(ClientConnectionStateArgs args)
    {

#if UNITY_EDITOR
        if (args.ConnectionState == LocalConnectionState.Stopping)
            EditorApplication.isPlaying = false;
#endif
    }

    public enum ConnectionType
    {
        Host,
        Client
    }
}