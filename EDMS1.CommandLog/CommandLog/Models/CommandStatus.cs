namespace EDMS1.CommandLog.Models;

/// <summary>
/// Статус команды.
/// </summary>
public enum CommandStatus
{
    /// <summary>
    /// Успешно.
    /// </summary>
    Successful = 0,

    /// <summary>
    /// Неудачно.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Команда поставлена на повторную обработку.
    /// </summary>
    Retry = 2,

    /// <summary>
    /// Повторно обработан.
    /// </summary>
    RetryProcessed = 3,

    /// <summary>
    /// Завершена с отклонением от нормального пути выполнения (операция уже выполнена, должна быть отфильтрована и т.п.)
    /// </summary>
    CompletedWithVariation = 4
}
