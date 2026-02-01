using LiteDB;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_LiteDB
{
    public class UserData
    {
        public int Id { get; set; }
        private string? textValue;
        private int? intValue;
        private double? doubleValue;
        private Boolean? boolValue;
        private DateTime dateValue;        
        public string TextValue { get => textValue ?? ""; set { textValue = value; } }
        public int? IntValue { get => intValue; set { intValue = value; } }
        public double? DoubleValue { get => doubleValue; set { doubleValue = value; } }
        public Boolean? BoolValue { get => boolValue; set { boolValue = value; } }
        public DateTime DateValue { get => dateValue; set { dateValue = value; } }        
    }    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly bool is_initialize = true;
        bool is_filter = false;
        readonly static string sql_tec_kat = System.Environment.CurrentDirectory + @"\WPF_LiteDB.db";
        int? DataGrig_Id;

        public MainWindow()
        {
            InitializeComponent();

            is_initialize = false;            
            UpdateDatagrid();
        }


        // Одно ленивое подключение на всё приложение
        private static readonly Lazy<LiteDatabase> _database =
            new(() => new LiteDatabase(sql_tec_kat));

        // Чтение базы данных
        public static ILiteCollection<UserData> GetUsersCollection()
        {
            // База не закрывается после вызова, файл удерживается программой
            return _database.Value.GetCollection<UserData>("UserData");
        }
        
        private void UpdateDatagrid()
        {
            if (is_initialize == true) return;

            var collection = GetUsersCollection();
            
            if (is_filter == false)
            {                
                DataGrid1.ItemsSource = collection.FindAll().ToList();
            }
            else
            {
                String m_value1 = value1.Text.ToString();
                String m_value2 = value2.Text.ToString();
                bool m_value1_bool;
                bool m_er;

                if (value_type.Text == "id")
                {
                    m_er = int.TryParse(m_value1, out int m_value1_int);
                    var user = collection.FindById(m_value1_int);
                    DataGrid1.ItemsSource = user != null
                        ? [user]
                        : new List<UserData>();
                }
                else if (value_type.Text == "text")
                {
                    DataGrid1.ItemsSource = collection.FindAll().Where(p => p.TextValue.Contains(m_value1));
                }
                else if (value_type.Text == "int")
                {
                    m_er = int.TryParse(m_value1, out int m_value1_int);
                    m_er = int.TryParse(m_value2, out int m_value2_int);
                    DataGrid1.ItemsSource = collection.FindAll().Where(p => p.IntValue >= m_value1_int && p.IntValue <= m_value2_int);
                }
                else if (value_type.Text == "double")
                {
                    m_er = double.TryParse(m_value1, out double m_value1_dbl);
                    m_er = double.TryParse(m_value2, out double m_value2_dbl);
                    DataGrid1.ItemsSource = collection.FindAll().Where(p => p.DoubleValue >= m_value1_dbl && p.DoubleValue <= m_value2_dbl);
                }
                else if (value_type.Text == "bool")
                {
                    m_value1_bool = false;
                    if (m_value1.Equals("T", StringComparison.CurrentCultureIgnoreCase) || m_value1.Equals("true", StringComparison.CurrentCultureIgnoreCase)) m_value1_bool = true;
                    DataGrid1.ItemsSource = collection.FindAll().Where(p => p.BoolValue == m_value1_bool);
                }
                else if (value_type.Text == "date")
                {
                    m_er = DateTime.TryParseExact(m_value1, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value1_dat);
                    m_er = DateTime.TryParseExact(m_value2, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime m_value2_dat);
                    m_value2_dat = m_value2_dat.AddDays(1);
                    DataGrid1.ItemsSource = collection.FindAll().Where(p => p.DateValue >= m_value1_dat && p.DateValue < m_value2_dat);
                }
            }
            this.DataContext = DataGrid1.ItemsSource;

            // Выделить сроку с курсором
            if (DataGrig_Id == null && DataGrid1.Items.Count > 0) DataGrig_Id = null;

            if (DataGrig_Id != null && DataGrid1.Items.Count > 0)
            {
                foreach (UserData drv in DataGrid1.ItemsSource)
                {
                    if (drv.Id == DataGrig_Id)
                    {
                        DataGrid1.SelectedItem = drv;
                        DataGrid1.ScrollIntoView(drv);
                        DataGrid1.Focus();
                        break;
                    }
                }
            }
        }

        // изменение типа базы данных
        private void Database_type_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            //ComboBox comboBox = (ComboBox)sender;
            //ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;            
            //try
            //{
            //    UpdateDatagrid();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
            //    DataGrid1.ItemsSource = null;
            //}
        }

        // добавить запись
        private void Button_insertClick(object sender, RoutedEventArgs e)
        {
            AddWindow addWin = new(new UserData());
            if (addWin.ShowDialog() == true)
            {
                UserData ud = addWin.UserDataAdd;
                try
                {
                    var collection = GetUsersCollection();
                    collection.Insert(ud);
                    UpdateDatagrid();
                }
                catch (Exception ex)
                {
                    MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                    DataGrid1.ItemsSource = null;
                }
            }
        }

        // изменить запись
        private void Button_updateClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;
            // получаем выделенный объект
            if (DataGrid1.SelectedItem is UserData ud)
            {
                AddWindow addWin = new(new UserData
                {
                    Id = ud.Id,
                    TextValue = ud.TextValue,
                    IntValue = ud.IntValue,
                    DoubleValue = ud.DoubleValue,
                    BoolValue = ud.BoolValue,
                    DateValue = ud.DateValue
                });

                if (addWin.ShowDialog() == true)
                {
                    // получаем измененный объект                
                    try
                    {                        
                        var collection = GetUsersCollection();
                        var userToUpdate = collection.FindById(ud.Id);
                        if (userToUpdate != null)
                        {                            
                            userToUpdate.TextValue = addWin.UserDataAdd.TextValue;
                            userToUpdate.IntValue = addWin.UserDataAdd.IntValue;
                            userToUpdate.DoubleValue = addWin.UserDataAdd.DoubleValue;
                            userToUpdate.BoolValue = addWin.UserDataAdd.BoolValue;
                            userToUpdate.DateValue = addWin.UserDataAdd.DateValue;
                            
                            collection.Update(userToUpdate);
                        }                        
                        UpdateDatagrid();
                        MessageBox("Запись обновлена");
                    }
                    catch (Exception ex)
                    {
                        MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                        DataGrid1.ItemsSource = null;
                    }
                }
            }
        }

        // удалить запись
        private void Button_deleteClick(object sender, RoutedEventArgs e)
        {
            // если ни одного объекта не выделено, выходим
            if (DataGrid1.SelectedItem == null) return;

            MessageBoxResult result = System.Windows.MessageBox.Show("Удалить запись ???", "Сообщение", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // получаем выделенный объект
                    if (DataGrid1.SelectedItem is UserData ud)
                    {
                        try
                        {
                            var collection = GetUsersCollection();
                            collection.Delete(ud.Id);                            
                            UpdateDatagrid();
                        }
                        catch (Exception ex)
                        {
                            MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                            DataGrid1.ItemsSource = null;
                        }
                    }
                    MessageBox("Запись удалена");
                    break;
                case MessageBoxResult.No:
                    break;
            }
        }

        // обновить запись
        private void Button_selectClick(object sender, RoutedEventArgs e)
        {
            try
            {                
                UpdateDatagrid();
            }
            catch (Exception ex)
            {
                MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                DataGrid1.ItemsSource = null;
            }
        }

        private readonly SolidColorBrush hb = new(Colors.MistyRose);
        private readonly SolidColorBrush nb = new(Colors.AliceBlue);
        private void DataGrid1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if ((e.Row.GetIndex() + 1) % 2 == 0)
                e.Row.Background = hb;
            else
                e.Row.Background = nb;

            // А можно в WPF установить - RowBackground - для нечетных строк и AlternatingRowBackground
        }

        // вывод диалогового окна
        public static void MessageBox(String infoMessage, MessageBoxImage mImage = System.Windows.MessageBoxImage.Information)
        {
            System.Windows.MessageBox.Show(infoMessage, "Сообщение", System.Windows.MessageBoxButton.OK, mImage);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var row_list = (UserData)DataGrid1.SelectedItem;
                if (row_list != null)
                    DataGrig_Id = row_list.Id;
            }
            catch
            {
                DataGrig_Id = null;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            Button_updateClick(sender, e);
        }

        // применить фильтр
        private void Button_findClick(object sender, RoutedEventArgs e)
        {
            is_filter = true;
            try
            {                
                UpdateDatagrid();
            }
            catch (Exception ex)
            {
                MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                DataGrid1.ItemsSource = null;
            }
        }

        // отменить фильтр
        private void Button_find_cancelClick(object sender, RoutedEventArgs e)
        {
            is_filter = false;
            value1.Text = "";
            value2.Text = "";
            try
            {                
                UpdateDatagrid();
            }
            catch (Exception ex)
            {
                MessageBox(ex.Message, System.Windows.MessageBoxImage.Error);
                DataGrid1.ItemsSource = null;
            }
        }

        // изменение типа данных
        private void Value_type_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            if (is_initialize == true) return;

            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            String? value_type = selectedItem.Content.ToString();

            if (value_type == "id") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "text") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "int") value2.IsEnabled = true;
            else if (value_type == "double") value2.IsEnabled = true;
            else if (value_type == "bool") { value2.IsEnabled = false; value2.Text = ""; }
            else if (value_type == "date") value2.IsEnabled = true;
        }

        // изменение фокуса на value2
        private void Value2_GotKeyboardFocus(object sender, EventArgs e)
        {
            if (value1.Text != "") value2.Text = value1.Text;
        }
    }

}               