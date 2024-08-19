using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class PhotonPeer
{
    public const bool NoSocket = false;

    internal PeerBase peerBase;

    private readonly object SendOutgoingLockObject = new object();
    private readonly object DispatchLockObject = new object();
    private readonly object EnqueueLock = new object();

    public Type SocketImplementation
    {
        get
        {
            return peerBase?.SocketImplementation;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.SocketImplementation = value;
            }
        }
    }

    public DebugLevel DebugOut
    {
        get
        {
            return peerBase?.debugOut ?? DebugLevel.ERROR;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.debugOut = value;
            }
        }
    }

    public IPhotonPeerListener Listener
    {
        get
        {
            return peerBase?.Listener;
        }
        protected set
        {
            if (peerBase != null)
            {
                peerBase.Listener = value;
            }
        }
    }

    public long BytesIn
    {
        get
        {
            return peerBase?.BytesIn ?? 0;
        }
    }

    public long BytesOut
    {
        get
        {
            return peerBase?.BytesOut ?? 0;
        }
    }

    public int ByteCountCurrentDispatch
    {
        get
        {
            return peerBase?.ByteCountCurrentDispatch ?? 0;
        }
    }

    public string CommandInfoCurrentDispatch
    {
        get
        {
            return peerBase?.CommandInCurrentDispatch != null ? peerBase.CommandInCurrentDispatch.ToString() : string.Empty;
        }
    }

    public int ByteCountLastOperation
    {
        get
        {
            return peerBase?.ByteCountLastOperation ?? 0;
        }
    }

    public bool TrafficStatsEnabled
    {
        get
        {
            return peerBase?.TrafficStatsEnabled ?? false;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.TrafficStatsEnabled = value;
            }
        }
    }

    public long TrafficStatsElapsedMs
    {
        get
        {
            return peerBase?.TrafficStatsEnabledTime ?? 0;
        }
    }

    public TrafficStats TrafficStatsIncoming
    {
        get
        {
            return peerBase?.TrafficStatsIncoming;
        }
    }

    public TrafficStats TrafficStatsOutgoing
    {
        get
        {
            return peerBase?.TrafficStatsOutgoing;
        }
    }

    public TrafficStatsGameLevel TrafficStatsGameLevel
    {
        get
        {
            return peerBase?.TrafficStatsGameLevel;
        }
    }

    public int CommandLogSize
    {
        get
        {
            return peerBase?.CommandLogSize ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.CommandLogSize = value;
                peerBase.CommandLogResize();
            }
        }
    }

    public byte QuickResendAttempts
    {
        get
        {
            return peerBase?.QuickResendAttempts ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.QuickResendAttempts = (byte)((value > 4) ? 4 : value);
            }
        }
    }

    public PeerStateValue PeerState
    {
        get
        {
            if (peerBase != null && peerBase.peerConnectionState == PeerBase.ConnectionStateValue.Connected && !peerBase.ApplicationIsInitialized)
            {
                return PeerStateValue.InitializingApplication;
            }
            return (PeerStateValue)(peerBase?.peerConnectionState ?? PeerBase.ConnectionStateValue.Disconnected);
        }
    }

    public string PeerID
    {
        get
        {
            return peerBase?.PeerID;
        }
    }

    public int CommandBufferSize
    {
        get
        {
            return peerBase?.commandBufferSize ?? 0;
        }
    }

    public int RhttpMinConnections
    {
        get
        {
            return peerBase?.rhttpMinConnections ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                if (value >= 8)
                {
                    if ((int)DebugOut >= 2)
                    {
                        Listener?.DebugReturn(DebugLevel.WARNING, "Forcing RhttpMinConnections=7 the currently max supported value.");
                    }
                    peerBase.rhttpMinConnections = 7;
                }
                else
                {
                    peerBase.rhttpMinConnections = value;
                }
            }
        }
    }

    public int RhttpMaxConnections
    {
        get
        {
            return peerBase?.rhttpMaxConnections ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.rhttpMaxConnections = value;
            }
        }
    }

    public int LimitOfUnreliableCommands
    {
        get
        {
            return peerBase?.limitOfUnreliableCommands ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.limitOfUnreliableCommands = value;
            }
        }
    }

    public int QueuedIncomingCommands
    {
        get
        {
            return peerBase?.QueuedIncomingCommandsCount ?? 0;
        }
    }

    public int QueuedOutgoingCommands
    {
        get
        {
            return peerBase?.QueuedOutgoingCommandsCount ?? 0;
        }
    }

    public byte ChannelCount
    {
        get
        {
            return peerBase?.ChannelCount ?? 0;
        }
        set
        {
            if (peerBase != null && (value == 0 || PeerState != 0))
            {
                throw new Exception("ChannelCount can only be set while disconnected and must be > 0.");
            }
            if (peerBase != null)
            {
                peerBase.ChannelCount = value;
            }
        }
    }

    public bool CrcEnabled
    {
        get
        {
            return peerBase?.crcEnabled ?? false;
        }
        set
        {
            if (peerBase != null && peerBase.peerConnectionState != 0)
            {
                throw new Exception("CrcEnabled can only be set while disconnected.");
            }
            if (peerBase != null)
            {
                peerBase.crcEnabled = value;
            }
        }
    }

    public int PacketLossByCrc
    {
        get
        {
            return peerBase?.packetLossByCrc ?? 0;
        }
    }

    public int PacketLossByChallenge
    {
        get
        {
            return peerBase?.packetLossByChallenge ?? 0;
        }
    }

    public int ResentReliableCommands
    {
        get
        {
            return (UsedProtocol == ConnectionProtocol.Udp) ? ((EnetPeer)peerBase)?.reliableCommandsRepeated ?? 0 : 0;
        }
    }

    public int WarningSize
    {
        get
        {
            return peerBase?.warningSize ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.warningSize = value;
            }
        }
    }

    public int SentCountAllowance
    {
        get
        {
            return peerBase?.sentCountAllowance ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.sentCountAllowance = value;
            }
        }
    }

    public int TimePingInterval
    {
        get
        {
            return peerBase?.timePingInterval ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.timePingInterval = value;
            }
        }
    }

    public int DisconnectTimeout
    {
        get
        {
            return peerBase?.DisconnectTimeout ?? 0;
        }
        set
        {
            if (peerBase != null)
            {
                peerBase.DisconnectTimeout = value;
            }
        }
    }

    public int ServerTimeInMilliSeconds
    {
        get
        {
            return peerBase?.serverTimeOffsetIsAvailable == true ? (peerBase.serverTimeOffset + SupportClass.GetTickCount()) : 0;
        }
    }

    public int ConnectionTime
    {
        get
        {
            return peerBase?.timeInt ?? 0;
        }
    }

    public int LastSendAckTime
    {
        get
        {
            return peerBase?.timeLastSendAck ?? 0;
        }
    }

