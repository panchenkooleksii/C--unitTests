using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using SimpleTradingApp.Models;

namespace SimpleTradingApp;

internal class TradingAppDbContext : DbContext, IDbContext
{
    public DbSet<Trade> Trades { get; init; }

    public static TradingAppDbContext Create()
    {
        var connectionString = "mongodb+srv://username:password@cluster0.non-existing.mongodb.net/?retryWrites=true&w=majority";
        var client = new MongoClient(connectionString);
        IMongoDatabase database = client.GetDatabase("trading_app_db");
        var builder = new DbContextOptionsBuilder<TradingAppDbContext>().UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName);
        return new(builder.Options);
    }

    public TradingAppDbContext(DbContextOptions options)
        : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Trade>().ToCollection("trades");
    }
}