using Client.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Database
{
	public class RecordContext : DbContext
	{
		public DbSet<FtpServerEntity> Servers { get; set; }
		public DbSet<RecordEntity> Records { get; set; }
		public RecordContext(DbContextOptions<RecordContext> options) : base(options) { }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<FtpServerEntity>().ToTable("Servers");
			modelBuilder.Entity<FtpServerEntity>().HasKey(s => s.Id);
			modelBuilder.Entity<FtpServerEntity>().Property(s => s.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<FtpServerEntity>().Property(s => s.IpAddress).IsRequired();
			modelBuilder.Entity<FtpServerEntity>().Property(s => s.Username).IsRequired();
			modelBuilder.Entity<FtpServerEntity>()
			   .HasMany<RecordEntity>()
			   .WithOne()
			   .HasForeignKey(r => r.ServerId)
			   .IsRequired(false);

			modelBuilder.Entity<RecordEntity>().ToTable("Records");
			modelBuilder.Entity<RecordEntity>().HasKey(r => r.Id);
			modelBuilder.Entity<RecordEntity>().Property(r => r.Id).ValueGeneratedOnAdd();
			modelBuilder.Entity<RecordEntity>().Property(r => r.Command).IsRequired();
			modelBuilder.Entity<RecordEntity>().Property(r => r.Timestamp).IsRequired();

			base.OnModelCreating(modelBuilder);
		}
	}
}
