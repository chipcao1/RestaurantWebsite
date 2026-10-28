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
        foreach (var type in typeof(User).Assembly.GetTypes())
            if (type.Namespace == "RestaurantWebsite.Models" && type.IsClass && type.IsPublic && !type.IsNested && type != typeof(AppDbContext))
                typeof(DbModelBuilder).GetMethod("Entity").MakeGenericMethod(type).Invoke(builder, null);
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
