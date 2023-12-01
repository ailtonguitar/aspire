// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a MongoDB database. This is a child resource of a <see cref="MongoDBContainerResource"/>.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="mongoDBContainer">The MongoDB server resource associated with this database.</param>
public class MongoDBDatabaseResource(string name, MongoDBContainerResource mongoDBContainer)
    : Resource(name), IMongoDBResource, IResourceWithParent<MongoDBContainerResource>
{
    public MongoDBContainerResource Parent => mongoDBContainer;

    /// <summary>
    /// Gets the connection string for the MongoDB database.
    /// </summary>
    /// <returns>A connection string for the MongoDB database.</returns>
    public string? GetConnectionString()
    {
        if (Parent.GetConnectionString() is { } connectionString)
        {
            var builder = new StringBuilder(connectionString);

            if (!connectionString.EndsWith('/'))
            {
                builder.Append('/');
            }

            builder.Append(Name);

            return builder.ToString();
        }

        throw new DistributedApplicationException("Parent resource connection string was null.");
    }
}
