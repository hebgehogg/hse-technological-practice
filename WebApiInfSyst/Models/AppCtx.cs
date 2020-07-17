using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiInfSyst.Models
{
    public class AppCtx : DbContext
    {
        public AppCtx(DbContextOptions<AppCtx> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public virtual DbSet<Games> Games { get; set; }
        public virtual DbSet<GemesPrices> GemesPrices { get; set; }
        public virtual DbSet<GameGenre> GameGenre { get; set; }
        public virtual DbSet<Genres> Genres { get; set; }
        public virtual DbSet<Inventory> Inventory { get; set; }
        public virtual DbSet<InventoryIPrices> InventoryIPrices { get; set; }
        public virtual DbSet<Wallpapers> Wallpapers { get; set; }
        public virtual DbSet<WallpaperPrice> WallpaperPrice { get; set; }
        public virtual DbSet<Client> Client { get; set; }
        public virtual DbSet<Staff> Staff { get; set; }
        public virtual DbSet<ClientsGames> ClientsGames { get; set; }
        public virtual DbSet<ClientsInventory> ClientsInventory { get; set; }
        public virtual DbSet<ClientStatus> ClientStatus { get; set; }
        public virtual DbSet<ClientsWallpapers> ClientsWallpapers { get; set; }
        public virtual DbSet<Purchases> Purchases { get; set; }
    }
}
