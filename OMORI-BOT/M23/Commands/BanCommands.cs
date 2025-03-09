using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Extensions;

namespace OMORI_BOT.M23.Commands;

/// <summary>
///     Commands for "/ban" and "/unban" members
/// </summary>
sealed class BanCommands : CommandGroup
{
    private readonly IDiscordRestChannelAPI _channelApi;
    private readonly ICommandContext _commandContext;
    private readonly IFeedbackService _feedbackService;
    private readonly IDiscordRestGuildAPI _guildApi;
    private readonly IDiscordRestUserAPI _userApi;
    private readonly DataBaseService _dataBaseService;
    
    public BanCommands(IDiscordRestChannelAPI channelApi, ICommandContext context,
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
    
    [Command("ban")]
    [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
    [DiscordDefaultDMPermission(false)]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [RequireBotDiscordPermissions(DiscordPermission.BanMembers)]
    public async Task<Result> BanAsync(
        [Description("User to ban")] IUser target,
        [Description("Ban reason")] [MaxLength(256)]
        string reason)
    {
        if(!_commandContext.TryGetContextGuildAndUserIDs(out var guildId, out var userId))
            return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary IDs from command context");
        
        var userResult = await _userApi.GetUserAsync(userId, CancellationToken);
        if (!userResult.IsDefined(out var user))
        {
            return Result.FromError(userResult);
        }

        var guildResult = await _guildApi.GetGuildAsync(guildId, ct: CancellationToken);
        if (!guildResult.IsDefined(out var guild))
        {
            return Result.FromError(guildResult);
        }
        var language = await _dataBaseService.GetLangServer(guildId.Value);
        Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
        return await ExucateBanAsync(target, reason, user, guild, CancellationToken);
    }

    private async Task<Result> ExucateBanAsync(IUser target, string reason,IUser user, IGuild guild, CancellationToken ct = default)
    {
        Embed embed;
        var existingBanResult = await _guildApi.GetGuildBanAsync(guild.ID, target.ID, ct);
        if (existingBanResult.IsDefined())
        {
            embed = new Embed(Title: Messages.Error, Description: string.Format(Messages.ErrorBanAlready, target.Username), Colour: Color.Red);
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
        }
        
        var dmChannelResult = await _userApi.CreateDMAsync(target.ID, ct);
        if (dmChannelResult.IsDefined(out var dmChannel))
        {
            await _channelApi.CreateMessageAsync(dmChannel.ID, string.Format(Messages.DMBanMessage, guild.Name, user.Username, reason), ct: ct);
        }
        
        var resultBan = await _guildApi.CreateGuildBanAsync(guild.ID, target.ID, reason: reason.EncodeHeader(), ct: ct);
        if (!resultBan.IsSuccess)
        {
            return Result.FromError(resultBan.Error);
        }
        
        embed = new Embed(Title: Messages.Successfully, Description: string.Format(Messages.SuccesfullyBan, target.Username), Colour: Color.Green);
        return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
    }


    [Command("unban")]
    [RequireDiscordPermission(DiscordPermission.ManageMessages)]
    [RequireBotDiscordPermissions(DiscordPermission.BanMembers)]
    [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
    public async Task<Result> UnbanAsync([Description("User for unban")]IUser target, string? reason = null)
    {
        if(!_commandContext.TryGetGuildID(out var guildId))
            return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary ID from command context");
        
        var guildResult = await _guildApi.GetGuildAsync(guildId, ct: CancellationToken);
        if (!guildResult.IsDefined(out var guild))
        {
            return Result.FromError(guildResult);
        }
        var language = await _dataBaseService.GetLangServer(guildId.Value);
        Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
        if (reason is null)
        {
            return await ExucateUnBanAsync(target, null, guild, CancellationToken);
        }
        return await ExucateUnBanAsync(target, reason, guild, CancellationToken);
    }

    private async Task<Result> ExucateUnBanAsync(IUser target, string? reason, IGuild guild,
        CancellationToken ct = default)
    {
        Embed embed;
        var existingBanResult = await _guildApi.GetGuildBanAsync(guild.ID, target.ID, ct);
        if (!existingBanResult.IsDefined())
        {
            embed = new Embed(Title: Messages.Error, Description: string.Format(Messages.ErrorUnbanMemberNotBan, target.Username), Colour: Color.Red);
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
        }
            
        var resultBan = await _guildApi.RemoveGuildBanAsync(guild.ID, target.ID, ct: ct);
        if (!resultBan.IsSuccess)
        {
            Result.FromError(resultBan.Error); 
        }
        
        embed = new Embed(Title: Messages.Successfully, Description: (reason is null) ? string.Format(Messages.UnBanWithoutReason, target.Username) : string.Format(Messages.UnBanWithReason, target.Username, reason), Colour: Color.Green);
        return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
    }

    
}