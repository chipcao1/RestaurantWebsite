using System;
using System.IO;
using System.IO.Compression;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Xml;
using RestaurantWebsite.Models;
class SnapshotContext : AppDbContext
{
    public void Configure(DbModelBuilder builder) { base.OnModelCreating(builder); }
}
class Program
{
    static void Main(string[] args)
    {
        var builder = new DbModelBuilder();
        // Match DbContext's DbSet entity-set naming, including Combos/Comboes.
        foreach (var property in typeof(AppDbContext).GetProperties())
            if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            {
                var type = property.PropertyType.GetGenericArguments()[0];
                var configuration = typeof(DbModelBuilder).GetMethod("Entity").MakeGenericMethod(type).Invoke(builder, null);
                configuration.GetType().GetMethod("HasEntitySetName").Invoke(configuration, new object[] { property.Name });
            }
        using (var context = new SnapshotContext()) context.Configure(builder);
        var model = builder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
        using (var memory = new MemoryStream())
        {
            using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
            using (var xml = XmlWriter.Create(gzip)) EdmxWriter.WriteEdmx(model, xml);
            File.WriteAllText(args[0], Convert.ToBase64String(memory.ToArray()));
        }
        Console.WriteLine("Snapshot generated from actual EF6 model");
    }
}
