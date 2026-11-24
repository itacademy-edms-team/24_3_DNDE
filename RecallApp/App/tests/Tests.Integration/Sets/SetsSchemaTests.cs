using Npgsql;
using Sets.Domain.Sets;

namespace Tests.Integration.Sets;

/// <summary>
/// Сверка схемы sets с константами домена: длины колонок не должны разъезжаться
/// с <see cref="SetConstraints"/> (ADR 0038).
/// </summary>
public sealed class SetsSchemaTests
{
    [DatabaseFact]
    public async Task SetsTable_ColumnLengths_MatchDomainConstraints()
    {
        await using var connection = new NpgsqlConnection(
            Environment.GetEnvironmentVariable("ConnectionStrings__AccountsMigrations"));
        await connection.OpenAsync();

        var lengths = await ReadCharacterLengthsAsync(connection);

        Assert.Equal(SetConstraints.TitleMaxLength, lengths["title"]);
        Assert.Equal(SetConstraints.DescriptionMaxLength, lengths["description"]);
    }

    private static async Task<Dictionary<string, int>> ReadCharacterLengthsAsync(NpgsqlConnection connection)
    {
        await using var command = new NpgsqlCommand(
            """
            SELECT a.attname, a.atttypmod - 4
            FROM pg_attribute a
            JOIN pg_class c ON c.oid = a.attrelid
            JOIN pg_namespace n ON n.oid = c.relnamespace
            WHERE n.nspname = 'sets' AND c.relname = 'sets'
              AND a.attnum > 0 AND NOT a.attisdropped AND a.atttypid = 'varchar'::regtype
            """,
            connection);

        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result[reader.GetString(0)] = reader.GetInt32(1);

        return result;
    }
}
