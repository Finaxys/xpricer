using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using XPricer.Model;
using XPricer.Model.Product;
using Microsoft.Azure.Batch.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XPricer.Scheduler
{
    public class monitor
    {
        private readonly Settings settings;



        private async Task jobmonitor(string jobid)
        {

            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(
            this.settings.BatchServiceUrl,
            this.settings.BatchAccountName,
            this.settings.BatchAccountKey);

            using (BatchClient batchClient = await BatchClient.OpenAsync(credentials))
            {

                ODATADetailLevel detail = new ODATADetailLevel();
                detail.FilterClause = "state eq 'active'";
                detail.SelectClause = "id,state";
                await QueryTasksAsync(batchClient, jobid, detail);
                detail.FilterClause = "state eq 'running'";
                await QueryTasksAsync(batchClient, jobid, detail);
                detail.FilterClause = "state eq 'completed'";
                await QueryTasksAsync(batchClient, jobid, detail);

                // Get all tasks, but limit the properties returned to task id and state only
                detail.FilterClause = null;
                detail.SelectClause = "id,state";
                await QueryTasksAsync(batchClient, jobid, detail);
             

                // Get all tasks, include id and state, also include the inflated environment settings property
                detail.SelectClause = "id,state,environmentSettings";
                await QueryTasksAsync(batchClient, jobid, detail);

                // Get all tasks, include all standard properties, and expand the statistics
                detail.ExpandClause = "stats";
                detail.SelectClause = null;
               await QueryTasksAsync(batchClient, jobid, detail);

                //return JsonConvert.SerializeObject(QueryTasksAsync);
                
            }
        }

        private static async Task QueryTasksAsync(BatchClient batchClient, string jobId, ODATADetailLevel detail)
        {
            List<CloudTask> taskList = new List<CloudTask>();

            Stopwatch stopwatch = Stopwatch.StartNew();

            taskList.AddRange(await batchClient.JobOperations.ListTasks(jobId, detail).ToListAsync());

            stopwatch.Stop();

            Console.WriteLine("{0} tasks retrieved in {1} (ExpandClause: {2} | FilterClause: {3} | SelectClause: {4})",
                        taskList.Count,
                        stopwatch.Elapsed,
                        detail.ExpandClause,
                        detail.FilterClause,
                        detail.SelectClause);
        }
    }


}
