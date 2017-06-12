using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using XPricer.Gateway.Mapper;
using XPricer.Gateway.Model;

namespace XPricer.Gateway.Controllers
{
    [Route("api/compute")]
    public class ComputeController : Controller
    {
        // POST api/values
        [HttpPost]
        public RequestId Post([FromBody] dynamic data)
        {
            var requests = ((JArray) data).ToObject<IEnumerable<ComputeRequest>>();

            var internalRequests = requests.Select(ComputeRequestMapper.ToInternal);
            var requestId = new RequestId(Guid.NewGuid().ToString());

            return requestId;
        }
    }
}
