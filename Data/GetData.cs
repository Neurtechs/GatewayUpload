using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using static GatewayUpload.Common;


namespace GatewayUpload.Data
{
    public static class GetData
    {
        public static void GetSerial(out DataTable dtGetSerial)
        {
            MySqlCommand cmd = new MySqlCommand("GetNodeSerial", mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            MySqlDataAdapter da = new MySqlDataAdapter();
            da.SelectCommand = cmd;
            dtGetSerial = new DataTable();
            da.Fill(dtGetSerial);
        }

        public static void InsertReading(string serialNo, int readingsType, double reading, DateTime timeStamp, int seq)
        {
            MySqlCommand cmd = new MySqlCommand("InsertReadingN", mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_serialNo", serialNo);
            cmd.Parameters.AddWithValue("_readingsType", readingsType);
            cmd.Parameters.AddWithValue("_reading", reading);
            cmd.Parameters.AddWithValue("_timeStamp", timeStamp);
            cmd.Parameters.AddWithValue("_UTC", 1);
            cmd.Parameters.AddWithValue("_seq", seq);
            if (mySqlConnection.State == ConnectionState.Closed) { mySqlConnection.Open(); }
            cmd.ExecuteNonQuery();
            mySqlConnection.Close();

        }

        public static int lastSeq()
        {
            MySqlCommand cmd = new MySqlCommand("GetLastSeq",mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("_seqmax", MySqlDbType.Int32);
            cmd.Parameters["_seqmax"].Direction = ParameterDirection.Output;
            if (mySqlConnection.State == ConnectionState.Closed) { mySqlConnection.Open(); }
            cmd.ExecuteNonQuery();
            mySqlConnection.Close();
            return Convert.ToInt32(cmd.Parameters["_seqmax"].Value);
        }

        public static void ThermoChangeStatus(string serialNo, DateTime timeStamp, double value, int seq)
        {
            MySqlCommand cmd = new MySqlCommand("ThermoChangeStatusN", mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_serialNo", serialNo); ;
            cmd.Parameters.AddWithValue("_value", value);
            cmd.Parameters.AddWithValue("_timeStamp", timeStamp);
            cmd.Parameters.AddWithValue("_seq", seq);
            if (mySqlConnection.State == ConnectionState.Closed) { mySqlConnection.Open(); }
            cmd.ExecuteNonQuery();
            mySqlConnection.Close();
        }

        public static void SwitchStatusChange(string serialNo, DateTime timeStamp, int status, int seq)
        {
            MySqlCommand cmd = new MySqlCommand("SwitchStatusChangeN", mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_serialNo", serialNo); ;
            cmd.Parameters.AddWithValue("_status", status);
            cmd.Parameters.AddWithValue("_timeStamp", timeStamp);
            cmd.Parameters.AddWithValue("_seq", seq);
            if (mySqlConnection.State == ConnectionState.Closed) { mySqlConnection.Open(); }
            cmd.ExecuteNonQuery();
            mySqlConnection.Close();
        }
    }
}
