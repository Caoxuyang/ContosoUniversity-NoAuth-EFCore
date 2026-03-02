using Amazon.S3;
using Amazon.S3.Model;
using dotnet_demo_s3;

// Configure AWS services
string bucketName = "demo-bucket-name";
string region = "us-east-1";

// Create S3 client
var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(region));
var s3Service = new S3Service(s3Client, bucketName);

Console.WriteLine("S3 CRUD Demo Application");
Console.WriteLine("=======================");

bool exit = false;
while (!exit)
{
    Console.WriteLine("\nSelect an operation:");
    Console.WriteLine("1. Upload a text object");
    Console.WriteLine("2. Download an object");
    Console.WriteLine("3. Update an object");
    Console.WriteLine("4. Delete an object");
    Console.WriteLine("5. List objects");
    Console.WriteLine("6. Exit");
    Console.Write("\nYour choice: ");

    if (int.TryParse(Console.ReadLine(), out int choice))
    {
        switch (choice)
        {
            case 1: // Upload
                Console.Write("Enter object key (name): ");
                string uploadKey = Console.ReadLine() ?? "sample.txt";
                Console.Write("Enter text content: ");
                string content = Console.ReadLine() ?? "Sample content";
                await s3Service.UploadStringAsync(content, uploadKey);
                break;

            case 2: // Download
                Console.Write("Enter object key to download: ");
                string downloadKey = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(downloadKey))
                {
                    string downloadedContent = await s3Service.DownloadObjectAsStringAsync(downloadKey);
                    if (!string.IsNullOrEmpty(downloadedContent))
                    {
                        Console.WriteLine($"Content: {downloadedContent}");
                    }
                }
                break;

            case 3: // Update
                Console.Write("Enter object key to update: ");
                string updateKey = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(updateKey))
                {
                    Console.Write("Enter new content: ");
                    string newContent = Console.ReadLine() ?? "Updated content";
                    await s3Service.UpdateObjectAsync(newContent, updateKey);
                }
                break;

            case 4: // Delete
                Console.Write("Enter object key to delete: ");
                string deleteKey = Console.ReadLine() ?? "";
                if (!string.IsNullOrEmpty(deleteKey))
                {
                    await s3Service.DeleteObjectAsync(deleteKey);
                }
                break;

            case 5: // List
                Console.Write("Enter prefix (optional): ");
                string prefix = Console.ReadLine() ?? "";
                List<S3Object> objects = await s3Service.ListObjectsAsync(prefix);
                Console.WriteLine("\nObjects in bucket:");
                foreach (var obj in objects)
                {
                    Console.WriteLine($"- {obj.Key} (Size: {obj.Size} bytes, Last Modified: {obj.LastModified})");
                }
                break;

            case 6: // Exit
                exit = true;
                break;

            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
    else
    {
        Console.WriteLine("Invalid input. Please enter a number.");
    }
}

Console.WriteLine("Thank you for using the S3 CRUD Demo!");
