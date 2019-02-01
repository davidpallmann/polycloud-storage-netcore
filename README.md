# polycloud-storage
PolyCloud-API library for cloud storage **.NET Core edition** | [.NET Framework Edition](https://github.com/davidpallmann/polycloud-storage)

![PolyCloud-API-logo](https://s3.amazonaws.com/david-pallmann-public/PolyCloud-small.png)

__PolyCloud-API__ is an API initiative that provides unified libraries for accessing AWS, Azure, and Google cloud platforms and performing common operations.

__PolyCloud-Storage__ is the PolyCloud-API library for cloud storage; that is, files and folders. This library's initial goals are modest: to provide an easy way to perform common storage operations that exist on all three cloud platforms.

We never want your data to be lost or your credentials to be compromised. Please be aware that this is a new (alpha) library and as such you should not use it for Production data unless you have become comfortable working with it. Please exercise caution.

# .NET Core Edition

The .NET Core Edition uses .NET Core 2.1.

## Dependencies

The .NET Framework edition of PolyCloud-Storage uses these nuget libraries:

* Amazon Web Services: AWSSDK.Core and dependent libraries like AWSSDK.S3
* Microsoft Azure: Microsoft.Azure.Storage.Blob
* Google Cloud Platform: Google.Cloud.Storage.v1 and dependent libraries

## Documentation

[Read the API documentation](https://github.com/davidpallmann/polycloud-storage/wiki/API-Documentation---.NET-Framework)

## Building

To build PolyCloud.Storage for .NET Core you need Visual Studio 2017 Community or higher.

The source code includes these projects:
* PolyCloud.Storage.NetCore: the .NET Core library
* PolyCloud.Storage.Tests.NetCore: unit tests that can be run in Visual Studio Test Explorer (before running, edit to supply your storage account credentials)

## Collaboration

Collaborators are welcome: I'd like to see PolyCloud-API libraries come to exist for multiple development environments. If you're interested in helping out, please contact [David Pallmann](http://davidpallmann.com).
