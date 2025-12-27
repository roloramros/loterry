using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FloridaLotteryApp;

public partial class AnalysisCardsWindow : Window
{
    public ObservableCollection<AnalysisPairCardVM> Cards { get; } = new();

    public AnalysisCardsWindow(GuideInfo guide, IEnumerable<AnalysisRow> resultRows)
    {
        InitializeComponent();
        DataContext = this;

        foreach (var r in resultRows)
            Cards.Add(AnalysisPairCardVM.Create(guide, r));
    }
}

public class GuideInfo
{
    public string Pick3 { get; set; } = "";
    public string Pick4 { get; set; } = "";
    public string Coding { get; set; } = "";
    public string DateText { get; set; } = "";     // yyyy-MM-dd
    public string DrawIcon { get; set; } = "";     // ☀️ / 🌙
    public int RepPosP3 { get; set; }
    public int RepPosP4 { get; set; }
}

public class AnalysisPairCardVM
{

    private static readonly Brush HighlightBrush =
    new SolidColorBrush(Color.FromRgb(255, 140, 0)); // amarillo tenue


    // GUÍA (igual en todas las cards)
    public string GuideDateText { get; set; } = "";
    public string GuideDrawIcon { get; set; } = "";
    public ObservableCollection<DigitVM> GuidePick3Digits { get; set; } = new();
    public ObservableCollection<DigitVM> GuidePick4Digits { get; set; } = new();
    public ObservableCollection<DigitVM> GuideSpacerDigits { get; set; } = new();   // 3 vacíos
    public ObservableCollection<DigitVM> GuideCodingDigits { get; set; } = new();
    

    // RESULTADO
    public string ResDateText { get; set; } = "";
    public string ResDrawIcon { get; set; } = "";
    public ObservableCollection<DigitVM> ResPick3Digits { get; set; } = new();
    public ObservableCollection<DigitVM> ResPick4Digits { get; set; } = new();
    public ObservableCollection<DigitVM> ResNextPick3Digits { get; set; } = new();
    public ObservableCollection<DigitVM> ResCodingDigits { get; set; } = new();

    public static AnalysisPairCardVM Create(GuideInfo guide, AnalysisRow r)
    {
        var vm = new AnalysisPairCardVM
        {
            GuideDateText = guide.DateText,
            GuideDrawIcon = guide.DrawIcon,
            GuidePick3Digits = DigitsFrom(guide.Pick3, 3),
            GuidePick4Digits = DigitsFrom(guide.Pick4, 4),
            GuideSpacerDigits = BlankDigits(3),
            GuideCodingDigits = DigitsFrom(guide.Coding, 6),

            ResDateText = r.Date,
            ResDrawIcon = r.DrawTime, // ☀️/🌙
            ResPick3Digits = DigitsFrom(r.Pick3, 3),
            ResPick4Digits = DigitsFrom(r.Pick4, 4),
            ResNextPick3Digits = DigitsFrom(r.NextPick3, 3),
            ResCodingDigits = DigitsFrom(r.Coding, 6),
        };

        // Colorear POSICIONES (no el valor): en Guía y Resultado
        HighlightPosition(vm.GuidePick3Digits, guide.RepPosP3);
        HighlightPosition(vm.GuidePick4Digits, guide.RepPosP4);
        HighlightPosition(vm.ResPick3Digits, guide.RepPosP3);
        HighlightPosition(vm.ResPick4Digits, guide.RepPosP4);

        return vm;
    }

    private static ObservableCollection<DigitVM> DigitsFrom(string s, int count)
    {
        s = (s ?? "").Trim();
        var list = new ObservableCollection<DigitVM>();
        for (int i = 0; i < count; i++)
        {
            var val = (i < s.Length) ? s[i].ToString() : "";
            list.Add(new DigitVM { Value = val });
        }
        return list;
    }

    private static ObservableCollection<DigitVM> BlankDigits(int count)
    {
        var list = new ObservableCollection<DigitVM>();
        for (int i = 0; i < count; i++)
            list.Add(new DigitVM { Value = "" });
        return list;
    }

    private static void HighlightPosition(ObservableCollection<DigitVM> list, int pos1Based)
    {
        int idx = pos1Based - 1;
        if (idx < 0 || idx >= list.Count) return;
        list[idx].Bg = HighlightBrush;
    }

}

public class DigitVM
{
    public string Value { get; set; } = "";
    public Brush Bg { get; set; } = Brushes.Transparent;
}
