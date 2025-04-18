using AIBookStreet.Repositories.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIBookStreet.Repositories.Data
{
    public class BSDbContext : BaseDbContext
    {
        public BSDbContext(DbContextOptions<BSDbContext> options) : base(options) 
        {
        }

        #region Config
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }
        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var strConn = config.GetConnectionString("AIBookStreet");

            return strConn ?? "";
        }
        #endregion

        #region Dbset

        public virtual DbSet<Author> Authors { get; set; } = null!;
        public virtual DbSet<Book> Books { get; set; } = null!;
        public virtual DbSet<Store> Stores { get; set; } = null!;
        public virtual DbSet<Street> BookStreets { get; set; } = null!;
        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Event> Events { get; set; } = null!;
        public virtual DbSet<Publisher> Publishers { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Zone> Zones { get; set; } = null!;
        public virtual DbSet<Image> Images { get; set; } = null!;
        public virtual DbSet<Person> Persons { get; set; } = null!;

        public virtual DbSet<BookAuthor> BookAuthors { get; set; } = null!;
        public virtual DbSet<BookCategory> BookCategories { get; set; } = null!;
        public virtual DbSet<Inventory> Inventories { get; set; } = null!;
        public virtual DbSet<UserRole> UserRoles { get; set; } = null!;
        public virtual DbSet<UserStore> UserStores { get; set; } = null!;
        public virtual DbSet<Souvenir> Souvenirs { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;
        public virtual DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public virtual DbSet<StoreSchedule> StoreSchedules { get; set; } = null!;
        public virtual DbSet<EventRegistration> EventRegistrations { get; set; } = null!;
        public virtual DbSet<Ticket> Tickets { get; set; } = null!;
        #endregion  


        #region Model Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<BaseEntity>(e =>
            //{
            //    e.HasKey(x => x.Id);
            //    e.Property(x => x.Id).ValueGeneratedOnAdd().HasDefaultValueSql("NEWID()");
            //    e.Property(x => x.CreatedBy).IsRequired(false);
            //    e.Property(x => x.CreatedDate).IsRequired();
            //    e.Property(x => x.LastUpdatedBy).IsRequired(false);
            //    e.Property(x => x.LastUpdatedDate).IsRequired(false);
            //    e.Property(x => x.IsDeleted).HasDefaultValue(false);
            //});

            modelBuilder.Entity<Person>(e =>
            {
                e.ToTable("Persons");
                e.Property(x => x.ExternalId).IsRequired();
                e.Property(x => x.Gender).HasMaxLength(10).IsRequired();
                e.Property(x => x.Features).IsRequired();
                e.Property(x => x.FirstSeen).IsRequired();
                e.Property(x => x.LastSeen).IsRequired();
                e.Property(x => x.DailyAppearances).IsRequired();
                e.Property(x => x.TotalAppearances).IsRequired();
                e.Property(x => x.ExternalCreatedAt).IsRequired();
                e.Property(x => x.ExternalUpdatedAt).IsRequired();
                
                // Add a unique index on ExternalId
                e.HasIndex(x => x.ExternalId).IsUnique();
            });

            modelBuilder.Entity<Author>(e =>
            {
                e.ToTable("Authors");
                e.Property(x => x.AuthorName).HasMaxLength(255).IsRequired();
                e.Property(x => x.DOB).IsRequired(false);
                e.Property(x => x.Nationality).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.Biography).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-n voi bookAuthor
                e.HasMany(x => x.BookAuthors)
                    .WithOne(x => x.Author)
                    .HasForeignKey(x => x.AuthorId)
                    .OnDelete(DeleteBehavior.Cascade); // cascade: neu author bi xoa, lien ket bookAuthor cung bi xoa

                // 1-n voi image
                e.HasMany(at => at.Images)
                    .WithOne(i => i.Author)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu author bi xoa, image lien quan cung bi xoa
            });

            modelBuilder.Entity<Book>(e =>
            {
                e.ToTable("Books");
                e.Property(x => x.ISBN).IsRequired().HasMaxLength(50);
                e.Property(x => x.Title).HasMaxLength(255).IsRequired(false);
                e.Property(x => x.PublicationDate).IsRequired(false);
                e.Property(x => x.Price).IsRequired(false).HasColumnType("decimal(18,2)"); ;
                e.Property(x => x.Languages).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
                e.Property(x => x.Size).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.Status).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-1 voi publisher
                e.HasOne(x => x.Publisher)
                 .WithMany(p => p.Books)
                 .HasForeignKey(x => x.PublisherId)
                 .OnDelete(DeleteBehavior.SetNull); // neu publisher bi xoa, publisherId set null thay vi xoa book

                // 1-n voi bookAuthor
                e.HasMany(x => x.BookAuthors)
                 .WithOne(x => x.Book)
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Cascade); // cascade: neu book bi xoa, lien ket bookAuthor cung bi xoa

                // 1-n voi image
                e.HasMany(b => b.Images)
                    .WithOne(i => i.Book)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu book bi xoa, image lien quan cung bi xoa

                // 1-n voi inventory
                e.HasMany(b => b.Inventories)
                    .WithOne(i => i.Book)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu book bi xoa, inventory lien quan cung bi xoa
            });

            modelBuilder.Entity<BookAuthor>(e =>
            {
                e.ToTable("BookAuthors");
                e.HasKey(x => new { x.BookId, x.AuthorId });

                // n-1 voi book
                e.HasOne(x => x.Book)
                 .WithMany(b => b.BookAuthors)
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Cascade); // neu book bi xoa, bookAuthor cung bi xoa

                // n-1 voi author
                e.HasOne(x => x.Author)
                 .WithMany(a => a.BookAuthors)
                 .HasForeignKey(x => x.AuthorId)
                 .OnDelete(DeleteBehavior.Cascade); // neu author bi xoa, bookAuthor cung bi xoa
            });

            modelBuilder.Entity<BookCategory>(e =>
            {
                e.ToTable("BookCategories");
                e.HasKey(x => new { x.BookId, x.CategoryId });

                // n-1 voi book
                e.HasOne(x => x.Book)
                 .WithMany(b => b.BookCategories)
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Cascade); // neu book bi xoa, bookCategory cung bi xoa

                // n-1 voi category
                e.HasOne(x => x.Category)
                 .WithMany(c => c.BookCategories)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade); // category bi xoa. bookCategory cung bi xoa
            });

            modelBuilder.Entity<Store>(e =>
            {
                e.ToTable("Stores");
                e.Property(x => x.StoreName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Address).HasMaxLength(500).IsRequired(false);
                e.Property(x => x.Phone).HasMaxLength(15).IsRequired(false);
                e.Property(x => x.Email).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.StoreTheme).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.Longitude).IsRequired(false).HasColumnType("decimal(18,6)");
                e.Property(x => x.Latitude).IsRequired(false).HasColumnType("decimal(18,6)");
                e.Property(x => x.Type).HasMaxLength(2000).IsRequired(false);

                // 1-n voi userStore
                e.HasMany(x => x.UserStores)
                 .WithOne(x => x.Store)
                 .HasForeignKey(x => x.StoreId)
                 .OnDelete(DeleteBehavior.Cascade); // neu store bi xoa, userStore lien quan bi xoa

                // 1-n voi zone
                e.HasOne(x => x.Zone)
                 .WithMany(x => x.Stores)
                 .HasForeignKey(x => x.ZoneId)
                 .OnDelete(DeleteBehavior.SetNull); // neu zone bi xoa, bookstore co zoneId = null

                // 1-n voi inventory
                e.HasMany(x => x.Inventories)
                 .WithOne(x => x.Store)
                 .HasForeignKey(x => x.StoreId)
                 .OnDelete(DeleteBehavior.Cascade); // neu bookStore bi xoa, inventory lien quan bi xoa

                // 1-n voi image
                e.HasMany(bs => bs.Images)
                    .WithOne(i => i.Store)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu bookStore bi xoa, image lien quan cung bi xoa
                e.HasMany(x => x.Orders)
                 .WithOne(z => z.Store)
                 .HasForeignKey(z => z.StoreId)
                 .OnDelete(DeleteBehavior.SetNull); // 
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("Categories");
                e.Property(x => x.CategoryName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);

                // 1-n voi bookCategory
                e.HasMany(x => x.BookCategories)
                 .WithOne(bc => bc.Category)
                 .HasForeignKey(bc => bc.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade); // category bi xoa, bookCategory lien quan bi xoa
            });

            modelBuilder.Entity<Event>(e =>
            {
                e.ToTable("Events");
                e.Property(x => x.EventName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
                e.Property(x => x.StartDate).IsRequired(false);
                e.Property(x => x.EndDate).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.VideoLink).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.IsOpen).IsRequired(true);
                e.Property(x => x.AllowAds).IsRequired(true);


                // n-1 voi zone
                e.HasOne(x => x.Zone)
                    .WithMany(s => s.Events)
                    .HasForeignKey(x => x.ZoneId)
                    .OnDelete(DeleteBehavior.SetNull); // neu zone bi xoa, zoneId cua event = null

                // 1-n voi image
                e.HasMany(e => e.Images)
                    .WithOne(i => i.Event)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu event bi xoa, image lien quan cung bi xoa

                e.HasMany(x => x.EventRegistrations)
                 .WithOne(z => z.Event)
                 .HasForeignKey(z => z.EventId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Inventory>(e =>
            {
                e.ToTable("Inventories");
                e.Property(x => x.Quantity).IsRequired();
                e.Property(x => x.IsInStock).IsRequired(false);

                ////n - 1 voi book
                //e.HasOne(x => x.Book)
                //    .WithMany(b => b.Inventories)
                //    .HasForeignKey(x => x.EntityId)
                //    .OnDelete(DeleteBehavior.Cascade); // neu book bi xoa, inventory lien quan bi xoa
                ////n - 1 voi souvenir
                //e.HasOne(x => x.Souvenir)
                //    .WithMany(b => b.Inventories)
                //    .HasForeignKey(x => x.EntityId)
                //    .OnDelete(DeleteBehavior.Cascade); // neu souvenir bi xoa, inventory lien quan bi xoa
                //n - 1 voi bookStore
                e.HasOne(x => x.Store)
                    .WithMany(bs => bs.Inventories)
                    .HasForeignKey(x => x.StoreId)
                    .OnDelete(DeleteBehavior.Cascade); // neu bookStore bi xoa, inventory lien quan bi xoa

                e.HasMany(i => i.OrderDetails)
                .WithOne(od => od.Inventory)
                .HasForeignKey(x => x.InventoryId)
                .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Publisher>(e =>
            {
                e.ToTable("Publishers");
                e.Property(x => x.PublisherName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Address).HasMaxLength(500).IsRequired(false);
                e.Property(x => x.Phone).HasMaxLength(15).IsRequired(false);
                e.Property(x => x.Email).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.Website).HasMaxLength(200).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-1 voi user
                e.HasOne(x => x.Manager)
                 .WithOne(u => u.Publisher)
                 .HasForeignKey<Publisher>(x => x.ManagerId)
                 .OnDelete(DeleteBehavior.SetNull); // khi xoa user, managerId = null

                // 1-n voi book
                e.HasMany(x => x.Books)
                 .WithOne(b => b.Publisher)
                 .HasForeignKey(b => b.PublisherId)
                 .OnDelete(DeleteBehavior.SetNull); // khi publisher bi xoa, publisherId cua book = null

                // 1-n voi image
                e.HasMany(p => p.Images)
                    .WithOne(i => i.Publisher)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu publisher bi xoa, image lien quan cung bi xoa
            });

            modelBuilder.Entity<Role>(e =>
            {
                e.ToTable("Roles");
                e.Property(x => x.RoleName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);

                // 1-n voi userRole
                e.HasMany(x => x.UserRoles)
                    .WithOne(ur => ur.Role)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade); // khi xoa role, userRole lien quan bi xoa
            });

            modelBuilder.Entity<Street>(e =>
            {
                e.ToTable("Streets");
                e.Property(x => x.StreetName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Address).HasMaxLength(500).IsRequired(false);
                e.Property(x => x.Description).HasMaxLength(2000).IsRequired(false);
                e.Property(x => x.Latitude).IsRequired(false).HasColumnType("decimal(18,6)");
                e.Property(x => x.Longitude).IsRequired(false).HasColumnType("decimal(18,6)");
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-n voi zone
                e.HasMany(x => x.Zones)
                 .WithOne(z => z.Street)
                 .HasForeignKey(z => z.StreetId)
                 .OnDelete(DeleteBehavior.SetNull); // neu xoa street, zone khong bi xoa (SetNull)


                // 1-n voi image
                e.HasMany(s => s.Images)
                    .WithOne(i => i.Street)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu street bi xoa, image lien quan cung bi xoa
            });

            modelBuilder.Entity<User>(e =>
            {
                e.ToTable("Users");
                e.Property(x => x.UserName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Password).HasMaxLength(255).IsRequired();
                e.Property(x => x.Email).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.FullName).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.DOB).IsRequired(false);
                e.Property(x => x.Address).HasMaxLength(100).IsRequired(false);
                e.Property(x => x.Phone).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.Gender).HasMaxLength(50).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-n voi userStore
                e.HasMany(x => x.UserStores)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // neu xoa user, userStore lien quan cung se xoa

                e.HasOne(x => x.Publisher)
                 .WithOne(x => x.Manager)
                 .HasForeignKey<Publisher>(x => x.ManagerId)
                 .OnDelete(DeleteBehavior.SetNull); // neu xoa user, publisher co managerId = null

                // 1-n voi userRole
                e.HasMany(x => x.UserRoles)
                    .WithOne(ur => ur.User)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // neu xoa user, userRole lien quan cung se xoa

                // 1-n voi image
                e.HasMany(u => u.Images)
                    .WithOne(i => i.User)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu user bi xoa, image lien quan cung bi xoa
            });

            modelBuilder.Entity<UserRole>(e =>
            {
                e.ToTable("UserRoles");
                e.HasKey(x => new { x.UserId, x.RoleId });

                // n-1 voi user
                e.HasOne(x => x.User)
                 .WithMany(u => u.UserRoles)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade); // xoa userRole khi user bi xoa

                // n-1 voi role
                e.HasOne(x => x.Role)
                 .WithMany(r => r.UserRoles)
                 .HasForeignKey(x => x.RoleId)
                 .OnDelete(DeleteBehavior.Cascade); // xoa userRole khi role bi xoa
            });

            modelBuilder.Entity<UserStore>(e =>
            {
                e.ToTable("UserStores");
                e.HasKey(us => new { us.UserId, us.StoreId });

                // n-1 voi user
                e.HasOne(us => us.User)
                 .WithMany(u => u.UserStores)
                 .HasForeignKey(us => us.UserId)
                 .OnDelete(DeleteBehavior.Cascade); // xoa userStore khi user bi xoa

                // n-1 voi store
                e.HasOne(us => us.Store)
                 .WithMany(s => s.UserStores)
                 .HasForeignKey(us => us.StoreId)
                 .OnDelete(DeleteBehavior.Cascade); // xoa userStore khi store bi xoa
            });

            modelBuilder.Entity<Zone>(e =>
            {
                e.ToTable("Zones");
                e.Property(x => x.ZoneName).HasMaxLength(255).IsRequired();
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
                e.Property(x => x.Longitude).IsRequired(false).HasColumnType("decimal(18,6)");
                e.Property(x => x.Latitude).IsRequired(false).HasColumnType("decimal(18,6)");

                // n-1 voi street
                e.HasOne(x => x.Street)
                    .WithMany(x => x.Zones)
                    .HasForeignKey(x => x.StreetId)
                    .OnDelete(DeleteBehavior.SetNull); // khi Street bi xoa, Zone co StreetId = null

                // 1-n voi bookStore
                e.HasMany(x => x.Stores)
                    .WithOne(x => x.Zone)
                    .HasForeignKey(x => x.ZoneId)
                    .OnDelete(DeleteBehavior.SetNull); // khi zone bi xoa, BookStore co ZoneId = null

                e.HasMany(x => x.Events)
                 .WithOne(ev => ev.Zone)
                 .HasForeignKey(ev => ev.ZoneId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Image>(e =>
            {
                e.ToTable("Images");
                e.Property(x => x.Url).HasMaxLength(1000).IsRequired(true);
                e.Property(x => x.Type).HasMaxLength(200).IsRequired(false);
                e.Property(x => x.AltText).HasMaxLength(200).IsRequired(true);

                //n-1 voi Author
                //e.HasOne(i => i.Author)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, author khong bi gi

                ////n-1 voi Event
                //e.HasOne(i => i.Event)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, event khong bi gi

                ////n-1 voi publisher
                //e.HasOne(i => i.Publisher)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, Publisher khong bi gi

                ////n-1 voi bookStore
                //e.HasOne(i => i.BookStore)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, bookStore khong bi gi

                ////n-1 voi street
                //e.HasOne(i => i.Street)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, street khong bi gi

                ////n-1 voi user
                //e.HasOne(i => i.User)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, user khong bi gi

                ////n-1 voi book
                //e.HasOne(i => i.Book)
                // .WithMany(at => at.Images)
                // .HasForeignKey(bc => bc.EntityId)
                // .OnDelete(DeleteBehavior.NoAction); // chi image bi xoa, book khong bi gi
            });

            modelBuilder.Entity<Souvenir>(e =>
            {
                e.ToTable("Souvenirs");
                e.Property(x => x.SouvenirName).HasMaxLength(255).IsRequired(false);
                e.Property(x => x.Price).IsRequired(false).HasColumnType("decimal(18,2)"); ;
                e.Property(x => x.Description).HasMaxLength(1000).IsRequired(false);
                e.Property(x => x.BaseImgUrl).HasMaxLength(2000).IsRequired(false);

                // 1-n voi image
                e.HasMany(b => b.Images)
                    .WithOne(i => i.Souvenir)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu souvenir bi xoa, image lien quan cung bi xoa

                // 1-n voi inventory
                e.HasMany(b => b.Inventories)
                    .WithOne(i => i.Souvenir)
                    .HasForeignKey(i => i.EntityId)
                    .OnDelete(DeleteBehavior.Cascade); // neu souvenir bi xoa, inventory lien quan cung bi xoa
            });
            modelBuilder.Entity<Order>(o =>
            {
                o.ToTable("Orders");
                o.Property(x => x.TotalAmount).IsRequired(false).HasColumnType("decimal(18, 2)");
                o.Property(x => x.PaymentMethod).IsRequired(true).HasMaxLength(2000);
                o.Property(x => x.Status).IsRequired(false).HasMaxLength(2000);

                o.HasMany(or => or.OrderDetails)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

                o.HasOne(or => or.Store)
                .WithMany(or => or.Orders)
                .HasForeignKey(or => or.StoreId)
                .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<OrderDetail>(e =>
            {
                e.ToTable("OrderDetails");
                e.Property(x => x.InventoryId).HasMaxLength(255).IsRequired(false);
                e.Property(x => x.OrderId).HasMaxLength(255).IsRequired(false);
                e.Property(x => x.Quantity).IsRequired();

                e.HasOne(x => x.Order)
                    .WithMany(x => x.OrderDetails)
                    .HasForeignKey(x => x.OrderId)
                    .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Inventory)
                    .WithMany(x => x.OrderDetails)
                    .HasForeignKey(x => x.InventoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<StoreSchedule>(e =>
            {
                e.ToTable("StoreSchedules");
                e.Property(x => x.DayOfWeek).IsRequired();
                e.Property(x => x.OpenTime).IsRequired();
                e.Property(x => x.CloseTime).IsRequired();
                e.Property(x => x.IsClosed).IsRequired();
                e.Property(x => x.SpecialDate).IsRequired(false);

                // 1-n với Store
                e.HasOne(x => x.Store)
                 .WithMany(x => x.StoreSchedules)
                 .HasForeignKey(x => x.StoreId)
                 .OnDelete(DeleteBehavior.Cascade); // store xoa, schedule cung bi xoa
            });
            modelBuilder.Entity<EventRegistration>(e =>
            {
                e.ToTable("EventRegistrations");
                e.Property(x => x.RegistrantName).HasMaxLength(255).IsRequired();
                e.Property(x => x.RegistrantEmail).HasMaxLength(255).IsRequired();
                e.Property(x => x.RegistrantPhoneNumber).HasMaxLength(255).IsRequired();
                e.Property(x => x.RegistrantAgeRange).HasMaxLength(255).IsRequired();
                e.Property(x => x.RegistrantGender).HasMaxLength(255).IsRequired();
                e.Property(x => x.RegistrantAddress).HasMaxLength(255).IsRequired(false);
                e.Property(x => x.ReferenceSource).HasMaxLength(1000).IsRequired();

                e.HasOne(x => x.Event)
                 .WithMany(ev => ev.EventRegistrations)
                 .HasForeignKey(er => er.EventId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(x => x.Ticket)
                .WithOne(t => t.EventRegistration)
                .HasForeignKey<Ticket>(t => t.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Ticket>(e =>
            {
                e.ToTable("Tickets");
                e.Property(x => x.TicketCode).HasMaxLength(20).IsRequired();
                e.Property(x => x.SecretPasscode).HasMaxLength(6).IsRequired();

                e.HasOne(x => x.EventRegistration)
                .WithOne(er => er.Ticket)
                .HasForeignKey<Ticket>(t => t.RegistrationId)
                .OnDelete(DeleteBehavior.SetNull);
            });
        }
        #endregion
    }
}
