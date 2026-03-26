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
    /// Логика взаимодействия для EditPage.xaml
    /// </summary>
    public partial class EditPage : Page
    {
        private int studentId;
        private Students currentStudent;
        private byte[] currentPhoto;

        public EditPage(int id)
        {
            InitializeComponent();
            studentId = id;
            LoadData();
            LoadGroups();
        }

        private void LoadData()
        {
            currentStudent = ConnectionClass.connect.Students
                .FirstOrDefault(s => s.ID_Stud == studentId);

            if (currentStudent == null) return;

            TxtSurname.Text = currentStudent.Surname;
            TxtName.Text = currentStudent.Name;
            TxtPatronumic.Text = currentStudent.Patronumic;
            DpBirthday.SelectedDate = currentStudent.Date_Birthday;
            TxtLogin.Text = currentStudent.Login_Password_Stud?.Login;

            if (currentStudent.Photo != null && currentStudent.Photo.Length > 0)
            {
                currentPhoto = currentStudent.Photo;
                var bitmap = new BitmapImage();
                using (var ms = new MemoryStream(currentStudent.Photo))
                {
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    IPicture.Source = bitmap;
                }
            }
        }

        private void LoadGroups()
        {
            var groups = ConnectionClass.connect.Groups.ToList();
            CmbxGroup.ItemsSource = groups;
            if (currentStudent != null)
            {
                CmbxGroup.SelectedItem = groups.FirstOrDefault(g => g.ID_Group == currentStudent.ID_Group);
                if (currentStudent.Group?.Speciality != null)
                    TxtSpec.Text = currentStudent.Group.Speciality.Name_Speciality;
            }
            CmbxGroup.SelectionChanged += (s, e) =>
            {
                var selected = CmbxGroup.SelectedItem as Groups;
                if (selected?.Speciality != null)
                    TxtSpec.Text = selected.Speciality.Name_Speciality;
            };
        }

        private void BtnChangeImage_Click(object sender, RoutedEventArgs e)
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

                currentPhoto = File.ReadAllBytes(dialog.FileName);
                var bitmap = new BitmapImage(new Uri(dialog.FileName));
                IPicture.Source = bitmap;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string surname = TxtSurname.Text.Trim();
            string name = TxtName.Text.Trim();
            string patronumic = TxtPatronumic.Text.Trim();
            string login = TxtLogin.Text.Trim();
            string password = TxtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Фамилия и имя обязательны!");
                return;
            }

            if (CmbxGroup.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу!");
                return;
            }

            if (DpBirthday.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату рождения!");
                return;
            }
            if (DpBirthday.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем!");
                return;
            }

            // Проверка уникальности логина
            var existingLogin = ConnectionClass.connect.Login_Password_Stud
                .FirstOrDefault(l => l.Login == login && l.ID_Login != currentStudent.ID_Login);
            if (existingLogin != null)
            {
                MessageBox.Show("Такой логин уже существует!");
                return;
            }

            try
            {
                currentStudent.Surname = surname;
                currentStudent.Name = name;
                currentStudent.Patronumic = patronumic;
                currentStudent.Date_Birthday = DpBirthday.SelectedDate.Value;
                currentStudent.ID_Group = ((Groups)CmbxGroup.SelectedItem).ID_Group;

                if (currentPhoto != null)
                    currentStudent.Photo = currentPhoto;

                // Обновление логина/пароля
                if (currentStudent.Login_Password_Stud != null)
                {
                    currentStudent.Login_Password_Stud.Login = login;
                    if (!string.IsNullOrWhiteSpace(password))
                        currentStudent.Login_Password_Stud.Pasword = password;
                }

                ConnectionClass.connect.SaveChanges();
                MessageBox.Show("Данные сохранены!", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
