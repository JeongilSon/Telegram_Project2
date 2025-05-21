using Microsoft.EntityFrameworkCore;
using System.Security.Principal;
using Telegram_Project2_Web.Models;

namespace Telegram_Project2_Web.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
     : base(options)
        {
        }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<UserModel> ChatMessages { get; set; }
        public DbSet<ChannelModel> Channels { get; set; }
        public DbSet<MissionModel> Attendances { get; set; }
        public DbSet<LiveChatModel> LiveChats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 계정 테이블 설정
            modelBuilder.Entity<AccountModel>().ToTable("account_table");

            // 계정 ID를 기본 키로 설정
            modelBuilder.Entity<AccountModel>()
                .HasKey(a => a.Id);                                       
            
            // 라이브 채팅 테이블 설정
            modelBuilder.Entity<LiveChatModel>().ToTable("live_chat_table");
            modelBuilder.Entity<LiveChatModel>().HasKey(l => l.Id);
        }
    }
}
