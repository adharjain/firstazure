using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace simple.core.net.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AwsController :   Controller
    {
        private readonly IAmazonS3 _client;

        private readonly IConfiguration config;
        public AwsController(IConfiguration _configuration)
        {
            config = _configuration;

            var awsaccess = config.GetValue<string>("AWSSDK:AccessKey");
            var awsSecret = config.GetValue<string>("AWSSDK:SecretKey");
            _client = new AmazonS3Client(awsaccess, awsSecret, RegionEndpoint.USWest1);

        }
        [HttpGet("List-Buckets")]
        public async Task<IActionResult> GetBuckets()
        {           
            try
            {
                var result = await _client.ListBucketsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest("S3 didnt work" + ex.Message);
            }
        }

        [HttpPost("create-bucket/{name}")]
        public async Task<IActionResult> CreateBucket(string name)
        {
            try
            {
                PutBucketRequest request = new PutBucketRequest();
                request.BucketName = name;
                await _client.PutBucketAsync(request);
                return Ok("S3 bucket " + name + " created");
            }
            catch (Exception ex)
            {
                return BadRequest("S3 didnt work" + ex.Message);
            }
        }

        [HttpPost("Enable-Versioning/{name}")]
        public async Task<IActionResult> EnableVersioning(string name)
        {
            try
            {
                PutBucketVersioningRequest request = new PutBucketVersioningRequest()
                {
                    BucketName = name,
                    VersioningConfig = new S3BucketVersioningConfig
                    {
                        Status = VersionStatus.Enabled
                    }
                };
                await _client.PutBucketVersioningAsync(request);
                return Ok("bucket " + name + " versioning enabled");
            }
            catch (Exception ex)
            {
                return BadRequest("S3 versioning not enabled" + ex.Message);
            }
        }

        [HttpPost("Create-folder/{bucketname}/{foldername}")]
        public async Task<IActionResult> CreateFolder(string bucketname, string foldername)
        {
            try
            {
                PutObjectRequest request = new PutObjectRequest()
                {
                    BucketName = bucketname,                
                    Key = foldername.Replace("%2F","/")
                };
                await _client.PutObjectAsync(request);
                return Ok("folder " + foldername + " created in bucket: "+ bucketname);
            }
            catch (Exception ex)
            {
                return BadRequest("Folder not created. Error: " + ex.Message);
            }
        }

        [HttpPost("Create-Object/{bucketname}/{foldername}")]
        public async Task<IActionResult> CreateObject(string bucketname, string objectName)
        {
            try
            {
                FileInfo fi = new FileInfo(@"C:\Adhar\Projects\simple.core.net.api\simple.core.net.api\Testfile.txt");
                PutObjectRequest request = new PutObjectRequest()
                {
                    InputStream = fi.OpenRead(),
                    BucketName = bucketname,
                    Key = "testfile.txt",
                    ContentType = "text/plain",
                    //ContentBody = "This is my first file"  

                };
                await _client.PutObjectAsync(request);
                await GetListofObjects(bucketname);

                return Ok("Object " + objectName + " created in bucket: " + bucketname);
            }
            catch (Exception ex)
            {
                return BadRequest("Folder not created. Error: " + ex.Message);
            }
        }

        private async Task GetListofObjects(string bucketname)
        {
            ListObjectsRequest r = new ListObjectsRequest()
            {
                BucketName = bucketname
            };
            ListObjectsResponse response = await _client.ListObjectsAsync(r);
        }

        [HttpPost("delete-bucket/{bucketname}")]
        public async Task<IActionResult> DeleteBucket(string bucketname)
        {
            try
            {
                DeleteBucketRequest request = new DeleteBucketRequest()
                {
                    BucketName = bucketname
                };
                await _client.DeleteBucketAsync(request);
                return Ok("Bucket: " + bucketname + " deleted");
            }
            catch (Exception ex)
            {
                return BadRequest("Not deleted bucket. Error: " + ex.Message);
            }
        }

        [HttpPost("copy-file/{sourceBucket}/{destinationBucket}/{sourceKey}/{destinationkey}")]
        public async Task<IActionResult> CopyFile(string sourceBucket, string destinationBucket, string sourceKey, string destinationkey)
        {
            try
            {
                CopyObjectRequest request = new CopyObjectRequest()
                {
                    SourceBucket = sourceBucket,
                    DestinationBucket = destinationBucket,
                    SourceKey = sourceKey,
                    DestinationKey = destinationkey
                };
                await _client.CopyObjectAsync(request);

                return Ok("File copied from: " + sourceBucket + " to " + destinationBucket);
            }
            catch (Exception ex)
            {
                return BadRequest("File not copied. Error: " + ex.Message);
            }
        }
    }
}
