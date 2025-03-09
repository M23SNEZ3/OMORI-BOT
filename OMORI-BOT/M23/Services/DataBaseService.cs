using Microsoft.EntityFrameworkCore;

namespace OMORI_BOT.M23.Services;
    /// <summary>
    ///     Service for managing Data Base with UserBirthDay and GuildLang tables
    /// </summary>
    public enum Lang
    {
        Ru, En
    }

    public class UserBirthDay // Table for Birthday
    {
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public ulong Name { get; set; }
        public DateTime Date { get; set; }

    }
    
    public class GuildLang // Table for language on servers
    {
        public int Id { get; set; }
        public ulong ServerId { get; set; }
        public Lang Lang { get; set; }

    }
    public class ApplicationContext : DbContext
    {
        public DbSet<UserBirthDay> Users => Set<UserBirthDay>();
        public DbSet<GuildLang> GuildsLang => Set<GuildLang>();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=GuildsInformation.db");
        }
    }

    public sealed class DataBaseService
    {
        private readonly ApplicationContext _context;

        public DataBaseService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<bool> GetUserInDataBase(ulong id, ulong serverId)
        {
            return await _context.Users.AnyAsync(u => u.ServerId == serverId && u.Name == id);
        }


        public async Task<UserBirthDay?> PrintUserFromDataBase(ulong id, ulong serverId)
        {
            return await _context.Users.SingleOrDefaultAsync(u => u.ServerId == serverId && u.Name == id);
        }

        public async Task AddToDataBase(ulong serverId, ulong name, DateTime date)
        {
            UserBirthDay userBirthDay = new UserBirthDay { ServerId = serverId, Name = name, Date = date };
            _context.Users.Add(userBirthDay);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteFromDataBase(ulong name, ulong serverId)
        {
            var user = await PrintUserFromDataBase(name, serverId);
            if (user is not null && user.Name == name)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return !(await GetUserInDataBase(serverId, name));
        }

        public async Task<List<UserBirthDay>> PrintDataBase(ulong serverId)
        {
            return await _context.Users.Where(u => u.ServerId == serverId).ToListAsync();
        }

        private async Task AddLangServer(ulong serverId)
        {
            var existingServer = await _context.GuildsLang.FirstOrDefaultAsync(u => u.ServerId == serverId);
            if (existingServer is null)
            {
                GuildLang guildLang = new GuildLang { ServerId = serverId, Lang = Lang.Ru };
                _context.Add(guildLang);
                await _context.SaveChangesAsync();
            }
        }

        public async Task EditLangServer(ulong serverId, Lang lang)
        {
            var existingServer = await _context.GuildsLang.FirstOrDefaultAsync(u => u.ServerId == serverId);
            if (existingServer is null)
            {
                await AddLangServer(serverId);
                existingServer = await _context.GuildsLang.FirstAsync(u => u.ServerId == serverId);
            }
            existingServer.Lang = lang;
            await _context.SaveChangesAsync();
        }

        public async Task<GuildLang> GetLangServer(ulong serverId)
        {
            var existingServer = await _context.GuildsLang.FirstOrDefaultAsync(u => u.ServerId == serverId);
            if (existingServer is not null)
                return existingServer;

            var newGuildLang = new GuildLang { ServerId = serverId, Lang = Lang.Ru };
            _context.Add(newGuildLang);
            await _context.SaveChangesAsync();
            return newGuildLang;
        }
    }
