namespace HttpLogging;

/// <summary>
/// Запись HTTP-лога.
/// </summary>
internal sealed class LogEntry
{
    /// <summary>
    /// Идентификатор записи.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// Момент начала запроса в UTC.
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Время выполнения запроса в миллисекундах.
    /// </summary>
    public double? Duration { get; init; }

    /// <summary>
    /// Уровень логирования.
    /// </summary>
    public string Level { get; init; } = string.Empty;

    /// <summary>
    /// Имя логгера.
    /// </summary>
    public string Logger { get; init; } = string.Empty;

    /// <summary>
    /// Основной текст.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Параметры основного текста.
    /// </summary>
    /// <remarks>
    /// JSON-строка
    /// </remarks>
    public string? MessageProperties { get; init; }

    /// <summary>
    /// Тип ошибки.
    /// </summary>
    public string? ExceptionType { get; init; }
    
    /// <summary>
    /// Сообщение ошибки.
    /// </summary>
    public string? ExceptionMessage { get; init; }

    /// <summary>
    /// Тип первопричины (корневой ошибки цепочки).
    /// </summary>
    public string? RootExceptionType { get; init; }

    /// <summary>
    /// Сообщение первопричины.
    /// </summary>
    public string? RootExceptionMessage { get; init; }

    /// <summary>
    /// Стектрейс, обрезанный согласно форматирования.
    /// </summary>
    public string? StackTraceShort { get; init; }

    /// <summary>
    /// Полный стектрейс, сжатый.
    /// </summary>
    public byte[]? StackTraceCompressed { get; init; }

    /// <summary>
    /// IP-адрес клиента
    /// </summary>
    public string? ClientIp { get; init; }

    /// <summary>
    /// Имя HTTP-метода.
    /// </summary>
    /// <example>
    /// GET, POST, ...
    /// </example>
    public string? HttpMethod { get; init; }

    /// <summary>
    /// URI без query-параметров.
    /// </summary>
    public string? RequestPath { get; init; }

    /// <summary>
    /// Query-параметры URI.
    /// </summary>
    public string? RequestQuery { get; init; }

    /// <summary>
    /// Статусный код HTTP-ответа.
    /// </summary>
    public int? ResponseStatusCode { get; init; }

    /// <summary>
    /// Заголовки запроса, обрезанные согласно форматирования.
    /// </summary>
    public string? RequestHeadersShort { get; init; }
    
    /// <summary>
    /// Заголовки запроса, сжатые.
    /// </summary>
    public byte[]? RequestHeadersCompressed { get; init; }

    /// <summary>
    /// Тело запроса, обрезанное согласно форматирования.
    /// </summary>
    public string? RequestBodyShort { get; init; }
    
    /// <summary>
    /// Тело запроса, сжатое.
    /// </summary>
    public byte[]? RequestBodyCompressed { get; init; }

    /// <summary>
    /// Заголовки ответа, обрезанные согласно форматирования.
    /// </summary>
    public string? ResponseHeadersShort { get; init; }
    
    /// <summary>
    /// Заголовки ответа, сжатые.
    /// </summary>
    public byte[]? ResponseHeadersCompressed { get; init; }

    /// <summary>
    /// Тело ответа, обрезанное согласно форматирования.
    /// </summary>
    public string? ResponseBodyShort { get; init; }
    
    /// <summary>
    /// Тело ответа, сжатое.
    /// </summary>
    public byte[]? ResponseBodyCompressed { get; init; }

    /// <summary>
    /// Глобальный идентификатор цепочки запросов.
    /// </summary>
    public string? TraceId { get; init; }
    
    /// <summary>
    /// Локальный идентификатор вызывающей операции.
    /// </summary>
    public string? ParentSpanId { get; init; }
    
    /// <summary>
    /// Локальный идентификатор текущей операции.
    /// </summary>
    public string? SpanId { get; init; }
    
    /// <summary>
    /// Роль участника.
    /// </summary>
    /// <example>
    /// Server, Client, Internal.
    /// </example>
    public string? SpanKind { get; init; }

    /// <summary>
    /// Имя сервиса.
    /// </summary>
    /// <example>
    /// app, bff, ...
    /// </example>
    public string ServiceName { get; init; } = string.Empty;
}
