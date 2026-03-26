using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using stud.Classes;
using stud.DBModel;

namespace stud.Pages
{
    /// <summary>
    /// Логика взаимодействия для StudentsPage.xaml
    /// </summary>
    public partial class StudentsPage : Page
    {
        public StudentsPage()
        {
            InitializeComponent();
            Refresh();
        }
        private void Refresh()
        {
            try
            {
                var students = ConnectionClass.connect.Students.ToList();
                DgStudents.ItemsSource = students;
                TblCount.Text = $"Всего студентов: {students.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPage());
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgStudents.SelectedItem as Students;
            if (selected == null)
            {
                MessageBox.Show("Выберите студента для редактирования!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            NavigationService.Navigate(new EditPage(selected.ID_Stud));
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = DgStudents.SelectedItem as Students;
            if (selected == null)
            {
                MessageBox.Show("Выберите студента для удаления!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить студента {selected.Surname} {selected.Name}?",
                                         "Подтверждение",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var login = ConnectionClass.connect.Login_Password_Stud
                        .FirstOrDefault(l => l.ID_Login == selected.ID_Login);
                    if (login != null)
                        ConnectionClass.connect.Login_Password_Stud.Remove(login);

                    ConnectionClass.connect.Students.Remove(selected);
                    ConnectionClass.c.SaveChanges();

                    MessageBox.Show("Студент удален!", "Успех",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
    
