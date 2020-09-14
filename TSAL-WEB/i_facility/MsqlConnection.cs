using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Configuration;

namespace i_facility
{
    class MsqlConnection : IDisposable
    {
        #region Local Server
        ////static String ServerName = @"TCP:10.20.10.65,1433";  // Server
        //static String ServerName = @"SRKSDEV001-PC\SQLSERVER17";

        ////static String ServerName = @"PAVANKUMARV013\SQL2017EXPDELL"; //local
        //static String username = "sa";
        ////static String password = "srks4$tsal";//server
        //static String password = "srks4$";//server
        ////static String password = "srks4$";//local
        ////static String port = "3306";
        //static String DB = "i_facility_tsal";


        public static String ServerName = @"" + ConfigurationManager.AppSettings["ServerName"]; //SIEMENS\SQLEXPRESS
        public static String username = ConfigurationManager.AppSettings["username"]; //sa
                                                                                      //static String password = "srks4$";//server
        public static String password = ConfigurationManager.AppSettings["password"];
        public static String port = "3306";
        public static String DB = ConfigurationManager.AppSettings["DB"];// i_facility_tsal //Common
        public static String Schema = ConfigurationManager.AppSettings["Schema"];  //Schema Name
        public static String DbName = ConfigurationManager.AppSettings["databasename"];
        #endregion

        //public SqlConnection msqlConnection = new SqlConnection("server = " + ServerName + ";userid = " + username + ";Password = " + password + ";database = " + DB + ";port = " + port + ";persist security info=False");

        public SqlConnection msqlConnection = new SqlConnection(@"Data Source = " + ServerName + ";User ID = " + username + ";Password = " + password + ";Initial Catalog = " + DB + ";Persist Security Info=True");

        public void open()
        {
            if (msqlConnection.State != System.Data.ConnectionState.Open)
                msqlConnection.Open();
        }

        public void close()
        {
            msqlConnection.Close();
        }

        void IDisposable.Dispose()
        { }
    }
}
