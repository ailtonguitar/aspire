// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Publishing;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding MongoDB resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class MongoDBBuilderExtensions
{
    private const int DefaultContainerPort = 27017;

    /// <summary>
    /// Adds a MongoDB container to the application model. The default image is "mongo" and the tag is "latest".
    /// </summary>
    /// <returns></returns>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="port">The host port for MongoDB.</param>
    /// <param name="password">The password for the MongoDB root user. Defaults to a random password.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{MongoDBContainerResource}"/>.</returns>
    public static IResourceBuilder<MongoDBContainerResource> AddMongoDBContainer(
        this IDistributedApplicationBuilder builder,
        string name,
        int? port = null,
        string? password = null)
    {
        var mongoDBContainer = new MongoDBContainerResource(name);

        return builder
            .AddResource(mongoDBContainer)
            .WithManifestPublishingCallback(WriteMongoDBContainerToManifest)
            .WithAnnotation(new ServiceBindingAnnotation(ProtocolType.Tcp, port: port, containerPort: DefaultContainerPort)) // Internal port is always 27017.
            .WithAnnotation(new ContainerImageAnnotation { Image = "mongo", Tag = "latest" });
    }

    /// <summary>
    /// Adds a MongoDB connection to the application model. Connection strings can also be read from the connection string section of the configuration using the name of the resource.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="connectionString">The MongoDB connection string (optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{MongoDBConnectionResource}"/>.</returns>
    public static IResourceBuilder<MongoDBConnectionResource> AddMongoDBConnection(this IDistributedApplicationBuilder builder, string name, string? connectionString = null)
    {
        var mongoDBConnection = new MongoDBConnectionResource(name, connectionString);

        return builder
            .AddResource(mongoDBConnection)
            .WithManifestPublishingCallback(context => context.WriteMongoDBConnectionToManifest(mongoDBConnection));
    }

    /// <summary>
    /// Adds a MongoDB database to the application model.
    /// </summary>
    /// <param name="builder">The MongoDB server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{MongoDBDatabaseResource}"/>.</returns>
    public static IResourceBuilder<MongoDBDatabaseResource> AddDatabase(this IResourceBuilder<MongoDBContainerResource> builder, string name)
    {
        var mongoDBDatabase = new MongoDBDatabaseResource(name, builder.Resource);

        return builder.ApplicationBuilder
            .AddResource(mongoDBDatabase)
            .WithManifestPublishingCallback(context => context.WriteMongoDBDatabaseToManifest(mongoDBDatabase));
    }

    private static void WriteMongoDBContainerToManifest(this ManifestPublishingContext context)
    {
        context.Writer.WriteString("type", "mongodb.server.v0");
    }

    private static void WriteMongoDBConnectionToManifest(this ManifestPublishingContext context, MongoDBConnectionResource mongoDbConnection)
    {
        context.Writer.WriteString("type", "mongodb.connection.v0");
        context.Writer.WriteString("connectionString", mongoDbConnection.GetConnectionString());
    }

    private static void WriteMongoDBDatabaseToManifest(this ManifestPublishingContext context, MongoDBDatabaseResource mongoDbDatabase)
    {
        context.Writer.WriteString("type", "mongodb.database.v0");
        context.Writer.WriteString("parent", mongoDbDatabase.Parent.Name);
    }
}