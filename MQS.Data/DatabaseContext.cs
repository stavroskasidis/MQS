using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Common;
using System.Data;
using System.Configuration;

namespace MQS.Data
{
    public class DatabaseContext : DbContext
    {
        public static void SetInitializer(IDatabaseInitializer<DatabaseContext> Initializer)
        {
            Database.SetInitializer<DatabaseContext>(Initializer);
        }

        //private static DbConnection innerconnection;

        public DatabaseContext()
            : base()
        {

        }

        public DatabaseContext(string providerInvariantName, string connectionString)
            : base(CreateConnection(providerInvariantName, connectionString), true)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
        }


        private static DbConnection CreateConnection(string providerInvariantName, string connectionString)
        {
            //if (innerconnection != null)
            //{
            //    return innerconnection;
            //}
            try
            {
                var dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
                dataSet.Tables[0].Rows.Add("SQLite Data Provider"
                , ".Net Framework Data Provider for SQLite"
                , "System.Data.SQLite"
                , "System.Data.SQLite.SQLiteFactory, System.Data.SQLite");
            }
            catch (Exception) { }

            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerInvariantName);
            if (providerFactory == null)
                throw new InvalidOperationException(String.Format("The '{0}' provider is not registered on the local machine.", providerInvariantName));

            DbConnection connection = providerFactory.CreateConnection();
            connection.ConnectionString = connectionString;
            //innerconnection = connection;
            return connection;
        }

        public DbSet<PendingMessage> PendingMessages { get; set; }
        public DbSet<Client> Clients { get; set; }
    }
    
}
