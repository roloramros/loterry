using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using FloridaLotteryApp.Data;
using System.Linq;

namespace FloridaLotteryApp;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<DayCard> DayCards { get; } = new();

    private const int PageSize = 10;
    private int _pageIndex = 0;
    private int _totalDates = 0;

    private bool _canPrev;
    public bool CanPrev { get => _canPrev; set { _canPrev = value; OnPropertyChanged(); } }

    private bool _canNext;
    public bool CanNext { get => _canNext; set { _canNext = value; OnPropertyChanged(); } }

    private string _pageText = "";
    public string PageText { get => _pageText; set { _pageText = value; OnPropertyChanged(); } }

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        LoadPage(0);
    }

    private void LoadPage(int pageIndex)
    {
        _totalDates = DrawRepository.CountDistinctDatesOverall();

        _pageIndex = Math.Max(0, pageIndex);

        var dates = DrawRepository.GetDistinctDatesOverall(PageSize, _pageIndex * PageSize);

        DayCards.Clear();

        foreach (var d in dates)
        {
            var p3m = DrawRepository.GetResult("pick3", d, "M");
            var p3e = DrawRepository.GetResult("pick3", d, "E");
            var p4m = DrawRepository.GetResult("pick4", d, "M");
            var p4e = DrawRepository.GetResult("pick4", d, "E");

            DayCards.Add(DayCard.From(d, p3m, p3e, p4m, p4e));
        }

        CanPrev = _pageIndex > 0;
        CanNext = (_pageIndex + 1) * PageSize < _totalDates;

        var totalPages = Math.Max(1, (int)Math.Ceiling(_totalDates / (double)PageSize));
        PageText = $"Página {_pageIndex + 1} / {totalPages}  (10 por página)";
    }

    private void Prev_Click(object sender, RoutedEventArgs e) => LoadPage(_pageIndex - 1);
    private void Next_Click(object sender, RoutedEventArgs e) => LoadPage(_pageIndex + 1);

    private void AddManual_Click(object sender, RoutedEventArgs e)
    {
        var win = new AddPick3Window { Owner = this };
        if (win.ShowDialog() == true)
        {
            // refresca página actual (por si insertaste una fecha reciente)
            LoadPage(_pageIndex);
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        var win = new SearchWindow { Owner = this };
        win.ShowDialog();
    }

    private void Analysis_Click(object sender, RoutedEventArgs e)
    {
        var p3 = (TxtManualP3.Text ?? "").Trim();
        var p4 = (TxtManualP4.Text ?? "").Trim();

        // Validación básica de longitud y numérico (para que no reviente después)
        if (p3.Length != 3 || !p3.All(char.IsDigit))
        {
            MessageBox.Show("Pick 3 inválido. Debe ser 3 dígitos (ej: 234).");
            return;
        }
        if (p4.Length != 4 || !p4.All(char.IsDigit))
        {
            MessageBox.Show("Pick 4 inválido. Debe ser 4 dígitos (ej: 2458).");
            return;
        }
        // Regla 1: NO repetir dígitos dentro de Pick3
        if (p3.Distinct().Count() != 3)
        {
            MessageBox.Show("Pick 3 inválido: no se pueden repetir dígitos.");
            return;
        }
        // Regla 1: NO repetir dígitos dentro de Pick4
        if (p4.Distinct().Count() != 4)
        {
            MessageBox.Show("Pick 4 inválido: no se pueden repetir dígitos.");
            return;
        }
        // Regla 2: debe repetirse EXACTAMENTE 1 dígito entre Pick3 y Pick4
        var comunes = p3.Intersect(p4).ToList();

        if (comunes.Count != 1)
        {
            MessageBox.Show($"Inválido: debe repetirse EXACTAMENTE 1 dígito entre Pick 3 y Pick 4. Ahora se repiten: {comunes.Count}");
            return;
        }

        char d = comunes[0];
        int posP3 = p3.IndexOf(d) + 1; // 1..3
        int posP4 = p4.IndexOf(d) + 1; // 1..4
        // BÚSQUEDA EN BD
        var hits = FloridaLotteryApp.Data.DrawRepository.FindPositionMatches(posP3, posP4);
        var rows = hits.Select(h => new
            {
                Hit = h,
                NextPick3 = DrawRepository.GetNextPick3Number(h.Date, h.DrawTime)
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.NextPick3)) // <-- si no hay próxima válida, NO entra
            .Select(x => new AnalysisRow
            {
                IsChecked = false,
                Date = x.Hit.Date.ToString("yyyy-MM-dd"),
                DrawTime = x.Hit.DrawTime == "M" ? "☀️" : "🌙",
                Pick3 = x.Hit.Pick3,
                Pick4 = x.Hit.Pick4,

                NextPick3 = x.NextPick3!, // ya está validado
                Coding = BuildCoding(x.Hit.Pick3, x.Hit.Pick4)
            })
            .ToList();
        
        var guide = new GuideInfo
        {
            Pick3 = p3,
            Pick4 = p4,
            Coding = BuildCoding(p3, p4),
            DateText = DateTime.Today.ToString("yyyy-MM-dd"), // por ahora hoy
            DrawIcon = "", // si luego quieres elegir ☀️/🌙 en la entrada, aquí lo ponemos
            RepPosP3 = posP3,
            RepPosP4 = posP4
        };

        var win = new AnalysisCardsWindow(guide, rows) { Owner = this };
        win.ShowDialog();
        
        
        
        
        
        //var header = $"Coincidencias: Pick3 pos {posP3} ↔ Pick4 pos {posP4} (Resultados: {rows.Count})";
        //var win = new AnalysisResultsWindow(header, rows) { Owner = this };
        //win.ShowDialog();

    }

    private static string BuildCoding(string pick3, string pick4)
    {
        // Unimos dígitos, quitamos duplicados, ordenamos ascendente
        return new string((pick3 + pick4)
            .Where(char.IsDigit)
            .Distinct()
            .OrderBy(c => c)
            .ToArray());
    }





    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}






