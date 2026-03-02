# Amazon S3 CRUD Demo with .NET

This application demonstrates basic CRUD (Create, Read, Update, Delete) operations and listing of objects using the AWS SDK for .NET with Amazon S3.

## Prerequisites

- .NET 6.0 or later
- AWS account with access to S3
- AWS credentials configured locally

## Configuration

Before running the application, update the `appsettings.json` file with:

1. Your preferred AWS region
2. Your S3 bucket name (it will be created if it doesn't exist)
3. Make sure you have AWS credentials configured (using AWS CLI or credentials file)

## AWS Credentials Setup

You can set up AWS credentials using one of these methods:
- AWS CLI: Run `aws configure`
- Create or edit `~/.aws/credentials` manually
- Use environment variables
- Use IAM roles (if running on AWS services like EC2)

## Features

This demo application can:
- Create (upload) objects to S3
- Read (download) objects from S3  
- Update objects in S3
- Delete objects from S3
- List objects in an S3 bucket

## Building and Running

```bash
cd dotnet-demo-s3
dotnet build
dotnet run
```

## Key Components

- `S3Service.cs`: Contains all the S3 operations logic
- `Program.cs`: Console application with menu for demonstrating S3 operations
- `appsettings.json`: Configuration for AWS and S3 settings
