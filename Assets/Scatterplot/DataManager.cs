﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ImmVis.Messages;
using ImmVis.Serialization;
using ImmVis.Discovery;
using ImmVis.WebSocketManager;

public class DataManager : MonoBehaviour
{

    public ImmVisWebSocketManager WebsocketManager;

    public ImmVisDiscoveryManager DiscoveryManager;

    public bool ShouldUseDiscoveryService = true;

    public ScatterplotBehaviour scatterplotBehaviour;

    void Start()
    {
        RegisterMessageTypes();

        if (ShouldUseDiscoveryService && DiscoveryManager != null)
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
        if (availableServersIps.Count > 0)
        {
            WebsocketManager.ServerAddress = availableServersIps[0];
            InitializeWebsocketClient();
        }
    }

    private void RegisterMessageTypes()
    {
        MessageConverter.RegisterMessage(Hello.MessageType, Hello.CreateMessage);
        MessageConverter.RegisterMessage(LoadDatasetResult.MessageType, LoadDatasetResult.CreateMessage);
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
        WebsocketManager.Send(LoadDataset.Create(@"C:\Users\felip\Projects\masters\python\immvis-server\example_datasets\OndasCalor_IAC_1961_2018.csv"));
    }

    private void MessageReceived(Message message)
    {
        Debug.Log($"Received message: {message} - {message.GetType()}");

        if (message.GetType() == typeof(LoadDatasetResult))
        {
            scatterplotBehaviour?.PlotData((message as LoadDatasetResult).Data);
        }

    }
}
