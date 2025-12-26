using System.Windows;
using FloridaLotteryApp.Data;
using System.Windows.Controls;


namespace FloridaLotteryApp;

public partial class AddPick3Window : Window
{
    public AddPick3Window()
    {
        InitializeComponent();
        DatePickerDate.SelectedDate = DateTime.Today;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DatePickerDate.SelectedDate == null)
        {
            MessageBox.Show("Selecciona una fecha");
            return;
        }

        var drawItem = ComboDrawTime.SelectedItem as ComboBoxItem;
        var drawTime = drawItem?.Tag?.ToString();
        var gameItem = ComboGame.SelectedItem as System.Windows.Controls.ComboBoxItem;
        var game = gameItem?.Tag?.ToString() ?? "pick3";
        int requiredDigits = game == "pick3" ? 3 : 4;

        if (TxtNumber.Text.Length != requiredDigits || !int.TryParse(TxtNumber.Text, out _))
        {
            MessageBox.Show($"El número debe tener {requiredDigits} dígitos");
            return;
        }


        int? fireball = null;
        if (!string.IsNullOrWhiteSpace(TxtFireball.Text))
        {
            if (!int.TryParse(TxtFireball.Text, out var fb))
            {
                MessageBox.Show("Fireball inválido");
                return;
            }
            fireball = fb;
        }

        ManualInsertRepository.Insert(
            game,
            DatePickerDate.SelectedDate.Value,
            drawTime!,
            TxtNumber.Text,
            fireball
        );


        DialogResult = true;
        Close();
    }
}
