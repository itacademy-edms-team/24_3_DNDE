namespace Sets.Application.Abstractions;

/// <summary>
/// Запись сообщения для другого модуля. Вызывается в той же транзакции, что и бизнес-изменение:
/// сообщение либо сохраняется вместе с ним, либо откатывается вместе с ним (ADR 0040).
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Добавляет сообщение в текущую единицу работы. Сохранение выполняет вызывающий код
    /// через свою транзакцию — отдельного коммита здесь нет.
    /// </summary>
    /// <param name="type">Тип сообщения из контракта потребителя.</param>
    /// <param name="payload">Тело сообщения: сериализуется в jsonb как есть.</param>
    void Enqueue(string type, object payload);
}
