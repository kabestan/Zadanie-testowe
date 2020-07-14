using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CommonCode
{
    public class DatabaseOperator
    {
        private const string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        private SqlConnection connection;

        public DatabaseOperator()
        {
            connection = new SqlConnection(connectionString);
        }

        public bool Open()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }

        }

        public void Close()
        {
            connection.Close();
        }
    }
}
