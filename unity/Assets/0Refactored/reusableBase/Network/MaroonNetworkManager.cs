﻿using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CharacterSpawnMessage : MessageBase
{
    public Vector3 CharacterPosition;
    public Quaternion CharacterRotation;
}

public class MaroonNetworkManager : NetworkManager
{
    private NetworkStatusLight _statusLight;
    private ListServer _listServer;
    private MaroonNetworkDiscovery _networkDiscovery;
    private PortForwarding _upnp;
    private GameManager _gameManager;

    private bool _isStarted;
    private bool _activePortMapping;
    
    public override void Start()
    {
        base.Start();
        _listServer = GetComponent<ListServer>();
        _networkDiscovery = GetComponent<MaroonNetworkDiscovery>();
        _upnp = GetComponent<PortForwarding>();
        _statusLight = FindObjectOfType<NetworkStatusLight>();
        _gameManager = FindObjectOfType<GameManager>();
    }

    public void StartMultiUser()
    {
        if(_isStarted)
            return;
        _listServer.ConnectToListServer();
        _networkDiscovery.StartDiscovery();
        _statusLight.SetActive(true);
        _isStarted = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _networkDiscovery.AdvertiseServer();
        _upnp.SetupPortForwarding();
        NetworkServer.RegisterHandler<CharacterSpawnMessage>(OnCreateCharacter);
    }

    private void OnCreateCharacter(NetworkConnection conn, CharacterSpawnMessage message)
    {
        GameObject playerObject = Instantiate(playerPrefab);
        playerObject.transform.position = message.CharacterPosition;
        playerObject.transform.rotation = message.CharacterRotation;

        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if(mode == NetworkManagerMode.ClientOnly)
            _networkDiscovery.StopDiscovery();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        _gameManager.UnregisterNetworkPlayer();
        _networkDiscovery.StartDiscovery();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Transform playerTransform = _gameManager.GetPlayerTransform();
        
        // you can send the message here, or wherever else you want
        CharacterSpawnMessage characterMessage = new CharacterSpawnMessage
        {
            CharacterPosition = playerTransform.position,
            CharacterRotation = playerTransform.rotation
        };

        conn.Send(characterMessage);
    }

    public void PortsMapped()
    {
        //TODO: Manual Port Mapping
        _activePortMapping = true;
        _listServer.PortMappingSuccessfull();
    }
    
    public override void OnApplicationQuit()
    {
        if (_activePortMapping)
        {
            _upnp.DeletePortMapping();
        }
        base.OnApplicationQuit();
    }
}
