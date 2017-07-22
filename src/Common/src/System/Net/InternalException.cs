// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if NAMERESO_NES
using NetEventSourceType = System.Net.NameResolutionEventSource;
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
