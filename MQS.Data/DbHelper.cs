using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQS.Data;
using System.Reflection;
using System.IO;
using System.Data.SQLite;

namespace MQS.Data
{
    public static class DbHelper
    {
        private static bool initialized;

        public static DatabaseContext GetSession()
        {
            if (!initialized)
            {
                DatabaseContext.SetInitializer(new IndexInitializer<DatabaseContext>());
                initialized = true;
                if (!File.Exists("pending_messages.sqlite"))
                {
                    SQLiteConnection.CreateFile("pending_messages.sqlite");
                    SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=pending_messages.sqlite;Version=3;");
                    m_dbConnection.Open();

                    //string sql = "CREATE TABLE IF NOT EXISTS [PendingMessages]([ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,[Key] NVARCHAR(2048) IPEndPoint,[Value] NVARCHAR(2048) SerializedMessage);";
                    string sql = "CREATE TABLE PendingMessages (ID TEXT PRIMARY KEY,Client_ID INT,SerializedMessage TEXT, CreatedAt DATETIME);";
                    SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                    command.CommandText = sql;
                    command.ExecuteNonQuery();

                    sql = "CREATE TABLE Clients (ID TEXT PRIMARY KEY,IPEndPoint TEXT, CreatedAt DATETIME);";
                    command.CommandText = sql;
                    command.ExecuteNonQuery();

                    m_dbConnection.Close();
                }
            }
            return new DatabaseContext("System.Data.SQLite", "Data Source=pending_messages.sqlite");//new DatabaseContext();
        }

    }
}
