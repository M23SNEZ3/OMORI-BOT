using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace OMORI_BOT.M23.Commands;

sealed class GuildSettingCommands : CommandGroup
{
    /// <summary>
    ///     Command for managing the server language
    /// </summary>
    
    private readonly FeedbackService _feedbackService;
    private readonly ICommandContext _commandContext;
    private readonly DataBaseService _dataBaseService;

    public GuildSettingCommands(FeedbackService feedback,
        ICommandContext commandContext,
        DataBaseService dataBaseService)
    {
        _feedbackService = feedback;
        _commandContext = commandContext;
        _dataBaseService = dataBaseService;
    }

    [Command("edit-lang")]
    [Ephemeral]
    [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
    [Description("Изменить язык ответа бота")]
    public async Task<Result> EditLang([Option("Язык")] Lang lang)
    {
        if (!_commandContext.TryGetGuildID(out var guildId))
        {
            return new ArgumentInvalidError(nameof(_commandContext),
                "Unable to retrieve necessary IDs from command context");
        }

        await _dataBaseService.EditLangServer(guildId.Value, lang);
        var language = await _dataBaseService.GetLangServer(guildId.Value);
        Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US");
        Embed embed = new Embed(Title: Messages.Successfully,
            Description: string.Format(Messages.EditLangOnServer, Messages.Culture), Colour: Color.Green);
        return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
    }
}