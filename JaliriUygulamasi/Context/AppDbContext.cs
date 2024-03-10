using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Nest;

namespace JaliriUygulamasi.Context;
public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Data Source=LAPTOP-HI735739\\SQLEXPRESS;Initial Catalog=JaliriDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
    }
    public DbSet<Product> Products { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var addedEntities = ChangeTracker.Entries<Product>().Where(e => e.State == EntityState.Added).Select(e => e.Entity).ToList();
        var updatedEntities = ChangeTracker.Entries<Product>().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToList();
        var deletedEntities = ChangeTracker.Entries<Product>().Where(e => e.State == EntityState.Deleted).Select(e => e.Entity).ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        var node = new Uri("http://localhost:9200");
        var settings = new ConnectionSettings(node).DefaultIndex("products");
        var client = new ElasticClient(settings);
        #region Case 1   
        //var tasks = new List<Task>();
        //foreach (var entity in addedEntities) tasks.Add(client.IndexDocumentAsync(entity));
        //foreach (var entity in updatedEntities) tasks.Add(client.UpdateAsync<Product>(entity.Id, u => u.Doc(entity).Index("products")));
        //foreach (var entity in deletedEntities) tasks.Add(client.DeleteAsync<Product>(entity.Id, d => d.Index("products")));
        //await Task.WhenAll(tasks); 
        #endregion
        #region Case 2
        var bulkDescriptor = new BulkDescriptor();
        foreach (var entity in addedEntities) bulkDescriptor.Index<Product>(op => op.Document(entity).Index("products"));
        foreach (var entity in updatedEntities) bulkDescriptor.Update<Product>(op => op.Id(entity.Id).Doc(entity).Index("products"));
        foreach (var entity in deletedEntities) bulkDescriptor.Delete<Product>(op => op.Id(entity.Id).Index("products"));
        await client.BulkAsync(bulkDescriptor);
        #endregion
        return result;
    }
}

