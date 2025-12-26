using FloridaLotteryApp.Data;

namespace FloridaLotteryApp.Data;

public static class ManualInsertRepository
{
    public static void InsertPick3(
        DateTime date,
        string drawTime,   // "M" o "E"
        string number,
        int? fireball
    )
    {
        using var conn = Db.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = """
        INSERT INTO pick3_draws
        (draw_date, draw_time, number, n1, n2, n3, fireball)
        VALUES
        ($d, $t, $n, $n1, $n2, $n3, $fb);
        """;

        cmd.Parameters.AddWithValue("$d", date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$t", drawTime);
        cmd.Parameters.AddWithValue("$n", number);
        cmd.Parameters.AddWithValue("$n1", number[0] - '0');
        cmd.Parameters.AddWithValue("$n2", number[1] - '0');
        cmd.Parameters.AddWithValue("$n3", number[2] - '0');
        cmd.Parameters.AddWithValue("$fb", (object?)fireball ?? DBNull.Value);

        cmd.ExecuteNonQuery();
    }
}
