// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace System.Net
{
    
    // [EventSource(Name = "Microsoft-System-Net-WebSockets-Client")]
    // internal sealed partial class NetEventSource { }
    
    [EventSource(Name = "Microsoft-System-Net-WebSockets-Client")]
    public sealed class WebSocketsEventSource : EventSource, INetEventSource
    {
        /// <summary>The single event source instance to use for all logging.</summary>
        public static readonly WebSocketsEventSource Log = new WebSocketsEventSource();
        public static new bool IsEnabled => Log.IsEnabled();

        [Event(NetEventSourceEventIds.EnterEventId, Level = EventLevel.Informational, Keywords = NetEventSourceKeywords.EnterExit)]
        public void Enter(string thisOrContextObject, string memberName, string parameters) =>
            WriteEvent(NetEventSourceEventIds.EnterEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, parameters);

        [Event(NetEventSourceEventIds.ExitEventId, Level = EventLevel.Informational, Keywords = NetEventSourceKeywords.EnterExit)]
        public void Exit(string thisOrContextObject, string memberName, string result) =>
            WriteEvent(NetEventSourceEventIds.ExitEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, result);

        [Event(NetEventSourceEventIds.InfoEventId, Level = EventLevel.Informational, Keywords = NetEventSourceKeywords.Default)]
        public void Info(string thisOrContextObject, string memberName, string message) =>
            WriteEvent(NetEventSourceEventIds.InfoEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, message);

        [Event(NetEventSourceEventIds.ErrorEventId, Level = EventLevel.Warning, Keywords = NetEventSourceKeywords.Default)]
        public void ErrorMessage(string thisOrContextObject, string memberName, string message) =>
            WriteEvent(NetEventSourceEventIds.ErrorEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, message);

        [Event(NetEventSourceEventIds.CriticalFailureEventId, Level = EventLevel.Critical, Keywords = NetEventSourceKeywords.Debug)]
        public void CriticalFailure(string thisOrContextObject, string memberName, string message) =>
            WriteEvent(NetEventSourceEventIds.CriticalFailureEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, message);

        [Event(NetEventSourceEventIds.DumpArrayEventId, Level = EventLevel.Verbose, Keywords = NetEventSourceKeywords.Debug)]
        public unsafe void DumpBuffer(string thisOrContextObject, string memberName, byte[] buffer) =>
            NetEventSource.WriteEvent(Log, NetEventSourceEventIds.DumpArrayEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, buffer);

        [Event(NetEventSourceEventIds.AssociateEventId, Level = EventLevel.Informational, Keywords = NetEventSourceKeywords.Default, Message = "[{2}]<-->[{3}]")]
        public void Associate(string thisOrContextObject, string memberName, string first, string second) =>
            NetEventSource.WriteEvent(Log, NetEventSourceEventIds.AssociateEventId, thisOrContextObject, memberName ?? NetEventSourceConstants.MissingMember, first, second);

        public static void AdditionalCustomizedToString<V>(V value, ref string result)
        {
            result = null;
        }
    }
}
