using System.Security.Claims;
using FINALPROJECT.model;
using Microsoft.EntityFrameworkCore;
namespace FINALPROJECT.Data
{
    public class DataCountextEf : DbContext
    {
        private readonly IConfiguration _conictionString;

        public DataCountextEf(IConfiguration config)
        {
            _conictionString = config;
        }
        public virtual DbSet<Users>? User { get; set; }
        public virtual DbSet<Cars>? Cars { get; set; }
        public virtual DbSet<CarsForRent>? CarsForRent { get; set; }
        public virtual DbSet<RentRequest>? RentRequest { get; set; }
        public virtual DbSet<Payment>? Payment { get; set; }
        public virtual DbSet<saleRequest>? saleRequest { get; set; }
        public virtual DbSet<Maintenance>? Maintenance { get; set; }
        public virtual DbSet<MaintenanceDetales>? MaintenanceDetales { get; set; }
        public virtual DbSet<SparePart>? SparePart { get; set; }
        public virtual DbSet<Basket>? Basket { get; set; }
        public virtual DbSet<CarForRentImages>? CarForRentImages { get; set; }
        public virtual DbSet<CarForSaleImage>? CarForSaleImage { get; set; }
        public virtual DbSet<Purchases>? Purchases { get; set; }











        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (!options.IsConfigured)
            {
                options.UseSqlServer(_conictionString.GetConnectionString("DefaultConnection"),
               options => options.EnableRetryOnFailure()
                );
            }
        }
        protected override void OnModelCreating(ModelBuilder ModelBuilder)
        {
            ModelBuilder.HasDefaultSchema("FinalProject");
            ModelBuilder.Entity<Users>().ToTable("UserInfo", "FinalProject").HasKey(U => U.UserID);
            ModelBuilder.Entity<Cars>().ToTable("SalesCars", "FinalProject").HasKey(U => U.CarID);
            ModelBuilder.Entity<CarsForRent>().ToTable("RentCar", "FinalProject").HasKey(U => U.carid);
            ModelBuilder.Entity<RentRequest>().ToTable("TimeRent", "FinalProject").HasKey(U => U.rentid);
            ModelBuilder.Entity<Payment>().ToTable("payment_info", "FinalProject").HasKey(U => U.payment_id);
            ModelBuilder.Entity<saleRequest>().ToTable("saleRequest", "FinalProject").HasKey(U => U.saleid);
            ModelBuilder.Entity<Maintenance>().ToTable("maintenance", "FinalProject").HasKey(U => U.maintenance_id);
            ModelBuilder.Entity<MaintenanceDetales>().ToTable("maintenance_details", "FinalProject").HasKey(U => U.detail_id);
            ModelBuilder.Entity<SparePart>().ToTable("SpareParts", "FinalProject").HasKey(U => U.SpareId);
            ModelBuilder.Entity<Basket>().ToTable("basket", "FinalProject").HasKey(U => U.basketId);
            ModelBuilder.Entity<CarForRentImages>().ToTable("CarForRentImages", "FinalProject").HasKey(U => U.ImageId);
            ModelBuilder.Entity<CarForSaleImage>().ToTable("CarForSaleImage", "FinalProject").HasKey(U => U.ImageId);
            ModelBuilder.Entity<Purchases>().ToTable("Purchases", "FinalProject").HasKey(U => U.PurchaseID);






        }

        internal ClaimsPrincipal ToList()
        {
            throw new NotImplementedException();
        }
    }
}