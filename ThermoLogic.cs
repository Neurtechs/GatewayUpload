using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GatewayUpload.Data;
using MySql.Data.MySqlClient;
using static GatewayUpload.Common;

namespace GatewayUpload
{
    class ThermoLogic
    {
        public static void CheckLogic(string nodeSer, DateTime timeStamp, double value, int seq)
        {
            //Add the data to the temp table
            MySqlCommand cmd = new MySqlCommand("insertThermoTemp", mySqlConnection);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("_nodeSer", nodeSer); ;
            cmd.Parameters.AddWithValue("_value", value);
            cmd.Parameters.AddWithValue("_timeStamp", timeStamp);
            cmd.Parameters.AddWithValue("_seq", seq);
            if (mySqlConnection.State == ConnectionState.Closed) { mySqlConnection.Open(); }
            cmd.ExecuteNonQuery();

            //Read back the temporary data
             cmd = new MySqlCommand("GetThermoTemp", mySqlConnection); //reads in reverse order
             cmd.CommandType = CommandType.StoredProcedure;
             cmd.Parameters.AddWithValue("_nodeSer", nodeSer); ;
             cmd.ExecuteNonQuery();
             MySqlDataAdapter da = new MySqlDataAdapter();
             da.SelectCommand = cmd;
             DataTable dt = new DataTable();
             da.Fill(dt);
             string sstr = seq.ToString();
            //if (sstr == "142336")
            //{
            //    MessageBox.Show(sstr + ", " + value.ToString());
            //}
            if (dt.Rows.Count < 2)
            {
                 GetData.ThermoChangeStatus(nodeSer, timeStamp, value, seq);
                 goto ExitHere;
            }
            double diffInSec =
                 (Convert.ToDateTime(dt.Rows[0]["TimeStamp"]) - Convert.ToDateTime(dt.Rows[1]["TimeStamp"]))
                 .TotalSeconds;


            //Check for off
            if (Convert.ToDouble(dt.Rows[0]["value"]) == 0)
            {
                //off
                GetData.ThermoChangeStatus(nodeSer, timeStamp, 0, seq);
            }
            else //>0
            {
                //compare to previous value
                if (Convert.ToDouble(dt.Rows[1]["value"]) > 0)
                {
                    //previous reading was also on
                    if (Convert.ToDouble(dt.Rows[0]["value"]) > Convert.ToDouble(dt.Rows[1]["value"]))
                    {
                        //the new value is higher, so replace
                        int lastSeq = Convert.ToInt32(dt.Rows[1]["seq"]);
                        GetData.UpdateThermoValue(seq, value, nodeSer, timeStamp);
                    }
                    else
                    {
                        //Do nothing
                    }
                }
                else  //previous value was 0
                {
                    //Switch on
                    GetData.ThermoChangeStatus(nodeSer, timeStamp, value, seq);
                }
            }



            ////Check for off
            //if (Convert.ToDouble(dt.Rows[0]["value"]) ==0)
            //{
            //    //Definately off
            //    GetData.ThermoChangeStatus(nodeSer, timeStamp, 0, seq);
            //}
            //else if (Convert.ToDouble(dt.Rows[0]["value"]) < 0.5 * Convert.ToDouble(dt.Rows[1]["value"]))
            //{
            //    //Powering down
            //    if (diffInSec < 10)
            //    {
            //        //Definately on update reading
            //        int lastSeq = Convert.ToInt32(dt.Rows[1]["seq"]);
            //        GetData.UpdateThermoValue(seq, value, nodeSer, timeStamp);
            //    }
            //    else
            //    {
            //        if (Convert.ToDouble(dt.Rows[1]["value"]) > 0.1)
            //        {
            //            //This cannot be off
            //            //Ignore
            //        }
            //        else
            //        {
            //            //Possibly on
            //            GetData.ThermoChangeStatus(nodeSer, timeStamp, value, seq);
            //        }

            //    }
            //}
            //else if (Convert.ToDouble(dt.Rows[0]["value"]) > 1.1 * Convert.ToDouble(dt.Rows[1]["value"]))
            //{
            //    //Powering up or on
            //    if (diffInSec < 10)
            //    {
            //        //Definately on update reading
            //        int lastSeq = Convert.ToInt32(dt.Rows[1]["seq"]);
            //        GetData.UpdateThermoValue(lastSeq,value,nodeSer,timeStamp);
            //    }
            //    else
            //    {
            //        //Possibly on or powering down
            //        GetData.ThermoChangeStatus(nodeSer,timeStamp,value,seq);
                    
            //    }
            //}
            //else //not provided for
            //{
            //    //Ignore

            //    //GetData.ThermoChangeStatus(nodeSer, timeStamp, value, seq);
            //}
            ExitHere: ;
        }
    }
}
