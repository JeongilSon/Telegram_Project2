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
        public DbSet<UserModel> Users { get; set; }
        public DbSet<ChannelModel> Channels { get; set; }
        public DbSet<LinkModel> Links { get; set; }
        public DbSet<MissionModel> Missions { get; set; }
        public DbSet<LivechatModel> LiveChats { get; set; }
        public DbSet<BotMessageModel> BotMessages { get; set; }
        
        public DbSet<BotTokenModel> BotTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 계정 테이블 설정
            modelBuilder.Entity<AccountModel>().ToTable("account_table");            
            modelBuilder.Entity<AccountModel>().HasKey(a => a.Id);                                              
            
            // 라이브 채팅 테이블 설정
            modelBuilder.Entity<LivechatModel>().ToTable("live_chat_table");
            modelBuilder.Entity<LivechatModel>().HasKey(l => l.Id);

            // 채널 테이블 설정
            modelBuilder.Entity<ChannelModel>().ToTable("channel_list_table");
            modelBuilder.Entity<ChannelModel>().HasKey(l => l.Channel_Code);

            // 링크 테이블 설정
            modelBuilder.Entity<LinkModel>().ToTable("link_list_table");
            modelBuilder.Entity<LinkModel>().HasKey(l => l.Link_Name);

            // 유저 테이블 설정
            modelBuilder.Entity<UserModel>().ToTable("user_list_table");
            modelBuilder.Entity<UserModel>().HasKey(l => l.Chat_ID);

            // 미션 테이블 설정
            modelBuilder.Entity<MissionModel>().ToTable("mission_table");
            modelBuilder.Entity<MissionModel>().HasKey(l => l.Mission_Name);

            // 봇 메시지 설정
            modelBuilder.Entity<BotMessageModel>().ToTable("bot_message_table");
            modelBuilder.Entity<BotMessageModel>().HasKey(l => l.Id);

            // 봇 토큰 설정
            modelBuilder.Entity<BotTokenModel>().ToTable("bot_token_table");
            modelBuilder.Entity<BotTokenModel>().HasKey(l => l.Id);
        }
    }
}
