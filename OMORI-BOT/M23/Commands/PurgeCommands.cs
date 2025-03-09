using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Rest.Core;
using Remora.Results;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Conditions;

namespace OMORI_BOT.M23.Commands;
    /// <summary>
    ///     Commands for purge ( clear ) messages
    /// </summary>

    sealed class PurgeCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestChannelAPI _discordrestService;
        private readonly ICommandContext _commandContext;
        private readonly DataBaseService _dataBaseService;

        public PurgeCommands(FeedbackService feedback, IDiscordRestChannelAPI discordRestChannelApi,
            ICommandContext commandContext, DataBaseService dataBaseService)
        {
            _feedbackService = feedback;
            _discordrestService = discordRestChannelApi;
            _commandContext = commandContext;
            _dataBaseService = dataBaseService;

        }

        [Command("purge-symbol")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
        [RequireDiscordPermission(DiscordPermission.ManageMessages)]
        [RequireBotDiscordPermissions(DiscordPermission.ManageMessages)]
        [Description("Удалить сообщение с символом")]
        public async Task<Result> DeleteMessages([Description("Количество сообщений")] [Option("Количество")] [MaxValue(99)][MinValue(2)] int quantity,
            [Description("Символ для удаления")] [Option("символ")] string triggerSymbol)
        {
            if (!_commandContext.TryGetContextGuildAndChannelIDs(out var guildId, out var channelId))
            {
                return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary IDs from command context");
            }
            var messagesResult = await _discordrestService.GetChannelMessagesAsync(channelId, limit: quantity + 1, ct: CancellationToken);
            if (!messagesResult.IsDefined(out var messages))
                return Result.FromError(messagesResult);
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
            
            return await ClearMessagesWithSymbolAsync(channelId, messages, triggerSymbol, CancellationToken);
        }

        private async Task<Result> ClearMessagesWithSymbolAsync(Snowflake channelId, IReadOnlyList<IMessage> messages, string triggerSymbol,
            CancellationToken ct)
        {
            var messageIds = new List<Snowflake>(messages.Count);
            Embed embed;
            for (var i = messages.Count - 1; i >= 1; i--) // Skip message "Bot thinking..."
            {
                var message = messages[i];
                if (message.Content.StartsWith(triggerSymbol))
                {
                    messageIds.Add(message.ID);
                }
            }
            
            if (messageIds.Count == 0)
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.ErrorNotFoundSymbol, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            }
            
            var deleteResult = await _discordrestService.BulkDeleteMessagesAsync(channelId, messageIds, ct: ct);
            if(!deleteResult.IsSuccess)
            {
                embed = new Embed(Title: Messages.Error, Description: $"``{deleteResult.Error.Message}``", Colour: Color.Red);
            }
            else
            {
                embed = new Embed(Title: Messages.Successfully, Description: Messages.SuccessfullyDeletedMessages, Colour: Color.Green);
            }
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            
        }

        [Command("purge")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
        [RequireDiscordPermission(DiscordPermission.ManageMessages)]
        [RequireBotDiscordPermissions(DiscordPermission.ManageMessages)]
        [Description("Удалить группу сообщений")]
        public async Task<Result> DeleteMessages(
            [Description("Количество сообщений")] [Option("Количество")] [MinValue(2)][MaxValue(99)] int quantity)
        {
            if (!_commandContext.TryGetContextGuildAndChannelIDs(out var guildId, out var channelId))
                return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary IDs from command context");

            var messagesResult = await _discordrestService.GetChannelMessagesAsync(channelId, limit: quantity + 1, ct: CancellationToken);
            if (!messagesResult.IsDefined(out var messages))
                return Result.FromError(messagesResult);
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
            
            return await ClearMessagesAsync( channelId, messages, ct: CancellationToken);
        }
        
        private async Task<Result> ClearMessagesAsync(Snowflake channelId, IReadOnlyList<IMessage> messages, CancellationToken ct)
        {
            var messageIds = new List<Snowflake>(messages.Count);
            Embed embed;
            for (var i = messages.Count - 1; i >= 1; i--) // Skip message "Bot thinking..."
            {
                var message = messages[i];
                messageIds.Add(message.ID);
            }
            
            if (messageIds.Count == 0)
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.NoMessagesToClear, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            }
            
            var deleteResult = await _discordrestService.BulkDeleteMessagesAsync(channelId, messageIds, ct: ct);
            if (!deleteResult.IsSuccess)
            {
                embed= new Embed(Title: Messages.Error, Description: $"``{deleteResult.Error.Message}``", Colour: Color.Red);
            }
            else
            {
                embed = new Embed(Title: Messages.Successfully, Description: Messages.SuccessfullyDeletedMessages, Colour: Color.Green);
            }
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            
        }

        [Command("purge-user")]
        [Ephemeral]
        [DiscordDefaultMemberPermissions(DiscordPermission.ManageMessages)]
        [RequireDiscordPermission(DiscordPermission.ManageMessages)]
        [RequireBotDiscordPermissions(DiscordPermission.ManageMessages)]
        [Description("Удалить сообщения от человека")]
        public async Task<Result> DeleteMessagesFromUser(
            [Description("Количество сообщений")] [Option("Количество")] [MaxValue(99)] [MinValue(2)]
            int quantity,
            [Description("ID человека")] IUser id)
        {
            if (!_commandContext.TryGetContextGuildAndChannelIDs(out var guildId, out var channelId))
            {
                return new ArgumentInvalidError(nameof(_commandContext), "Unable to retrieve necessary IDs from command context");
            }

            var messagesResult = await _discordrestService.GetChannelMessagesAsync(channelId, limit: quantity + 1, ct: CancellationToken);
            if (!messagesResult.IsDefined(out var messages))
                return Result.FromError(messagesResult);
            var language = await _dataBaseService.GetLangServer(guildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );
            return await ClearMessagesFromUserAsync(channelId, messages, id, CancellationToken);

        }
        private async Task<Result> ClearMessagesFromUserAsync(Snowflake channelId, IReadOnlyList<IMessage> messages, IUser author, CancellationToken ct)
        {
            var messageIds = new List<Snowflake>(messages.Count);
            Embed embed;
            for (var i = messages.Count - 1; i >= 1; i--) // Skip message "Bot thinking..."
            {
                var message = messages[i];
                if (message.Author.Equals(author))
                {
                    messageIds.Add(message.ID);
                }
            }
            
            if (messageIds.Count == 0)
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.NoMessagesToClear, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            }
            
            var deleteResult = await _discordrestService.BulkDeleteMessagesAsync(channelId, messageIds, ct: ct);
            if(!deleteResult.IsSuccess)
            {
                embed = new Embed(Title: Messages.Error, Description: $"``{deleteResult.Error.Message}``", Colour: Color.Red);
            }
            else
            {
                embed = new Embed(Title: Messages.Successfully, Description: Messages.SuccessfullyDeletedMessagesFromUser, Colour: Color.Green);
            }
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: ct);
            
        }

    }
