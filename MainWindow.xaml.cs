using System.Windows;
using FloridaLotteryApp.Data;

namespace FloridaLotteryApp;

public partial class MainWindow : Window
{
    public MainWindow()
{
    InitializeComponent();

    try
    {
        LoadLastDay();
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.ToString(), "Error al iniciar");
    }
}


    private void LoadLastDay()
{
    var lastDate = DrawRepository.GetLastDateOverall();

    if (lastDate == null)
    {
        TxtDate.Text = "No hay datos";
        TxtP3Night.Text = "---";
        TxtP3Midday.Text = "---";
        TxtP4Night.Text = "---";
        TxtP4Midday.Text = "---";
        return;
    }

    TxtDate.Text = lastDate.Value.ToString("yyyy-MM-dd");

    TxtP3Night.Text  = Format(DrawRepository.GetResult("pick3", lastDate.Value, "E"));
    TxtP3Midday.Text = Format(DrawRepository.GetResult("pick3", lastDate.Value, "M"));

    TxtP4Night.Text  = Format(DrawRepository.GetResult("pick4", lastDate.Value, "E"));
    TxtP4Midday.Text = Format(DrawRepository.GetResult("pick4", lastDate.Value, "M"));
}

private static string Format((string? Number, int? Fireball) r)
{
    return r.Number == null ? "---" : $"{r.Number} | FB {(r.Fireball?.ToString() ?? "-")}";
}



    private void AddManual_Click(object sender, RoutedEventArgs e)
    {
        var win = new AddPick3Window { Owner = this };
        if (win.ShowDialog() == true)
        {
            LoadLastDay(); // refresca al guardar
        }
    }

    private void Search_Click(object sender, RoutedEventArgs e)
{
    var win = new SearchWindow { Owner = this };
    win.ShowDialog();
}

}
