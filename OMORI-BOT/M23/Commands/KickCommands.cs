using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace OMORI_BOT.M23.Commands;

sealed class KickCommands : CommandGroup
{
    /// <summary>
    ///     Command for "/kick" members
    /// </summary>
    
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _commandContext;
    private readonly IFeedbackService _feedbackService;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestUserAPI _userApi;
    private readonly DataBaseService _dataBaseService;

    public KickCommands(IDiscordRestChannelAPI channelApi, ICommandContext context,
        IFeedbackService feedback, IDiscordRestGuildAPI guildApi,
        IDiscordRestUserAPI userApi, DataBaseService dataBaseService)
    {
        _channelApi = channelApi;
        _commandContext = context;
        _feedbackService = feedback;
        _guildApi = guildApi;
        _userApi = userApi;
        _dataBaseService = dataBaseService;
    }

    
    [Command("kick")]
    [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
    [DiscordDefaultDMPermission(false)]
    [RequireContext(ChannelContext.Guild)]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [RequireBotDiscordPermissions(DiscordPermission.KickMembers)]
    public async Task<Result> KickAsync(IUser target, string reason)
    {
        
        if(!_commandContext.TryGetContextGuildAndUserIDs(out var guildId, out var userId))
            return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary IDs from command context");
        
        var userResult = await _userApi.GetUserAsync(userId, CancellationToken);
        if (!userResult.IsDefined(out var user))
            return Result.FromError(userResult);

        var guildResult = await _guildApi.GetGuildAsync(guildId, ct: CancellationToken);
        if (!guildResult.IsDefined(out var guild))
            return Result.FromError(guildResult);
        
        var language = await _dataBaseService.GetLangServer(guildId.Value);
        Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
        var memberResult = await _guildApi.GetGuildMemberAsync(guildId, target.ID, CancellationToken);
        if (!memberResult.IsSuccess)
        {
            var embed = new Embed(Title: Messages.Error, Colour: Color.Red );
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
        }
        return await ExucateKickAsync(target, reason, user, guild, CancellationToken);
    }

    private async Task<Result> ExucateKickAsync(IUser target, string reason, IUser user, IGuild guild,
        CancellationToken ct = default)
    {
        Embed embed;
        var dmChannelResult = await _userApi.CreateDMAsync(target.ID, ct);
        if (dmChannelResult.IsDefined(out var dmChannel))
        {
            await _channelApi.CreateMessageAsync(dmChannel.ID, string.Format(Messages.DMMessageKick, guild.Name, user.Username, reason), ct: ct);
        }
        
        var kickResult = await _guildApi.RemoveGuildMemberAsync(guild.ID, target.ID, reason.EncodeHeader(), CancellationToken);
        if (!kickResult.IsSuccess)
        {
            embed = new Embed(Title: Messages.Error, Description: $"``{kickResult.Error.Message}``", Colour: Color.Red);
        }
        else
        {
            embed = new Embed(Title: Messages.Successfully, Description: string.Format(Messages.MemberKicked, target.Username), Colour: Color.Green);
        }
        return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
        
        
    }
}