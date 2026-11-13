namespace Media.Application.Objects;

/// <summary>Тело запроса на инициализацию загрузки: что клиент собирается отправить.</summary>
/// <param name="ContentType">Тип файла из белого списка.</param>
/// <param name="SizeBytes">Размер в байтах.</param>
public sealed record InitUploadRequest(string? ContentType, long? SizeBytes);
