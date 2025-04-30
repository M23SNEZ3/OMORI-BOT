using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Extensions.Embeds;
using Remora.Rest.Core;
using Remora.Results;

namespace OMORI_BOT.M23.Commands;

sealed class AboutCommand : CommandGroup
{
    private readonly ICommandContext _context;
    private readonly IFeedbackService _feedback;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestUserAPI _userApi;
    private readonly DataBaseService _dataBaseService;

    public AboutCommand(
        ICommandContext context,
        IFeedbackService feedback, IDiscordRestUserAPI userApi,
        IDiscordRestGuildAPI guildApi, DataBaseService dataBaseService)
    {
        _context = context;
        _feedback = feedback;
        _userApi = userApi;
        _guildApi = guildApi;
        _dataBaseService = dataBaseService;
    }
    
    [Command("about")]
    [RequireContext(ChannelContext.Guild)]
    [Description("Показать информацию о разработчиках бота")]
    public async Task<Result> AboutAsync()
    {
        if (!_context.TryGetGuildID(out var guildId))
        {
            return new ArgumentInvalidError(nameof(_context), "Unable to retrieve necessary IDs from command context");
        }
        
        var botResult = await _userApi.GetCurrentUserAsync(CancellationToken);
        if (!botResult.IsDefined(out var bot))
            return Result.FromError(botResult);

        var language = await _dataBaseService.GetLangServer(guildId.Value);
        Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

        return await SendAboutAsync(bot, guildId, CancellationToken);
    }

    private async Task<Result> SendAboutAsync(IUser bot, Snowflake guildId, CancellationToken ct)
    {
        var embed = new EmbedBuilder()
            .WithTitle(string.Format(bot.Username))
            .WithDescription(Messages.about)
            .WithColour(Color.Bisque)
            .WithImageUrl("https://raw.githubusercontent.com/M23SNEZ3/OMORI-BOT/HEAD/OmoriBot.png")
            .Build();
        return await _feedback.SendContextualEmbedResultAsync(embed, ct: ct);
    }
}