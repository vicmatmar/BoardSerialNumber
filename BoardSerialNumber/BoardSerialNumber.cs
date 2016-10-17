using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.SqlClient;
using System.Globalization;

using System.Net.NetworkInformation;
using Microsoft.Win32;

using CentraliteDataUtils;

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
            Number = 0;
            Week_Year = new Week_Year();
        }

        public int Product_ID { get; set; }
        public int Number { get; set; }
        public Week_Year Week_Year { get; set; }
    }

    static string _con_str = CentraliteDataUtils.DataUtils.DBConnStr;
    static public string DBConnectionStr
    {
        get { return _con_str; }
        set { _con_str = value; }
    }
    static public CentraliteDataContext DataContext
    {
        get { return new CentraliteDataContext(DBConnectionStr); }
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
        using (CentraliteDataContext cx = DataContext)
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

    public static string BuildSerial(int product_id, int number, int week, int year)
    {
        string week_number = string.Format("{0:D2}{1}", week, year);

        string serial_number = string.Format("{0:D3}{1}", product_id, week_number);
        serial_number += string.Format("{0:D6}", number);

        return serial_number;
    }

    public static string BuildSerial(int product_id)
    {
        Week_Year wy = GetWeekYearNumber();
        int number = GetNextNumber(product_id: product_id, wy: wy);
        string serial_number = BuildSerial(product_id, number, wy.Week, wy.Year);

        return serial_number;
    }

    public static string BuildSerial(int product_id, int number, Week_Year wy)
    {
        string serial_number = BuildSerial(product_id, number, wy.Week, wy.Year);

        return serial_number;
    }

    public static int GetNextNumber(int product_id, Week_Year wy)
    {
        int number = 0;
        using (CentraliteDataContext dc = DataContext)
        {
            int[] numbers = dc.BoardTrackers
                .Where(
                c => c.ProductId == product_id &&
                c.Week == wy.Week &&
                c.Year == wy.Year
                )
                .OrderByDescending(c => c.Number)
                .Select(c => c.Number).Take(1).ToArray<int>();
            if (numbers.Length > 0)
            {
                number = numbers[0] + 1;
            }
        }

        return number;
    }

    public static int GetNextNumber(int product_id)
    {
        Week_Year wy = GetWeekYearNumber();
        int number = GetNextNumber(product_id: product_id, wy: wy);
        return number;
    }

    public static Serial_Parts Parse(string serialnumber)
    {
        if(serialnumber.Length != 13)
        {
            throw new ArgumentException("Incorrect serial number length: " + serialnumber.Length.ToString());
        }
        Serial_Parts parts = new Serial_Parts();

        parts.Product_ID = Convert.ToInt32(serialnumber.Substring(0, 3));

        parts.Week_Year.Week = Convert.ToInt32(serialnumber.Substring(3, 2));

        parts.Week_Year.Year = Convert.ToInt32(serialnumber.Substring(5, 2));

        parts.Number = Convert.ToInt32(serialnumber.Substring(8));

        return parts;
    }
}
