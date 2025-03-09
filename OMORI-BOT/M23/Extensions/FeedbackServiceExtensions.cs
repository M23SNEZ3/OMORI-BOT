using Remora.Discord.Pagination;
using Remora.Discord.Pagination.Extensions;
using Remora.Rest.Core;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Messages;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace OMORI_BOT.M23.Extensions
{
    public static class FeedbackServiceExtensions
    {
        /// <summary>
        ///     Extensions for Feedback Service
        /// </summary>
        public static async Task<Result> SendContextualEmbedResultAsync(
            this IFeedbackService feedback, Result<Embed> embedResult,
            FeedbackMessageOptions? options = null, CancellationToken ct = default)
        {
            if (!embedResult.IsDefined(out var embed))
            {
                return Result.FromError(embedResult);
            }

            return (Result)await feedback.SendContextualEmbedAsync(embed, options, ct);
        }

        public static async Task<Result> SendContextualPaginatedResultAsync(
            this FeedbackService feedback,
            Snowflake sourceUser,
            Result<IReadOnlyList<Embed>> pages,
            PaginatedAppearanceOptions? appearance = null,
            FeedbackMessageOptions? options = null,
            CancellationToken ct = default)
        {
            if (!pages.IsDefined(out var embedPages))
            {
                return Result.FromError(pages);
            }

            return (Result)await feedback.SendContextualPaginatedMessageAsync(sourceUser, embedPages, appearance,
                options, ct);
        }
    }
}