public int LastSendOutgoingTime
{
    get
    {
        return peerBase?.timeLastSendOutgoing ?? 0;
    }
}

[Obsolete("Should be replaced by: SupportClass.GetTickCount(). Internally this is used, too.")]
public int LocalTimeInMilliSeconds
{
    get
    {
        return SupportClass.GetTickCount();
    }
}

public SupportClass.IntegerMillisecondsDelegate LocalMsTimestampDelegate
{
    set
    {
        if (PeerState != 0)
        {
            throw new Exception("LocalMsTimestampDelegate only settable while disconnected. State: " + PeerState);
        }
        SupportClass.IntegerMilliseconds = value;
    }
}

public int RoundTripTime
{
    get
    {
        return peerBase?.roundTripTime ?? 0;
    }
}

public int RoundTripTimeVariance
{
    get
    {
        return peerBase?.roundTripTimeVariance ?? 0;
    }
}

public int TimestampOfLastSocketReceive
{
    get
    {
        return peerBase?.timestampOfLastReceive ?? 0;
    }
}

public string ServerAddress
{
    get
    {
        return peerBase?.ServerAddress;
    }
    set
    {
        if ((int)DebugOut >= 1)
        {
            Listener?.DebugReturn(DebugLevel.ERROR, "Failed to set ServerAddress. Can be set only when using HTTP.");
        }
    }
}

public ConnectionProtocol UsedProtocol
{
    get
    {
        return peerBase?.usedProtocol ?? ConnectionProtocol.Udp;
    }
}

public virtual bool IsSimulationEnabled
{
    get
    {
        return NetworkSimulationSettings.IsSimulationEnabled;
    }
    set
    {
        if (value == NetworkSimulationSettings.IsSimulationEnabled)
        {
            return;
        }
        lock (SendOutgoingLockObject)
        {
            NetworkSimulationSettings.IsSimulationEnabled = value;
        }
    }
}

public NetworkSimulationSet NetworkSimulationSettings
{
    get
    {
        return peerBase?.NetworkSimulationSettings;
    }
}

public static int OutgoingStreamBufferSize
{
    get
    {
        return PeerBase.outgoingStreamBufferSize;
    }
    set
    {
        PeerBase.outgoingStreamBufferSize = value;
    }
}

public int MaximumTransferUnit
{
    get
    {
        return peerBase?.mtu ?? 0;
    }
    set
    {
        if (PeerState != 0)
        {
            throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + PeerState);
        }
        if (value < 576)
        {
            value = 576;
        }
        if (peerBase != null)
        {
            peerBase.mtu = value;
        }
    }
}

public bool IsEncryptionAvailable
{
    get
    {
        return peerBase?.isEncryptionAvailable ?? false;
    }
}

public bool IsSendingOnlyAcks
{
    get
    {
        return peerBase?.IsSendingOnlyAcks ?? false;
    }
    set
    {
        lock (SendOutgoingLockObject)
        {
            if (peerBase != null)
            {
                peerBase.IsSendingOnlyAcks = value;
            }
        }
    }
}

public void TrafficStatsReset()
{
    if (peerBase != null)
    {
        peerBase.InitializeTrafficStats();
        peerBase.TrafficStatsEnabled = true;
    }
}

