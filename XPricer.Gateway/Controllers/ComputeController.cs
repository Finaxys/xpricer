using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using XPricer.Gateway.Mapper;
using XPricer.Gateway.Model;
using XPricer.Scheduler;

namespace XPricer.Gateway.Controllers
{
    [Route("api/compute")]
    public class ComputeController : Controller
    {
        // POST api/values
        [HttpPost]
        public async Task<RequestId> Post([FromBody] dynamic data)
        {
            var requests = ((JArray) data).ToObject<IEnumerable<ComputeRequest>>();

            var internalRequests = requests.Select(ComputeRequestMapper.ToInternal);
            var scheduler = new XPricerScheduler();
            var requestId = await scheduler.RunAsync(internalRequests);
            return RequestIdMapper.ToExternal(requestId);
        }
    }
}
