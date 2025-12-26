using Microsoft.Data.Sqlite;

namespace FloridaLotteryApp.Data;

public record Pick3Hit(DateTime Date, string DrawTime, string Number, int? Fireball);

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
}
