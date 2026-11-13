using System.Text.Json;
using Common.Application.Outbox;
using ErrorOr;
using MediatR;
using Media.Application.References;
using Media.Contracts;
using Media.Domain.Objects;

namespace Media.Infrastructure.Outbox;

/// <summary>
/// Обработчик сообщения `media.references.sync`:
/// разбирает payload контракта и передаёт его в прикладной сценарий синхронизации ссылок.
/// Повтор сообщения безопасен: набор декларативен.
/// </summary>
internal sealed class MediaReferencesSyncHandler(ISender sender) : IOutboxMessageHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string MessageType => MediaOutboxMessages.ReferencesSync;

    public async Task<ErrorOr<Success>> HandleAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        MediaReferencesSyncPayload? message;
        try
        {
            message = payload.Deserialize<MediaReferencesSyncPayload>(JsonOptions);
        }
        catch (JsonException)
        {
            return MediaErrors.MessageInvalid;
        }

        if (message is null || message.OwnerId == Guid.Empty || string.IsNullOrWhiteSpace(message.ElementScope))
            return MediaErrors.MessageInvalid;

        return await sender.Send(
            new SyncReferencesCommand(message.OwnerId, message.ElementScope, message.Slots ?? []),
            cancellationToken);
    }
}
