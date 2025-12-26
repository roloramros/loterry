using Microsoft.Data.Sqlite;

namespace FloridaLotteryApp.Data;

public record Pick3Hit(DateTime Date, string DrawTime, string Number, int? Fireball);
public record Hit(DateTime Date, string Game, string DrawTime, string Number, int? Fireball);

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
}
