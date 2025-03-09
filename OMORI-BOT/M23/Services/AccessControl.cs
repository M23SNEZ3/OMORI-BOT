using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Rest.Core;
namespace OMORI_BOT.M23.Services;
    /// <summary>
    /// It's not final version Service:)
    /// </summary>
    public sealed class AccessControlService
    {
        private readonly IDiscordRestGuildAPI _guildApi;

        public AccessControlService(IDiscordRestGuildAPI guildApi)
        {
            _guildApi = guildApi;
        }

        public async Task<bool> IsUserAdmin(Snowflake guildId, Snowflake userId, CancellationToken ct)
        {
            bool roleAdministrator = false;
            var resultRole = await _guildApi.GetGuildMemberAsync(guildId, userId, ct);
            var user = resultRole.Entity;
            var resultRoles = await _guildApi.GetGuildRolesAsync(guildId, ct);
            var roles = resultRoles.Entity;
            foreach (var roleID in user.Roles)
            {
                var role = roles.First(roleObject => roleID == roleObject.ID);
                roleAdministrator = role.Permissions.HasPermission(DiscordPermission.Administrator);
                if (roleAdministrator)
                {
                    return true;
                }
            }
            return false;
        }
    }
