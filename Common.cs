using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace GatewayUpload
{
    public class Common
    {
        public static MySqlConnection mySqlConnection { get; set; }
    }
}
