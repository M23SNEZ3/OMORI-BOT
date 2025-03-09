using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Rest.Core;

namespace OMORI_BOT.M23.Extensions
{

    public static class CommandContextExtensions
    {
        /// <summary>
        ///     Extensions for Command Context
        /// </summary>
        public static bool TryGetContextGuildAndUserIDs(
            this ICommandContext context, out Snowflake guildId, out Snowflake userId)
        {
            userId = default;
            return context.TryGetGuildID(out guildId) && context.TryGetUserID(out userId);
        }

        public static bool TryGetContextGuildUserAndChannelIDs(this ICommandContext context,
            out Snowflake guildId, out Snowflake channelId, out Snowflake userId)
        {
            userId = default;
            channelId = default;
            return context.TryGetGuildID(out guildId)
                   && context.TryGetChannelID(out channelId)
                   && context.TryGetUserID(out userId);
        }

        public static bool TryGetContextGuildAndChannelIDs(this ICommandContext context,
            out Snowflake guildId, out Snowflake channelId)
        {
            channelId = default;
            guildId = default;
            return context.TryGetGuildID(out guildId)
                   && context.TryGetChannelID(out channelId);
        }
    }
}