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
    var lastDate = DrawRepository.GetLastPick3Date();

    if (lastDate == null)
    {
        TxtDate.Text = "No hay datos";

        TxtPick3Night.Text = "---";
        TxtFbNight.Text = "-";

        TxtPick3Midday.Text = "---";
        TxtFbMidday.Text = "-";
        return;
    }

    TxtDate.Text = lastDate.Value.ToString("yyyy-MM-dd");

    // Noche = "E"
    var night = DrawRepository.GetPick3Result(lastDate.Value, "E");
    TxtPick3Night.Text = night.Number ?? "---";
    TxtFbNight.Text = night.Fireball?.ToString() ?? "-";

    // Mediodía = "M"
    var midday = DrawRepository.GetPick3Result(lastDate.Value, "M");
    TxtPick3Midday.Text = midday.Number ?? "---";
    TxtFbMidday.Text = midday.Fireball?.ToString() ?? "-";
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
