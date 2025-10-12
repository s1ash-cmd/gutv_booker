using Microsoft.EntityFrameworkCore;
using gutv_booker.Models;

namespace gutv_booker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<EquipmentItem> EquipmentItems { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EquipmentItem>()
                .HasIndex(e => e.InventoryNumber)
                .IsUnique();

            modelBuilder.Entity<BookingItem>()
                .HasOne(bi => bi.EquipmentItem)
                .WithMany(e => e.BookingItems)
                .HasForeignKey(bi => bi.EquipmentItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingItem>()
                .HasOne(bi => bi.Booking)
                .WithMany(b => b.BookingItems)
                .HasForeignKey(bi => bi.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}