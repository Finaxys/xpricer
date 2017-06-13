using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using XPricer.Model;
using XPricer.Model.Product;

namespace XPricer.Scheduler
{

    public class XPricerScheduler 
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(typeof(XPricerScheduler));

        private readonly Settings settings;

        public XPricerScheduler()
        {
            this.settings = Settings.Default;
        }

        public async Task<RequestId> RunAsync(IEnumerable<ComputeRequest> requests)
        {

            var requestId = new RequestId(Guid.NewGuid().ToString());

            logger.Info("Running with the following settings: ");

            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(
               new StorageCredentials(
                   this.settings.StorageAccountName,
                   this.settings.StorageAccountKey),
               this.settings.StorageServiceUrl,
               useHttps: true);

            //Generate a SAS for the container.
            string containerSasUrl = ConstructContainerSas(
                cloudStorageAccount,
                this.settings.BlobContainer, true);

            //Set up the Batch Service credentials used to authenticate with the Batch Service.
            BatchSharedKeyCredentials credentials = new BatchSharedKeyCredentials(
                this.settings.BatchServiceUrl,
                this.settings.BatchAccountName,
                this.settings.BatchAccountKey);

            using (BatchClient batchClient = await BatchClient.OpenAsync(credentials))
            {
                CloudJob xpricerJob = CreateJob(batchClient, requestId.Id, Constants.XPricerPool);
                xpricerJob.PoolInformation = new PoolInformation() { PoolId = settings.PoolID };

                List<CloudTask> tasksToRun = new List<CloudTask>();
                foreach (ComputeRequest cr in requests)
                {
                    VanillaOption Vanilla = cr.Product as VanillaOption;
                    if (Vanilla != null) {
                        String requestBlobFile = UploadRequestToBlob(cr, Vanilla.Underlying, cloudStorageAccount, this.settings.BlobContainer);
                        CloudTask task = new CloudTask("xpricer_task_" + Vanilla.Underlying , String.Format("{0} {1}",
                        "cmd /c %AZ_BATCH_APP_PACKAGE_XPRICER%\\xpricer.exe -args",    
                        requestBlobFile,
                        containerSasUrl));

                    task.ApplicationPackageReferences = new List<ApplicationPackageReference>
                    {
                        new ApplicationPackageReference
                        {
                            ApplicationId = settings.ApplicationPackageName,
                            Version = settings.ApplicationPackageVersion 
                        }
                    };
                    tasksToRun.Add(task);
                    }
                }

                //Add tasks to the Job
                batchClient.JobOperations.AddTask(requestId.Id, tasksToRun);
            }

            return requestId;
        }

        private string UploadRequestToBlob(ComputeRequest cr, String filename, CloudStorageAccount storageAccount, String containerName)
        {
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            CloudBlockBlob blob = container.GetBlockBlobReference(filename);
            blob.DeleteIfExists();

            var options = new BlobRequestOptions()
            {
                ServerTimeout = TimeSpan.FromMinutes(10)
            };

            var crJson = JsonConvert.SerializeObject(cr);
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(crJson), false))
            {
                blob.UploadFromStream(stream, null, options);
            }
            return blob.Uri.AbsoluteUri;
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
            string containerName, bool tokenOnly)
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
            if (tokenOnly)
            {
                return sasString;
            }
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
