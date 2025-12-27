using Microsoft.Data.Sqlite;

namespace FloridaLotteryApp.Data;

public record Pick3Hit(DateTime Date, string DrawTime, string Number, int? Fireball);
public record Hit(DateTime Date, string Game, string DrawTime, string Number, int? Fireball);
public record AnalysisHit(DateTime Date, string DrawTime, string Pick3, string Pick4);

public static class DrawRepository
{
    public static DateTime? GetLastPick3Date()
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(draw_date) FROM pick3_draws;";
        var result = cmd.ExecuteScalar();
        if (result == null || result == DBNull.Value) return null;
        return DateTime.Parse(result.ToString()!);
    }

    // NUEVO: devuelve number + fireball
    public static (string? Number, int? Fireball) GetPick3Result(DateTime date, string drawTime)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT number, fireball
            FROM pick3_draws
            WHERE draw_date = $d AND draw_time = $t
            ORDER BY rowid DESC
            LIMIT 1;
        """;

        cmd.Parameters.AddWithValue("$d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$t", drawTime);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return (null, null);

        var number = r.IsDBNull(0) ? null : r.GetString(0);
        int? fb = r.IsDBNull(1) ? null : r.GetInt32(1);

        return (number, fb);
    }

    public static List<Pick3Hit> SearchPick3ByNumber(string number)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT draw_date, draw_time, number, fireball
            FROM pick3_draws
            WHERE number = $n
            ORDER BY draw_date DESC, draw_time DESC, rowid DESC;
        """;

        cmd.Parameters.AddWithValue("$n", number);

        using var r = cmd.ExecuteReader();
        var list = new List<Pick3Hit>();

        while (r.Read())
        {
            var d = DateTime.Parse(r.GetString(0));
            var t = r.GetString(1);
            var num = r.GetString(2);
            int? fb = r.IsDBNull(3) ? null : r.GetInt32(3);
            list.Add(new Pick3Hit(d, t, num, fb));
        }

        return list;
    }

    public static DateTime? GetLastDateOverall()
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT MAX(d) FROM (
              SELECT MAX(draw_date) AS d FROM pick3_draws
              UNION ALL
              SELECT MAX(draw_date) AS d FROM pick4_draws
            );
        """;

        var r = cmd.ExecuteScalar();
        if (r == null || r == DBNull.Value) return null;
        return DateTime.Parse(r.ToString()!);
    }

    public static (string? Number, int? Fireball) GetResult(string game, DateTime date, string drawTime)
    {
        // game: "pick3" o "pick4"
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = $"""
            SELECT number, fireball
            FROM {game}_draws
            WHERE draw_date = $d AND draw_time = $t
            ORDER BY rowid DESC
            LIMIT 1;
        """;

        cmd.Parameters.AddWithValue("$d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$t", drawTime);

        using var r = cmd.ExecuteReader();
        if (!r.Read()) return (null, null);

        var num = r.IsDBNull(0) ? null : r.GetString(0);
        int? fb = r.IsDBNull(1) ? null : r.GetInt32(1);
        return (num, fb);
    }

    public static List<Hit> SearchByNumberBoth(string number)
    {
        var hits = new List<Hit>();
        if (number.Length == 3)
            hits.AddRange(SearchByNumberGame("pick3", number));
        else if (number.Length == 4)
            hits.AddRange(SearchByNumberGame("pick4", number));
        else
            return hits;

        return hits;
    }

    private static List<Hit> SearchByNumberGame(string game, string number)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = $"""
            SELECT draw_date, draw_time, number, fireball
            FROM {game}_draws
            WHERE number = $n
            ORDER BY draw_date DESC, draw_time DESC, rowid DESC;
        """;
        cmd.Parameters.AddWithValue("$n", number);

        using var r = cmd.ExecuteReader();
        var list = new List<Hit>();
        while (r.Read())
        {
            list.Add(new Hit(
                DateTime.Parse(r.GetString(0)),
                game.ToUpper(),               // PICK3 / PICK4
                r.GetString(1),
                r.GetString(2),
                r.IsDBNull(3) ? null : r.GetInt32(3)
            ));
        }
        return list;
    }

    public static int CountDistinctDatesOverall()
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT COUNT(*) FROM (
              SELECT DISTINCT draw_date AS d FROM pick3_draws
              UNION
              SELECT DISTINCT draw_date AS d FROM pick4_draws
            );
        """;

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public static List<DateTime> GetDistinctDatesOverall(int limit, int offset)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT d FROM (
              SELECT DISTINCT draw_date AS d FROM pick3_draws
              UNION
              SELECT DISTINCT draw_date AS d FROM pick4_draws
            )
            ORDER BY d DESC
            LIMIT $limit OFFSET $offset;
        """;

        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.Parameters.AddWithValue("$offset", offset);

        using var r = cmd.ExecuteReader();
        var list = new List<DateTime>();
        while (r.Read())
            list.Add(DateTime.Parse(r.GetString(0)));

        return list;
    }

    public static List<AnalysisHit> FindPositionMatches(int p3Pos1Based, int p4Pos1Based)
    {
        // p3Pos1Based: 1..3
        // p4Pos1Based: 1..4

        string p3Col = p3Pos1Based switch { 1 => "p3.n1", 2 => "p3.n2", 3 => "p3.n3", _ => throw new ArgumentOutOfRangeException(nameof(p3Pos1Based)) };
        string p4Col = p4Pos1Based switch { 1 => "p4.n1", 2 => "p4.n2", 3 => "p4.n3", 4 => "p4.n4", _ => throw new ArgumentOutOfRangeException(nameof(p4Pos1Based)) };

        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        // Filtramos en SQL por la igualdad de dígito en esas posiciones para reducir candidatos.
        cmd.CommandText = $"""
            SELECT p3.draw_date, p3.draw_time, p3.number, p4.number,
                   p3.n1, p3.n2, p3.n3,
                   p4.n1, p4.n2, p4.n3, p4.n4
            FROM pick3_draws p3
            JOIN pick4_draws p4
              ON p3.draw_date = p4.draw_date
             AND p3.draw_time = p4.draw_time
            WHERE {p3Col} = {p4Col}
            ORDER BY p3.draw_date DESC, p3.draw_time DESC, p3.rowid DESC;
        """;

        using var r = cmd.ExecuteReader();
        var results = new List<AnalysisHit>();

        while (r.Read())
        {
            var date = DateTime.Parse(r.GetString(0));
            var drawTime = r.GetString(1);     // "M" o "E"
            var p3num = r.GetString(2);
            var p4num = r.GetString(3);

            int p3n1 = r.GetInt32(4);
            int p3n2 = r.GetInt32(5);
            int p3n3 = r.GetInt32(6);

            int p4n1 = r.GetInt32(7);
            int p4n2 = r.GetInt32(8);
            int p4n3 = r.GetInt32(9);
            int p4n4 = r.GetInt32(10);

            // Regla: dígitos independientes dentro de cada pick
            var p3 = new[] { p3n1, p3n2, p3n3 };
            if (p3.Distinct().Count() != 3) continue;

            var p4 = new[] { p4n1, p4n2, p4n3, p4n4 };
            if (p4.Distinct().Count() != 4) continue;

            // Regla: exactamente 1 dígito común entre Pick3 y Pick4
            var comunes = p3.Intersect(p4).ToList();
            if (comunes.Count != 1) continue;

            // Regla: el dígito común debe estar exactamente en las posiciones pedidas
            int expected = p3[p3Pos1Based - 1];
            if (expected != p4[p4Pos1Based - 1]) continue;       // ya debería por WHERE, pero lo dejamos seguro
            if (comunes[0] != expected) continue;

            results.Add(new AnalysisHit(date, drawTime, p3num, p4num));
        }

        return results;
    }

    public static DateTime? GetNextPick3Date(DateTime date)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
            SELECT MIN(draw_date)
            FROM pick3_draws
            WHERE draw_date > $d;
        """;

        cmd.Parameters.AddWithValue("$d", date.ToString("yyyy-MM-dd"));

        var r = cmd.ExecuteScalar();
        if (r == null || r == DBNull.Value) return null;
        return DateTime.Parse(r.ToString()!);
    }

    public static string? GetNextPick3Number(DateTime date, string drawTime)
    {
        // Si el hallazgo fue Mediodía (M), la próxima es Noche (E) del mismo día.
        if (drawTime == "M")
        {
            var nextSameDay = GetResult("pick3", date, "E").Number;
            return IsPick3Unique(nextSameDay) ? nextSameDay : null;
        }

        // Si el hallazgo fue Noche (E), la próxima es la primera tirada disponible del próximo día:
        // 1) intenta Mediodía (M)
        // 2) si no existe o es inválida, usa Noche (E)
        var nextDate = GetNextPick3Date(date);
        if (nextDate == null) return null;

        var nextMidday = GetResult("pick3", nextDate.Value, "M").Number;
        if (IsPick3Unique(nextMidday)) return nextMidday;

        var nextNight = GetResult("pick3", nextDate.Value, "E").Number;
        if (IsPick3Unique(nextNight)) return nextNight;

        return null;
    }

    private static bool IsPick3Unique(string? n)
    {
        if (string.IsNullOrWhiteSpace(n) || n.Length != 3) return false;
        if (!n.All(char.IsDigit)) return false;
        return n.Distinct().Count() == 3;
    }

}
