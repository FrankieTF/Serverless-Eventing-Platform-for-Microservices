﻿using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContentReactor.Shared.BlobHelper
{
    public class BlobHelper
    {
        private static readonly BlobServiceClient BlobServiceClient;
        private static readonly string ServiceUrl = Environment.GetEnvironmentVariable("CONTENT_REACTOR_STORAGE_BLOB_SERVICE_URI");
        private static readonly string StorageAccountName = Environment.GetEnvironmentVariable("CONTENT_REACTOR_STORAGE_ACCOUNT_NAME");
        private static readonly string StorageAccountKey = Environment.GetEnvironmentVariable("CONTENT_REACTOR_STORAGE_KEY");

        static BlobHelper()
        {
            // connect to Azure Storage
            StorageSharedKeyCredential credential = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);
            BlobServiceClient = new BlobServiceClient(new Uri(ServiceUrl), credential);
        }

        public BlockBlobClient GetBlobClient(string containerName, string blobId)
        {
            var container = BlobServiceClient.GetBlobContainerClient(containerName);

            var blob = container.GetBlockBlobClient(blobId);

            return blob;
        }
        public BlobContainerClient GetContainerClient(string containerName)
        {
            var container = BlobServiceClient.GetBlobContainerClient(containerName);
            return container;
        }



        public async Task<IList<BlobItem>> ListBlobsAsync(string containerName)
        {
            var container = BlobServiceClient.GetBlobContainerClient(containerName);

            var blobs = new List<BlobItem>();
            blobs = await container.GetBlobsAsync().ToListAsync();

            return blobs;
        }

        public string GetSasUriForBlob(BlockBlobClient blob, BlobSasPermissions blobSasPermissions)
        {
            // Create a SharedKeyCredential that we can use to sign the SAS token
            var credential = new StorageSharedKeyCredential(StorageAccountName, StorageAccountKey);

            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                StartsOn = DateTime.UtcNow.AddMinutes(-5), // to allow for clock skew
                ExpiresOn = DateTime.UtcNow.AddHours(24),
            };
            sasBuilder.SetPermissions(blobSasPermissions);
            // Build a URI + SASToken string
            UriBuilder sasUri = new UriBuilder(blob.Uri)
            {
                Query = sasBuilder.ToSasQueryParameters(credential).ToString()
            };

            return sasUri.Uri.AbsoluteUri;
        }
    }

    public static class Extentions
    {
        public static async Task<List<BlobItem>> ToListAsync(this IAsyncEnumerable<BlobItem> items)
        {
            List<BlobItem> blobItemsList = new List<BlobItem>();
            await foreach (var item in items)
            {
                blobItemsList.Add(item);
            }
            return blobItemsList;
        }
    }
}
