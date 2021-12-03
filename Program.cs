using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zradelna
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create new instance of client
            WebApiClient wa = new WebApiClient();

            //Open homepage first to get sessin ID and cookie
            wa.OpenHomepage();

            //Login as "FFFFFFFF"
            wa.Login(CardToId("FFFFFFFF"));

            //Get current account balance
            wa.GetAccountBalance();
        }

        /// <summary>
        /// HEX (Little-endian) string to UInt32 as string 
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        static string CardToId(string hexString)
        {
            StringBuilder sb = new StringBuilder();

            uint num = uint.Parse(hexString, System.Globalization.NumberStyles.AllowHexSpecifier);

            byte[] floatVals = BitConverter.GetBytes(num);
            byte[] newArray = new byte[4];
            int position = 3;

            foreach (byte b in floatVals)
            {
                newArray[position] = b;
                position--;
            }

            return BitConverter.ToUInt32(newArray, 0).ToString();
        }
    }

    
}
