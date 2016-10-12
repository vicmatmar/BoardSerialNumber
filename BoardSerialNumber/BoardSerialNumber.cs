using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Globalization;

using System.Net.NetworkInformation;
using Microsoft.Win32;

using BoardSerialData;

public class BoardSerialNumber
{
    public class Week_Year
    {
        public Week_Year()
        {
            Week = 0;
            Year = 0;
        }
        public Week_Year(int week, int year)
        {
            Week = week;
            Year = year;
        }

        public int Week { get; set; }
        public int Year { get; set; }
    }

    public class Serial_Parts
    {

        public Serial_Parts()
        {
            Product_ID = 0;
            Serial_Number = 0;
            Week_Year = new Week_Year();
        }

        public int Product_ID { get; set; }
        public int Serial_Number { get; set; }
        public Week_Year Week_Year { get; set; }
    }

    /// <summary>
    /// Gets the week number based on database current date
    /// </summary>
    /// <returns>4 digit week + year string</returns>
    public static Week_Year GetWeekYearNumber()
    {
        int year = -1;
        int week = -1;
        //using (SqlConnection con = new SqlConnection(ConnectionSB.ConnectionString))
        using (BoardSerial_DataContext cx = new BoardSerial_DataContext())
        {

            DateTime date = cx.ExecuteQuery<DateTime>("SELECT GETDATE()").First();

            week = DateTimeFormatInfo.CurrentInfo.Calendar.GetWeekOfYear(
                date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            year = DateTimeFormatInfo.CurrentInfo.Calendar.GetYear(date);
        }

        if (year < 0 || week < 0)
        {
            throw new Exception("Unable to determine week or year info");
        }

        int year_2digit = Convert.ToInt32(year.ToString().Substring(2));
        return new Week_Year(week, year_2digit);
    }

    public static string BuildSerial(int product_id, int serial_num, int week, int year)
    {
        string week_number = string.Format("{0:D2}{1}", week, year);

        string serial_number = string.Format("{0:D3}{1}", product_id, week_number);
        serial_number += string.Format("{0:D6}", serial_num);

        return serial_number;
    }

    public static Serial_Parts Parse(string serialnumber)
    {
        if(serialnumber.Length != 14)
        {
            throw new ArgumentException("Incorrect serial number length: " + serialnumber.Length.ToString());
        }
        Serial_Parts parts = new Serial_Parts();

        parts.Product_ID = Convert.ToInt32(serialnumber.Substring(0, 3));

        parts.Week_Year.Week = Convert.ToInt32(serialnumber.Substring(3, 2));

        parts.Week_Year.Year = Convert.ToInt32(serialnumber.Substring(5, 2));

        parts.Serial_Number = Convert.ToInt32(serialnumber.Substring(8));

        return parts;
    }
}
