using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sets.Infrastructure.Migrations
{
    /// <summary>
    /// Переводит позицию элемента из плотного целого числа в строковый ранг (LexoRank).
    /// Существующие строки получают ранги в текущем порядке: <c>0|000001:</c>, <c>0|000002:</c> и т.д.
    /// </summary>
    public partial class ElementPositionRanks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Новый столбец: строки сортируются побайтово (COLLATE "C") - так же, как считает генератор рангов.
            migrationBuilder.Sql("""
                ALTER TABLE sets.elements ADD COLUMN position_rank text COLLATE "C";
                """);

            // 2. Пересчёт существующих позиций.
            migrationBuilder.Sql("""
                UPDATE sets.elements AS e
                SET position_rank = '0|' || lpad(numbered.rn::text, 6, '0') || ':'
                FROM (
                    SELECT id, row_number() OVER (PARTITION BY set_id ORDER BY position, id) AS rn
                    FROM sets.elements
                ) AS numbered
                WHERE e.id = numbered.id;
                """);

            // 3. Замена столбца: старые индексы уходят вместе с ним.
            migrationBuilder.Sql("""
                ALTER TABLE sets.elements DROP COLUMN position;
                ALTER TABLE sets.elements RENAME COLUMN position_rank TO position;
                ALTER TABLE sets.elements ALTER COLUMN position SET NOT NULL;
                """);

            // 4. Новые индексы
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ux_elements_set_id_position
                    ON sets.elements (set_id, position)
                    WHERE deleted_at IS NULL;

                CREATE INDEX ix_elements_set_id_position_id
                    ON sets.elements (set_id, position, id)
                    WHERE deleted_at IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE sets.elements ADD COLUMN position_int integer;

                UPDATE sets.elements AS e
                SET position_int = numbered.rn
                FROM (
                    SELECT id, row_number() OVER (PARTITION BY set_id ORDER BY position, id) AS rn
                    FROM sets.elements
                ) AS numbered
                WHERE e.id = numbered.id;

                ALTER TABLE sets.elements DROP COLUMN position;
                ALTER TABLE sets.elements RENAME COLUMN position_int TO position;
                ALTER TABLE sets.elements ALTER COLUMN position SET NOT NULL;

                CREATE UNIQUE INDEX ux_elements_set_id_position
                    ON sets.elements (set_id, position)
                    WHERE deleted_at IS NULL;

                CREATE INDEX ix_elements_set_id_position_id
                    ON sets.elements (set_id, position, id)
                    WHERE deleted_at IS NULL;
                """);
        }
    }
}
