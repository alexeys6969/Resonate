using MySql.Data.MySqlClient;
using Resonate_course_project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using BCrypt.Net;
using System.Windows;

namespace Resonate_course_project.Contexts
{
    public class EmployeeContext : Employee
    {
        public EmployeeContext(string full_name, string login, string password, string position) : base(full_name, login, password, position) { }
        public void Create()
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(this.password);
            MySqlConnection conn = DB.mysqlConnection.Open();

            try
            {
                // 1. Создать пользователя в MySQL (пароль в открытом виде!)
                string createUserSql = $"CREATE USER IF NOT EXISTS '{this.login}'@'localhost' IDENTIFIED BY '{this.password}'";
                using (var cmd = new MySqlCommand(createUserSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                string roleName = "";
                if (this.position == "Администратор")
                    roleName = "admin_role";
                else if (this.position == "Менеджер")
                    roleName = "manager_role";
                else if (this.position == "Кассир")
                    roleName = "cashier_role";
                else
                    throw new Exception($"Неизвестная должность: {this.position}");

                // 3. Назначить роль и активировать её по умолчанию
                using (var cmd = new MySqlCommand($"GRANT {roleName} TO '{this.login}'@'localhost'", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new MySqlCommand($"SET DEFAULT ROLE {roleName} TO '{this.login}'@'localhost'", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 4. Сохранить сотрудника в таблицу (с хешированным паролем)
                string insertSql = "INSERT INTO `Employees`(`full_name`, `login`, `password`, `position`) " +
                                   "VALUES (@full_name, @login, @password, @position)";
                using (var cmd = new MySqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("@full_name", this.full_name);
                    cmd.Parameters.AddWithValue("@login", this.login);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@position", this.position);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show($"Сотрудник {this.full_name} успешно создан!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания сотрудника:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            finally
            {
                conn.Close();
            }
        }

        public void Load()
        {
            MySqlConnection conn = DB.mysqlConnection.Open();
            conn.Open();
            DB.mysqlConnection.Query($"SELECT * FROM Employee", conn);
        }

        public void Update()
        {
            MySqlConnection conn = DB.mysqlConnection.Open();
            conn.Open();
            DB.mysqlConnection.Query($"UPDATE `Employees` SET `full_name`='{this.full_name}',`login`='{this.login}',`password`='{BCrypt.Net.BCrypt.HashPassword(this.password)}',`position`='{this.position}' WHERE id = {this.id}", conn);
        }

        public void Delete()
        {
            MySqlConnection conn = DB.mysqlConnection.Open();
            conn.Open();
            DB.mysqlConnection.Query($"DELETE FROM `Employees` WHERE id = {this.id}", conn);
        }
    }
}
