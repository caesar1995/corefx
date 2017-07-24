// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NetEventSourceType =
#if NAMERESO_NES
System.Net.NameResolutionEventSource;
#else
System.Net.NetEventSource;
#endif

namespace System.Net
{
    internal class InternalException : Exception
    {
        internal InternalException()
        {
            NetEventSourceType.Fail(this, "InternalException thrown.");
        }
    }
}
