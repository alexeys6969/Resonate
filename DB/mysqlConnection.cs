using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Resonate_course_project.DB
{
    public static class mysqlConnection
    {
        public static string _connection = "server=127.0.0.1;port=3307;uid=readonly_user;pwd=1111;database=Resonate";
        public static MySqlConnection Open()
        {
            MySqlConnection connection = new MySqlConnection(_connection);
            connection.Open();

            return connection;
        }

        public static MySqlDataReader Query(string Sql, MySqlConnection connection)
        {
            MySqlCommand Command = new MySqlCommand(Sql, connection);
            MySqlDataReader Query = Command.ExecuteReader();

            return Query;
        }

        public static void Close(MySqlConnection connection)
        {
            connection.Close();
            MySqlConnection.ClearPool(connection);
        }
    }
}
