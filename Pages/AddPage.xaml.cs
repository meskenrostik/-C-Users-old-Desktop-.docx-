using Microsoft.Win32;
using stud.Classes;
using stud.DBModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
namespace stud.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddPage.xaml
    /// </summary>
    public partial class AddPage : Page
    {
        public AddPage()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            var groups = ConnectionClass.connect.Groups.ToList();
            CmbxGroup.ItemsSource = groups;
            CmbxGroup.SelectionChanged += (s, e) =>
            {
                var selected = CmbxGroup.SelectedItem as groups;
                if (selected?.Speciality != null)
                    TxtSpec.Text = selected.Speciality.Name_Speciality;
            };
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(dialog.FileName);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("Файл слишком большой (макс. 5 МБ)!");
                    return;
                }

                selectedPhoto = File.ReadAllBytes(dialog.FileName);
                var bitmap = new BitmapImage(new Uri(dialog.FileName));
                IPicture.Source = bitmap;
            }
        }

        private string GenerateLogin(string surname, string name)
        {
            string baseLogin = $"{surname.ToLower()}_{name.ToLower()}";
            string login = baseLogin;
            int counter = 1;

            while (ConnectionClass.connect.Login_Password_Stud.Any(l => l.Login == login))
            {
                login = $"{baseLogin}{counter}";
                counter++;
            }
            return login;
        }

        private string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            string surname = TxtSurname.Text.Trim();
            string name = TxtName.Text.Trim();
            string patronumic = TxtPatronumic.Text.Trim();

            // Проверки
            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Фамилия и имя обязательны!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbxGroup.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpBirthday.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpBirthday.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (DpBirthday.SelectedDate < DateTime.Now.AddYears(-100))
            {
                MessageBox.Show("Некорректная дата рождения!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Генерация логина и пароля
                string login = GenerateLogin(surname, name);
                string password = GeneratePassword();

                // Создание логина/пароля
                var loginPassword = new Login_Password_Stud
                {
                    Login = login,
                    Pasword = password
                };
                ConnectionClass.connect.Login_Password_Stud.Add(loginPassword);
                ConnectionClass.connect.SaveChanges();

                // Создание студента
                var student = new Students
                {
                    Surname = surname,
                    Name = name,
                    Patronumic = patronumic,
                    Date_Birthday = DpBirthday.SelectedDate.Value,
                    ID_Group = ((Groups)CmbxGroup.SelectedItem).ID_Group,
                    ID_Login = loginPassword.ID_Login,
                    Photo = selectedPhoto
                };

                ConnectionClass.connect.Students.Add(student);
                ConnectionClass.connect.SaveChanges();

                MessageBox.Show($"Студент добавлен!\nЛогин: {login}\nПароль: {password}",
                                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}