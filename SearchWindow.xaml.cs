using System.Windows;
using FloridaLotteryApp.Data;

namespace FloridaLotteryApp;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        DpDate.SelectedDate = System.DateTime.Today;
    }

    private void SearchByDate_Click(object sender, RoutedEventArgs e)
    {
        if (DpDate.SelectedDate is null)
        {
            MessageBox.Show("Selecciona una fecha");
            return;
        }

        var date = DpDate.SelectedDate.Value;

        var night = DrawRepository.GetPick3Result(date, "E");
        TxtNight.Text = FormatResult(night.Number, night.Fireball);

        var midday = DrawRepository.GetPick3Result(date, "M");
        TxtMidday.Text = FormatResult(midday.Number, midday.Fireball);
    }

    private void SearchByNumber_Click(object sender, RoutedEventArgs e)
    {
        var n = TxtNumber.Text.Trim();

        if (n.Length != 3 || !int.TryParse(n, out _))
        {
            MessageBox.Show("La combinación debe tener 3 dígitos (ej: 123)");
            return;
        }

        var hits = DrawRepository.SearchPick3ByNumber(n);

        TxtCount.Text = $"Veces: {hits.Count}";
        LvHits.ItemsSource = hits.Select(h => new
        {
            Date = h.Date.ToString("yyyy-MM-dd"),
            h.DrawTime,
            h.Number,
            Fireball = h.Fireball?.ToString() ?? "-"
        }).ToList();
    }

    private static string FormatResult(string? number, int? fb)
        => (number is null) ? "---" : $"{number} | FB {(fb?.ToString() ?? "-")}";
}
