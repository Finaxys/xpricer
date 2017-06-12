using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XPricer.Model.MarketData;

namespace XPricer.Scheduler
{
    class XPricerScheduler
    {
        private static readonly IList<String> stockList = new List<String>() { "MSFT", "GOOG", "FB" };

    private readonly Settings settings;

        public XPricerScheduler()
        {
            this.settings = Settings.Default;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Running with the following settings: ");
            Console.WriteLine("----------------------------------------");

            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(
               new StorageCredentials(
                   this.settings.StorageAccountName,
                   this.settings.StorageAccountKey),
               this.settings.StorageServiceUrl,
               useHttps: true);

            //Generate a SAS for the container.
            string containerSasUrl = ConstructContainerSas(
                cloudStorageAccount,
                this.settings.BlobContainer);

            //Set up the Batch Service credentials used to authenticate with the Batch Service.
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(
                this.settings.BatchServiceUrl,
                this.settings.BatchAccountName,
                this.settings.BatchAccountKey);

            using (BatchClient batchClient = await BatchClient.OpenAsync(credentials))
            {
                CloudJob xpricerJob = CreateJob(batchClient, Constants.XPricerJob, Constants.XPricerPool);
                List<CloudTask> tasksToRun = new List<CloudTask>();
                foreach (String index in stockList)
                {
                    CloudTask task = new CloudTask("xpricer_task_" + index, String.Format("{0} {1} {2} {3}",
                        "cmd /c %AZ_BATCH_APP_PACKAGE_XPRICER%\\xpricer.exe -args",    
                        String.Format("{0}_{1}", Constants.EquityQuote, index),
                        String.Format("{0}_{1}", Constants.Key, index),
                        containerSasUrl));

                    task.ApplicationPackageReferences = new List<ApplicationPackageReference>
                    {
                        new ApplicationPackageReference
                        {
                            ApplicationId = "xpricer",
                            Version = "1"
                        }
                    };
                    tasksToRun.Add(task);
                }

                //Add tasks to the Job
                batchClient.JobOperations.AddTask(Constants.XPricerJob, tasksToRun);
            }
        }

        private CloudJob CreateJob(BatchClient client, String id, String poolId) {
            Console.WriteLine("Creating job: " + id);
            // get an empty unbound Job
            CloudJob unboundJob = client.JobOperations.CreateJob();
            unboundJob.Id = id;
            unboundJob.PoolInformation = new PoolInformation() { PoolId = poolId };

            // Commit Job to create it in the service
            unboundJob.Commit();

            return unboundJob;
        }

        private string ConstructContainerSas(
            CloudStorageAccount cloudStorageAccount,
            string containerName)
        {
            //Lowercase the container name because containers must always be all lower case
            containerName = containerName.ToLower();

            CloudBlobClient client = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            DateTimeOffset sasStartTime = DateTime.UtcNow;
            TimeSpan sasDuration = TimeSpan.FromHours(2);
            DateTimeOffset sasEndTime = sasStartTime.Add(sasDuration);

            SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessExpiryTime = sasEndTime
            };

            string sasString = container.GetSharedAccessSignature(sasPolicy);
            return String.Format("{0}{1}", container.Uri, sasString); ;
        }

        private List<ResourceFile> GetResourceFiles(string containerSas, IEnumerable<string> dependencies)
        {
            List<ResourceFile> resourceFiles = new List<ResourceFile>();

            foreach (string dependency in dependencies)
            {
                ResourceFile resourceFile = new ResourceFile(ConstructBlobSource(containerSas, dependency), dependency);
                resourceFiles.Add(resourceFile);
            }

            return resourceFiles;
        }

        /// <summary>
        /// Combine container and blob into a URL.
        /// </summary>
        /// <param name="containerSasUrl">Container SAS url.</param>
        /// <param name="blobName">Blob name.</param>
        /// <returns>Full url to the blob.</returns>
        private  string ConstructBlobSource(string containerSasUrl, string blobName)
        {
            int index = containerSasUrl.IndexOf("?");

            if (index != -1)
            {
                //SAS                
                string containerAbsoluteUrl = containerSasUrl.Substring(0, index);
                return containerAbsoluteUrl + "/" + blobName + containerSasUrl.Substring(index);
            }
            else
            {
                return containerSasUrl + "/" + blobName;
            }
        }
    }
}
