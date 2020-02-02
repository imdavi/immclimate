using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{

    public ImmVisWebsocketManager WebsocketManager;

    public ImmVisDiscoveryManager DiscoveryManager;

    public bool ShouldUseDiscoveryService = true;


    void Start()
    {
        RegisterMessageTypes();

        if(ShouldUseDiscoveryService && DiscoveryManager != null)
        {
            DiscoveryManager.DiscoveryFinished += DiscoveryFinished;
            DiscoveryManager.StartDiscovery(shouldReturnOnFirstOccurrence: true);
            Debug.Log("Discovery has started!");
        }
        else
        {
            InitializeWebsocketClient();
        }
    }

    private void DiscoveryFinished(List<string> availableServersIps)
    {
        if(availableServersIps.Count > 0)
        {
            WebsocketManager.ServerAddress = availableServersIps[0];
            InitializeWebsocketClient();
        }
    }

    private void RegisterMessageTypes()
    {
        SerializationUtils.RegisterMessageType<Hello>(Hello.MessageType);
    }

    private void InitializeWebsocketClient()
    {
        if (WebsocketManager != null && !WebsocketManager.IsConnected)
        {
            WebsocketManager.Connected += ClientConnected;
            WebsocketManager.MessageReceived += MessageReceived;
            WebsocketManager.RawMessageReceived += RawMessageReceived;
            WebsocketManager.InitializeClient();
        }
    }

    private void RawMessageReceived(string message)
    {
        Debug.Log(message);
    }

    private void ClientConnected()
    {
        Debug.Log("Now you can send messages!");
        WebsocketManager.Send(LoadDataset.Create("/home/felipe/Projects/masters/python/immvis-server/example_datasets/111.csv"));
    }

    private void MessageReceived(Message message)
    {

    }
}