public class DayCard
{
    public string DateText { get; set; } = "";

    public string P3_M_Number { get; set; } = "---";
    public string P3_M_FB { get; set; } = "FB";
    public string P3_E_Number { get; set; } = "---";
    public string P3_E_FB { get; set; } = "FB";

    public string P4_M_First2 { get; set; } = "--";
    public string P4_M_Last2  { get; set; } = "--";
    public string P4_M_FB     { get; set; } = "FB";

    public string P4_E_First2 { get; set; } = "--";
    public string P4_E_Last2  { get; set; } = "--";
    public string P4_E_FB     { get; set; } = "FB";

    public static DayCard From(
        DateTime date,
        (string? Number, int? Fireball) p3m,
        (string? Number, int? Fireball) p3e,
        (string? Number, int? Fireball) p4m,
        (string? Number, int? Fireball) p4e)
    {
        var card = new DayCard
        {
            DateText = date.ToString("yyyy-MM-dd")
        };

        // Pick3
        card.P3_M_Number = p3m.Number ?? "---";
        card.P3_M_FB = p3m.Fireball?.ToString() ?? "-";

        card.P3_E_Number = p3e.Number ?? "---";
        card.P3_E_FB = p3e.Fireball?.ToString() ?? "-";

        // Pick4 split
        (card.P4_M_First2, card.P4_M_Last2) = Split2(p4m.Number);
        card.P4_M_FB = p4m.Fireball?.ToString() ?? "-";

        (card.P4_E_First2, card.P4_E_Last2) = Split2(p4e.Number);
        card.P4_E_FB = p4e.Fireball?.ToString() ?? "-";

        return card;
    }

    private static (string, string) Split2(string? number)
    {
        if (string.IsNullOrWhiteSpace(number) || number.Length < 4) return ("--", "--");
        return (number.Substring(0, 2), number.Substring(2, 2));
    }

    
}
