using System;
using System.Collections.Generic;
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
            foreach (var request in requests)
            {
                var product = ProductMapper.ToInternal(request.Product);
                var config = PricingConfigMapper.ToInternal(request.Config);

            }
            var requestId = new RequestId(Guid.NewGuid().ToString());

            return requestId;
        }
    }
}
