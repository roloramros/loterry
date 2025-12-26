using System.Windows;
using FloridaLotteryApp.Data;
using System.Windows.Controls;


namespace FloridaLotteryApp;

public partial class AddPick3Window : Window
{
    public AddPick3Window()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DatePickerDate.SelectedDate == null)
        {
            MessageBox.Show("Selecciona una fecha");
            return;
        }

        if (TxtNumber.Text.Length != 3 || !int.TryParse(TxtNumber.Text, out _))
        {
            MessageBox.Show("El número debe tener 3 dígitos");
            return;
        }

        var drawItem = ComboDrawTime.SelectedItem as ComboBoxItem;
        var drawTime = drawItem?.Tag?.ToString();

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

        ManualInsertRepository.InsertPick3(
            DatePickerDate.SelectedDate.Value,
            drawTime!,
            TxtNumber.Text,
            fireball
        );

        DialogResult = true;
        Close();
    }
}
