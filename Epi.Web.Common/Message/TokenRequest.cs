﻿using System.Runtime.Serialization;
using Epi.Web.Enter.Common.MessageBase;

namespace Epi.Web.Enter.Common.Message
{
    /// <summary>
    /// Respresents a security token request message from client to web service.
    /// </summary>
    [DataContract(Namespace = "http://www.yourcompany.com/types/")]
    public class TokenRequest : RequestBase
    {
        // Nothing needed here...
    }
}

