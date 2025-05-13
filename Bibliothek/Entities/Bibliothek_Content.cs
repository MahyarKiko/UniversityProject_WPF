using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Bibliothek.Entities
{
    public partial class Bibliothek_Content : DbContext
    {
        public Bibliothek_Content()
        {
        }

        public Bibliothek_Content(DbContextOptions<Bibliothek_Content> options)
            : base(options)
        {
        }

        public virtual DbSet<Authors> Authors { get; set; }
        public virtual DbSet<Book> Book { get; set; }
        public virtual DbSet<BookBorrow> BookBorrow { get; set; }
        public virtual DbSet<BuchAusLeihen> BuchAusLeihen { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<CountOfBooks> CountOfBooks { get; set; }
        public virtual DbSet<Menu> Menu { get; set; }
        public virtual DbSet<TypeToMenuAccess> TypeToMenuAccess { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserPermission> UserPermission { get; set; }
        public virtual DbSet<UserToSearch> UserToSearch { get; set; }
        public virtual DbSet<UserType> UserType { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Data Source=KIKO\\MAHYARSQLSERVER;Initial Catalog= Bibliothek;Persist Security Info=True;User ID=sa;Password=123456;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasOne(d => d.Author)
                    .WithMany(p => p.Book)
                    .HasForeignKey(d => d.AuthorID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buch_Autors");

                entity.HasOne(d => d.Categorye)
                    .WithMany(p => p.Book)
                    .HasForeignKey(d => d.CategoryeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buch_Kategorie");
            });

            modelBuilder.Entity<BookBorrow>(entity =>
            {
                entity.HasOne(d => d.Book)
                    .WithMany(p => p.BookBorrow)
                    .HasForeignKey(d => d.BookID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BuchAusLeihen_Buch");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.BookBorrow)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BuchAusLeihen_User");
            });

            modelBuilder.Entity<BuchAusLeihen>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<CountOfBooks>(entity =>
            {
                entity.HasOne(d => d.Book)
                    .WithMany(p => p.CountOfBooks)
                    .HasForeignKey(d => d.BookID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CountOfBooks_Buch");
            });

            modelBuilder.Entity<TypeToMenuAccess>(entity =>
            {
                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.TypeToMenuAccess)
                    .HasForeignKey(d => d.MenuID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TypeToMenuAccess_Menü");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.TypeToMenuAccess)
                    .HasForeignKey(d => d.TypeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TypeToMenuAccess_UserType");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(d => d.UserType)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.UserTypeID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_UserType");
            });

            modelBuilder.Entity<UserPermission>(entity =>
            {
                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.UserPermission)
                    .HasForeignKey(d => d.MenuID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRechte_Menü1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserPermission)
                    .HasForeignKey(d => d.UserID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserRechte_User1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