public string CommandLogToString()
{
    return peerBase?.CommandLogToString() ?? string.Empty;
}

public PhotonPeer(ConnectionProtocol protocolType)
{
    switch (protocolType)
    {
        case ConnectionProtocol.Tcp:
            peerBase = new TPeer();
            peerBase.usedProtocol = protocolType;
            break;
        case ConnectionProtocol.Udp:
            peerBase = new EnetPeer();
            peerBase.usedProtocol = protocolType;
            break;
        default:
            if (protocolType != ConnectionProtocol.WebSocketSecure)
            {
                break;
            }
            goto case ConnectionProtocol.WebSocket;
        case ConnectionProtocol.WebSocket:
            peerBase = new TPeer
            {
                DoFraming = false
            };
            peerBase.usedProtocol = protocolType;
            break;
    }
}

public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
    : this(protocolType)
{
    Listener = listener;
}

[Obsolete("Use the constructor with ConnectionProtocol instead.")]
public PhotonPeer(IPhotonPeerListener listener)
    : this(listener, ConnectionProtocol.Udp)
{
}

[Obsolete("Use the constructor with ConnectionProtocol instead.")]
public PhotonPeer(IPhotonPeerListener listener, bool useTcp)
    : this(listener, useTcp ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp)
{
}

public virtual bool Connect(string serverAddress, string applicationName)
{
    lock (DispatchLockObject)
    {
        lock (SendOutgoingLockObject)
        {
            return peerBase?.Connect(serverAddress, applicationName) ?? false;
        }
    }
}

public virtual void Disconnect()
{
    lock (DispatchLockObject)
    {
        lock (SendOutgoingLockObject)
        {
            peerBase?.Disconnect();
        }
    }
}

public virtual void StopThread()
{
    lock (DispatchLockObject)
    {
        lock (SendOutgoingLockObject)
        {
            peerBase?.StopConnection();
        }
    }
}

public virtual void FetchServerTimestamp()
{
    peerBase?.FetchServerTimestamp();
}

public bool EstablishEncryption()
{
    return peerBase?.ExchangeKeysForEncryption() ?? false;
}

public virtual void Service()
{
    while (DispatchIncomingCommands())
    {
    }
    while (SendOutgoingCommands())
    {
    }
}

public virtual bool SendOutgoingCommands()
{
    if (TrafficStatsEnabled)
    {
        TrafficStatsGameLevel?.SendOutgoingCommandsCalled();
    }
    lock (SendOutgoingLockObject)
    {
        return peerBase?.SendOutgoingCommands() ?? false;
    }
}

public virtual bool SendAcksOnly()
{
    if (TrafficStatsEnabled)
    {
        TrafficStatsGameLevel?.SendOutgoingCommandsCalled();
    }
    lock (SendOutgoingLockObject)
    {
        return peerBase?.SendAcksOnly() ?? false;
    }
}

public virtual bool DispatchIncomingCommands()
{
    if (peerBase != null)
    {
        peerBase.ByteCountCurrentDispatch = 0;
    }
    if (TrafficStatsEnabled)
    {
        TrafficStatsGameLevel?.DispatchIncomingCommandsCalled();
    }
    lock (DispatchLockObject)
    {
        return peerBase?.DispatchIncomingCommands() ?? false;
    }
}

public string VitalStatsToString(bool all)
{
    if (TrafficStatsGameLevel == null)
    {
        return "Stats not available. Use PhotonPeer.TrafficStatsEnabled.";
    }
    if (!all)
    {
        return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {4}sec.\n{3}", RoundTripTime, RoundTripTimeVariance, SupportClass.GetTickCount() - TimestampOfLastSocketReceive, TrafficStatsGameLevel.ToStringVitalStats(), TrafficStatsElapsedMs / 1000);
    }
    return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", RoundTripTime, RoundTripTimeVariance, SupportClass.GetTickCount() - TimestampOfLastSocketReceive, TrafficStatsGameLevel.ToStringVitalStats(), TrafficStatsIncoming.ToString(), TrafficStatsOutgoing.ToString(), TrafficStatsElapsedMs / 1000);
}

public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable)
{
    return OpCustom(customOpCode, customOpParameters, sendReliable, 0);
}

public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId)
{
    lock (EnqueueLock)
    {
        return peerBase?.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, false) ?? false;
    }
}

public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId, bool encrypt)
{
    if (encrypt && !IsEncryptionAvailable)
    {
        throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
    }
    lock (EnqueueLock)
    {
        return peerBase?.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, encrypt) ?? false;
    }
}

public virtual bool OpCustom(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
{
    if (encrypt && !IsEncryptionAvailable)
    {
        throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
    }
    lock (EnqueueLock)
    {
        return peerBase?.EnqueueOperation(operationRequest.Parameters, operationRequest.OperationCode, sendReliable, channelId, encrypt) ?? false;
    }
}

    public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
    {
        return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
    }

    public static bool RegisterType(Type customType, byte code, SerializeStreamMethod serializeMethod, DeserializeStreamMethod constructor)
    {
        return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
    }
}
