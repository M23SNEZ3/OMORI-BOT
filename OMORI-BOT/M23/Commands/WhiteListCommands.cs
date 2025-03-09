using OMORI_BOT.M23.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Extensions;

namespace OMORI_BOT.M23.Commands;
    /// <summary>
    ///     Commands for managing WhiteList
    /// </summary>
    public sealed class WhiteListCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _commandContext;
        private readonly WhiteListService _whiteListService;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly DataBaseService _dataBaseService;

        public WhiteListCommands(FeedbackService feedback, ICommandContext commandContext,
            WhiteListService whiteListService, IDiscordRestGuildAPI guildApi, DataBaseService dataBaseService)
        {
            _feedbackService = feedback;
            _commandContext = commandContext;
            _whiteListService = whiteListService;
            _guildApi = guildApi;
            _dataBaseService = dataBaseService;
        }
        
        [Command("add")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Description("Добавить человека в вайт лист")]
        public async Task<Result> AddToWhiteList([Description("ID человека")] [Option("участник")] IUser id)
        {
            if (!_commandContext.TryGetGuildID(out var guildId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary ID from command context");
            }
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            var memberOnServer = await _guildApi.GetGuildMemberAsync(guildId, id.ID, CancellationToken);
            Embed embed;
            if (!memberOnServer.IsSuccess)
            {
                embed = new Embed(Title: Messages.Error,
                    Description: string.Format(Messages.ErrorDeleteWhiteListServer, $"<@{id.ID}>"));
            }
            else
            {
                var success = await _whiteListService.AddWhiteList(id.ID);
                embed = success
                    ? new Embed(Title: Messages.Successfully,
                        Description: string.Format(Messages.SuccessfullyAddToWhiteList, $"<@{id.ID}>"), Colour: Color.Green)
                    : new Embed(Title: Messages.Error,
                        Description: string.Format(Messages.ErrorAddWhiteListYet, $"<@{id.ID}>"), Colour: Color.Red);
            }
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);

        }

        [Command("delete")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Description("Удалить человека с вайт листа")]
        public async Task<Result> DeleteFromWhiteList([Option("участник")] IUser id)
        {
            if (!_commandContext.TryGetGuildID(out var guildId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary ID from command context");
            }
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            var memberOnServer = await _guildApi.GetGuildMemberAsync(guildId, id.ID, CancellationToken);
            if (!memberOnServer.IsSuccess)
            {
                var embed = new Embed(
                    Title: Messages.Error,
                    Description: string.Format(Messages.ErrorDeleteWhiteListServer, $"<@{id.ID}>"), Colour: Color.Red
                );

                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            var success = await _whiteListService.DeleteWhiteList(id.ID);

            var embedResult = success
                ? new Embed(Title: Messages.Successfully,
                    Description: string.Format(Messages.SuccesfullyDeleteFromWhiteList, id.ID), Colour: Color.Green)
                : new Embed(Title: Messages.Error,
                    Description: string.Format(Messages.ErrorDeleteWhiteListYet, $"<@{id.ID}>"), Colour: Color.Red);

            return await _feedbackService.SendContextualEmbedResultAsync(embedResult, ct: CancellationToken);
        }

        [Command("print")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Description("Вывести пользователей из вайтлиста")]
        public async Task<Result> PrintWhiteList()
        {
            if (!_commandContext.TryGetGuildID(out var guildId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary ID from command context");
            }
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
            var membersList = await _whiteListService.GetWhiteList();
            Embed embed;
            if (membersList is null || membersList.Triggers.Count == 0)
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.ErrorPrintWhiteList, Colour: Color.Red);
            }
            else
            {
                var triggerList = string.Join(",", membersList.Triggers.Select(id => $"<@{id}>"));
                embed = new Embed(Title: Messages.Successfully, Description: string.Format(Messages.PrintWhiteList, triggerList), Colour: Color.Green);
            }

            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
        }
    }

