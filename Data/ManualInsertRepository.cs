using FloridaLotteryApp.Data;

namespace FloridaLotteryApp.Data;

public static class ManualInsertRepository
{
    public static void Insert(string game, DateTime date, string drawTime, string number, int? fireball)
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        if (game == "pick3")
        {
            cmd.CommandText = """
                INSERT INTO pick3_draws (draw_date, draw_time, number, n1, n2, n3, fireball)
                VALUES ($d, $t, $n, $n1, $n2, $n3, $fb);
            """;
            cmd.Parameters.AddWithValue("$n1", number[0] - '0');
            cmd.Parameters.AddWithValue("$n2", number[1] - '0');
            cmd.Parameters.AddWithValue("$n3", number[2] - '0');
        }
        else // pick4
        {
            cmd.CommandText = """
                INSERT INTO pick4_draws (draw_date, draw_time, number, n1, n2, n3, n4, fireball)
                VALUES ($d, $t, $n, $n1, $n2, $n3, $n4, $fb);
            """;
            cmd.Parameters.AddWithValue("$n1", number[0] - '0');
            cmd.Parameters.AddWithValue("$n2", number[1] - '0');
            cmd.Parameters.AddWithValue("$n3", number[2] - '0');
            cmd.Parameters.AddWithValue("$n4", number[3] - '0');
        }

        cmd.Parameters.AddWithValue("$d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$t", drawTime);
        cmd.Parameters.AddWithValue("$n", number);
        cmd.Parameters.AddWithValue("$fb", (object?)fireball ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }
}
