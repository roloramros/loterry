using System;
using System.Linq;
using System.Windows;
using FloridaLotteryApp.Data;

namespace FloridaLotteryApp;

public partial class SearchWindow : Window
{
    public SearchWindow()
    {
        InitializeComponent();
        DpDate.SelectedDate = DateTime.Today;
        ClearDateResults();
    }

    // ================= BUSCAR POR FECHA =================
    private void SearchByDate_Click(object sender, RoutedEventArgs e)
    {
        if (DpDate.SelectedDate == null)
        {
            MessageBox.Show("Selecciona una fecha");
            return;
        }

        var date = DpDate.SelectedDate.Value;

        // Pick 3
        TxtP3Night.Text  = Format(DrawRepository.GetResult("pick3", date, "E"));
        TxtP3Midday.Text = Format(DrawRepository.GetResult("pick3", date, "M"));

        // Pick 4
        TxtP4Night.Text  = Format(DrawRepository.GetResult("pick4", date, "E"));
        TxtP4Midday.Text = Format(DrawRepository.GetResult("pick4", date, "M"));
    }

    // ================= BUSCAR POR COMBINACIÓN =================
    private void SearchByNumber_Click(object sender, RoutedEventArgs e)
    {
        var n = TxtNumber.Text.Trim();

        if (!int.TryParse(n, out _) || (n.Length != 3 && n.Length != 4))
        {
            MessageBox.Show("Escribe 3 dígitos (Pick3) o 4 dígitos (Pick4)");
            return;
        }

        var hits = DrawRepository.SearchByNumberBoth(n);
        TxtCount.Text = $"Veces: {hits.Count}";

        LvHits.ItemsSource = hits.Select(h => new
        {
            Date = h.Date.ToString("yyyy-MM-dd"),
            Game = h.Game,
            h.DrawTime,
            h.Number,
            Fireball = h.Fireball?.ToString() ?? "-"
        }).ToList();
    }

    // ================= HELPERS =================
    private static string Format((string? Number, int? Fireball) r)
    {
        return r.Number == null
            ? "---"
            : $"{r.Number} | FB {(r.Fireball?.ToString() ?? "-")}";
    }

    private void ClearDateResults()
    {
        TxtP3Night.Text = "---";
        TxtP3Midday.Text = "---";
        TxtP4Night.Text = "---";
        TxtP4Midday.Text = "---";
    }
}
