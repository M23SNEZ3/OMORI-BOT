using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using OMORI_BOT.M23.Extensions;
using OMORI_BOT.M23.Services;
using Remora.Discord.Commands.Extensions;

namespace OMORI_BOT.M23.Commands;

    /// <summary>
    ///     Commands for managing the birthday table
    /// </summary>
    sealed class BirthdayCommands : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly ICommandContext _commandContext;
        private readonly IDiscordRestGuildAPI _discordRestGuildApi;
        private readonly DataBaseService _dataBaseService;
        private readonly WhiteListService _whiteListService;

        public BirthdayCommands(FeedbackService feedback, ICommandContext commandContext,
            IDiscordRestGuildAPI discordRestGuildApi, DataBaseService dataBaseService,
            WhiteListService whiteListService)
        {
            _feedbackService = feedback;
            _commandContext = commandContext;
            _discordRestGuildApi = discordRestGuildApi;
            _dataBaseService = dataBaseService;
            _whiteListService = whiteListService;
        }

        [Command("remember-birthday")]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Запомнить день рождение участника")]
        public async Task<Result> AddToBirthDataBase
        (
            [Option("участник")] IUser nameUser,
            [Option("месяц")] int month,
            [Option("день")] int day
        )
        {
            Embed embed;
            if (!_commandContext.TryGetContextGuildAndUserIDs(out var contextGuildId, out var contextUserId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary IDs from command context");
            }
            
            var language = await _dataBaseService.GetLangServer(contextGuildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            if (!DateTime.TryParseExact($"{month:00}/{day:00}", "MM/dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _))
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.UnknownDate, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }


            if (!(_discordRestGuildApi.GetGuildMemberAsync(contextGuildId, nameUser.ID).Result.IsSuccess))
            {
                embed = new Embed(Title: Messages.UnknownError, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            if (await _dataBaseService.GetUserInDataBase(contextUserId.Value, contextGuildId.Value))
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.ErrorRememberBirthday,
                    Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            await _dataBaseService.AddToDataBase(contextGuildId.Value, contextUserId.Value,
                new(DateTime.Now.Year, month, day));
            embed = new Embed(Title: Messages.Successfully, Description: Messages.SuccessfullyRememberBirthday,
                Colour: Color.Green);
            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
        }

        [Command("print-birthday")]
        [Description("Вывести все дни рождения")]
        public async Task<Result> ShowUpcomingBirthdays()
        {
            if (!_commandContext.TryGetContextGuildAndUserIDs(out var contextGuildId, out var contextUserId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary IDs from command context");
            }
            var language = await _dataBaseService.GetLangServer(contextGuildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            if (!(await _discordRestGuildApi.GetGuildMemberAsync(contextGuildId, contextUserId)).IsSuccess)
            {
                var faildEmbed = new Embed(Title: Messages.UnknownError, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(faildEmbed, ct: CancellationToken);
            }

            if (!await _whiteListService.CheckWhitelist(contextUserId))
            {
                var faildEmbed = new Embed(Title: Messages.Error, Description: Messages.NoAccessToUse,
                    Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(faildEmbed, ct: CancellationToken);
            }

            var users = (await _dataBaseService.PrintDataBase(contextGuildId.Value))
                .OrderBy(p => p.Date)
                .ToList();

            if (!users.Any())
            {
                var faildEmbed = new Embed(Title: Messages.Error, Description: Messages.ErrorTableIsEmpty, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(faildEmbed, ct: CancellationToken);
            }

            var pageFields = users.Select(resident => new EmbedField(
                resident.Date.ToString("dd MMMM", new CultureInfo(language.Lang == Lang.Ru ? "ru-RU" : "en-US")),
                $"<@{resident.Name}>",
                false)).ToList();

            var embed = new Embed
            {
                Title = Messages.UpcomingBirthdays,
                Colour = Color.Bisque,
                Fields = pageFields
            };

            return await _feedbackService.SendContextualPaginatedResultAsync(contextUserId, new List<Embed> { embed });
        }

        [Command("birthday")]
        [Ephemeral]
        [Description("Вывести день рождения конкретного участника")]
        public async Task<Result> PrintBirthDay([Option("участник")] IUser nameUser)
        {
            if (!_commandContext.TryGetContextGuildAndUserIDs(out var contextGuildId, out var contextUserId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary IDs from command context");
            }
            var language = await _dataBaseService.GetLangServer(contextGuildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            if (!await _whiteListService.CheckWhitelist(contextUserId))
            {
                var embed = new Embed(Title: Messages.Error, Description: Messages.NoAccessToUse, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            if (!await _dataBaseService.GetUserInDataBase(nameUser.ID.Value, contextGuildId.Value))
            {
                var embed = new Embed(Title: Messages.Error, Description: Messages.ErrorDontRemember,
                    Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            var resident = await _dataBaseService.PrintUserFromDataBase(nameUser.ID.Value, contextGuildId.Value);
            if (resident is not null)
            {
                var pageFields = new List<EmbedField>
                {
                    new EmbedField($"{resident.Date:dd MMMM}", $"<@{resident.Name}>", false)
                };
                var embed = new Embed(Title: Messages.MemberBirthday, Fields: pageFields, Colour: Color.Bisque);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }
            else
            {
                var faildEmbed = new Embed(Title: Messages.Error, Description: Messages.ErrorUserInTableIsEmpty, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(faildEmbed, ct: CancellationToken);
            }
        }

        [Command("forget-birthday")]
        [DiscordDefaultMemberPermissions(DiscordPermission.Administrator)]
        [Ephemeral]
        [Description("Забыть день рождения участника")]
        public async Task<Result> DeleteBirthDay([Option("участник")] IUser nameUser)
        {
            if (!_commandContext.TryGetGuildID(out var contextGuildId))
            {
                return new ArgumentInvalidError(nameof(_commandContext),
                    "Unable to retrieve necessary IDs from command context");
            }
            var language = await _dataBaseService.GetLangServer(contextGuildId.Value);
            Messages.Culture = new CultureInfo((language.Lang == Lang.Ru) ? "ru-RU" : "en-US" );

            Embed embed;

            if (!await _dataBaseService.GetUserInDataBase(nameUser.ID.Value, contextGuildId.Value))
            {
                embed = new Embed(Title: Messages.Error, Description: Messages.ErrorDontRemember, Colour: Color.Red);
                return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
            }

            if (await _dataBaseService.DeleteFromDataBase(nameUser.ID.Value, contextGuildId.Value))
            {
                embed = new Embed(Title: Messages.Successfully, Description: Messages.SuccessfullyForgetBirthday,
                    Colour: Color.Green);
            }
            else
            {
                embed = new Embed(Title: Messages.UnknownError, Colour: Color.Red);
            }

            return await _feedbackService.SendContextualEmbedResultAsync(embed, ct: CancellationToken);
        }
    }

