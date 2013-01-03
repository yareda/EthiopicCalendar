using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EthiopicCalendar
{
    public class Calendar
    {
        //    
        //	** ********************************************************************************
        //	**  Era Definitions and Private Data
        //	** ********************************************************************************
        //	

        public enum EpochOffset
        {
            AmeteAlem = -285019,
            AmeteMihret = 1723856,
            Coptic = 1824665,
            Gregorian = 1721426,
            Unset = -1
        }

        public const int JD_EPOCH_OFFSET_AMETE_ALEM = -285019; // ዓ/ዓ
        public const int JD_EPOCH_OFFSET_AMETE_MIHRET = 1723856; // ዓ/ም
        public const int JD_EPOCH_OFFSET_COPTIC = 1824665;
        public const int JD_EPOCH_OFFSET_GREGORIAN = 1721426;
        public const int JD_EPOCH_OFFSET_UNSET = -1;

        private int _jdOffset = JD_EPOCH_OFFSET_UNSET;

        private int _year = -1;
        private int _month = -1;
        private int _day = -1;
        private bool _dateIsUnset = true;


        //    
        //	** ********************************************************************************
        //	**  Constructors
        //	** ********************************************************************************
        //	
        public Calendar()
        {
        }
        public Calendar(int year, int month, int day, int era)
        {
            this.Set(year, month, day, era);
        }
        public Calendar(int year, int month, int day)
        {
            this.Set(year, month, day);
        }

        public virtual void Set(int year, int month, int day, int era)
        {
            this._year = year;
            this._month = month;
            this._day = day;
            this.SetEra(era);
            this._dateIsUnset = false;
        }
        public virtual void Set(int year, int month, int day)
        {
            this._year = year;
            this._month = month;
            this._day = day;
            this._dateIsUnset = false;
        }

        public virtual int GetDay()
        {
            return _day;
        }
        public virtual int GetMonth()
        {
            return _month;
        }
        public virtual int GetYear()
        {
            return _year;
        }
        public virtual int GetEra()
        {
            return _jdOffset;
        }
        public virtual int[] GetDate()
        {
            int[] date = { _year, _month, _day, _jdOffset };
            return date;
        }

        public virtual void SetEra(int era)
        {
            if ((JD_EPOCH_OFFSET_AMETE_ALEM == era) || (JD_EPOCH_OFFSET_AMETE_MIHRET == era))
            {
                _jdOffset = era;
            }
            else
            {
                throw (new ArithmeticException("Unknown era: " + era + " must be either ዓ/ዓ or ዓ/ም."));
            }
        }

        public virtual bool IsEraSet()
        {
            return (JD_EPOCH_OFFSET_UNSET == _jdOffset) ? false : true;
        }

        public virtual void UnSetEra()
        {
            _jdOffset = JD_EPOCH_OFFSET_UNSET;
        }

        public virtual void UnSet()
        {
            UnSetEra();
            _year = -1;
            _month = -1;
            _day = -1;
            _dateIsUnset = true;
        }

        public virtual bool IsDateSet()
        {
            return (_dateIsUnset) ? false : true;
        }


        //    
        //	** ********************************************************************************
        //	**  Conversion Methods To/From the Ethiopic & Gregorian Calendars
        //	** ********************************************************************************

        /// <summary>
        /// Converts Ethiopic date to Gregorian date. The Ethiopic date format should be dd/mm/yyyy
        /// </summary>
        /// <param name="etDate">Ethiopic date in dd/mm/yyyy format</param>
        /// <returns>Gregorian date in a DateTime instance</returns>
        public DateTime EthiopicToGregorian(string etDate)
        {
            var date = etDate.Split('/');
            int day = Convert.ToInt32(date[0]);
            int month = Convert.ToInt32(date[1]);
            int year = Convert.ToInt32(date[2]);

            var temp = EthiopicToGregorian(year, month, day);

            int y = Convert.ToInt32(temp[0]);
            int m = Convert.ToInt32(temp[1]);
            int d = Convert.ToInt32(temp[2]);
            return new DateTime(y,m,d);
        }

        public virtual int[] EthiopicToGregorian(int era)
        {
            if (!IsDateSet())
            {
                throw (new ArithmeticException("Unset date."));
            }
            return EthiopicToGregorian(this._year, this._month, this._day, era);
        }

        public virtual int[] EthiopicToGregorian(int year, int month, int day, int era)
        {
            SetEra(era);
            int[] date = EthiopicToGregorian(year, month, day);
            UnSetEra();
            return date;
        }
        
        public virtual int[] EthiopicToGregorian()
        {
            if (_dateIsUnset)
            {
                throw (new ArithmeticException("Unset date."));
            }
            return EthiopicToGregorian(this._year, this._month, this._day);
        }
        public virtual int[] EthiopicToGregorian(int year, int month, int day)
        {
            if (!IsEraSet())
            {
                if (year <= 0)
                {
                    SetEra(JD_EPOCH_OFFSET_AMETE_ALEM);
                }
                else
                {
                    SetEra(JD_EPOCH_OFFSET_AMETE_MIHRET);
                }
            }

            int jdn = EthiopicToJdn(year, month, day);
            return JdnToGregorian(jdn);
        }

        public virtual string GregorianToEthiopic(DateTime gcDate)
        {
            var date = GregorianToEthiopic(gcDate.Year, gcDate.Month, gcDate.Day);
            var result = string.Format("{0}/{1}/{2}", date[2], date[1], date[0]);
            return result;
        }

        public virtual int[] GregorianToEthiopic()
        {
            if (_dateIsUnset)
            {
                throw (new ArithmeticException("Unset date."));
            }
            return GregorianToEthiopic(this._year, this._month, this._day);
        }
        public virtual int[] GregorianToEthiopic(int year, int month, int day)
        {
            int jdn = GregorianToJdn(year, month, day);

            return JdnToEthiopic(jdn, GuessEraFromJDN(jdn));
        }

        //    
        //	** ********************************************************************************
        //	**  Conversion Methods To/From the Julian Day Number
        //	** ********************************************************************************
        //	
        private static int nMonths = 12;

        private static int[] monthDays = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        private int Quotient(long i, long j)
        {
            return (int)Math.Floor((double)i / j);
        }

        private int Mod(long i, long j)
        {
            return (int)(i - (j * Quotient(i, j)));
        }

        private int GuessEraFromJDN(int jdn)
        {
            return (jdn >= (JD_EPOCH_OFFSET_AMETE_MIHRET + 365)) ? JD_EPOCH_OFFSET_AMETE_MIHRET : JD_EPOCH_OFFSET_AMETE_ALEM;
        }

        private bool IsGregorianLeap(int year)
        {
            return (year % 4 == 0) && ((year % 100 != 0) || (year % 400 == 0));
        }

        public virtual int[] JdnToGregorian(int j)
        {
            int r2000 = Mod((j - JD_EPOCH_OFFSET_GREGORIAN), 730485);
            int r400 = Mod((j - JD_EPOCH_OFFSET_GREGORIAN), 146097);
            int r100 = Mod(r400, 36524);
            int r4 = Mod(r100, 1461);

            int n = Mod(r4, 365) + 365 * Quotient(r4, 1460);
            int s = Quotient(r4, 1095);


            int aprime = 400 * Quotient((j - JD_EPOCH_OFFSET_GREGORIAN), 146097) + 100 * Quotient(r400, 36524) + 4 * Quotient(r100, 1461) + Quotient(r4, 365) - Quotient(r4, 1460) - Quotient(r2000, 730484);
            ;
            int year = aprime + 1;
            int t = Quotient((364 + s - n), 306);
            int month = t * (Quotient(n, 31) + 1) + (1 - t) * (Quotient((5 * (n - s) + 13), 153) + 1);
            //        
            //		int day    = t * ( n - s - 31*month + 32 )
            //		           + ( 1 - t ) * ( n - s - 30*month - quotient((3*month - 2), 5) + 33 )
            //		;
            //		

            // int n2000 = quotient( r2000, 730484 );
            n += 1 - Quotient(r2000, 730484);
            int day = n;


            if ((r100 == 0) && (n == 0) && (r400 != 0))
            {
                month = 12;
                day = 31;
            }
            else
            {
                monthDays[2] = (IsGregorianLeap(year)) ? 29 : 28;
                for (int i = 1; i <= nMonths; ++i)
                {
                    if (n <= monthDays[i])
                    {
                        day = n;
                        break;
                    }
                    n -= monthDays[i];
                }
            }

            int[] @out = { year, month, day };

            return @out;
        }

        public virtual int GregorianToJdn(int year, int month, int day)
        {
            int s = Quotient(year, 4) - Quotient(year - 1, 4) - Quotient(year, 100) + Quotient(year - 1, 100) + Quotient(year, 400) - Quotient(year - 1, 400);
            ;

            int t = Quotient(14 - month, 12);

            int n = 31 * t * (month - 1) + (1 - t) * (59 + s + 30 * (month - 3) + Quotient((3 * month - 7), 5)) + day - 1;

            int j = JD_EPOCH_OFFSET_GREGORIAN + 365 * (year - 1) + Quotient(year - 1, 4) - Quotient(year - 1, 100) + Quotient(year - 1, 400) + n;

            return j;
        }

        public virtual int[] JdnToEthiopic(int jdn)
        {
            return (IsEraSet()) ? JdnToEthiopic(jdn, _jdOffset) : JdnToEthiopic(jdn, GuessEraFromJDN(jdn))
            ;
        }
        public virtual int[] JdnToEthiopic(int jdn, int era)
        {
            long r = Mod((jdn - era), 1461);
            long n = Mod(r, 365) + 365 * Quotient(r, 1460);

            int year = 4 * Quotient((jdn - era), 1461) + Quotient(r, 365) - Quotient(r, 1460);
            ;
            int month = Quotient(n, 30) + 1;
            int day = Mod(n, 30) + 1;

            return new int[] { year, month, day };
        }

        public virtual int EthiopicToJdn()
        {
            if (_dateIsUnset)
            {
                throw (new ArithmeticException("Unset date."));
            }
            return EthiopicToJdn(this._year, this._month, this._day);
        }

        ///    
        ///	 <summary>
        ///  Computes the Julian day number of the given Coptic or Ethiopic date.
        ///	 This method assumes that the JDN epoch offset has been set. This method
        ///	 is called by copticToGregorian and ethiopicToGregorian which will set
        ///	 the jdn offset context.
        ///	 </summary>
        ///	 <param name="year"> a year in the Ethiopic calendar </param>
        ///	 <param name="month"> a month in the Ethiopic calendar </param>
        ///	 <param name="date"> a date in the Ethiopic calendar
        ///	 </param>
        ///	 <returns> The Julian Day Number (JDN) </returns>
        ///	 
        private int EthCopticToJdn(int year, int month, int day, int era)
        {
            int jdn = (era + 365) + 365 * (year - 1) + Quotient(year, 4) + 30 * month + day - 31;

            return jdn;
        }
        public virtual int EthiopicToJdn(int year, int month, int day)
        {
            return (IsEraSet()) ? EthCopticToJdn(year, month, day, _jdOffset) : EthCopticToJdn(year, month, day, JD_EPOCH_OFFSET_AMETE_MIHRET)
            ;
        }
        //JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int ethiopicToJDN(int era) throws java.lang.ArithmeticException
        public virtual int EthiopicToJdn(int era)
        {
            return EthiopicToJdn(_year, _month, _day, era);
        }
        //JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int ethiopicToJDN(int year, int month, int day, int era) throws java.lang.ArithmeticException
        public virtual int EthiopicToJdn(int year, int month, int day, int era)
        {
            return EthCopticToJdn(year, month, day, era);
        }


        //    
        //	** ********************************************************************************
        //	**  Methods for the Coptic Calendar
        //	** ********************************************************************************
        //	
        //JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int[] copticToGregorian() throws java.lang.ArithmeticException
        public virtual int[] CopticToGregorian()
        {
            if (_dateIsUnset)
            {
                throw (new ArithmeticException("Unset date."));
            }
            return CopticToGregorian(this._year, this._month, this._day);
        }
        public virtual int[] CopticToGregorian(int year, int month, int day)
        {
            SetEra(JD_EPOCH_OFFSET_COPTIC);
            int jdn = EthiopicToJdn(year, month, day);
            return JdnToGregorian(jdn);
        }

        //JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int[] gregorianToCoptic() throws java.lang.ArithmeticException
        public virtual int[] GregorianToCoptic()
        {
            if (_dateIsUnset)
            {
                throw (new ArithmeticException("Unset date."));
            }
            return GregorianToCoptic(this._year, this._month, this._day);
        }
        public virtual int[] GregorianToCoptic(int year, int month, int day)
        {
            SetEra(JD_EPOCH_OFFSET_COPTIC);
            int jdn = GregorianToJdn(year, month, day);
            return JdnToEthiopic(jdn);
        }

        public virtual int CopticToJdn(int year, int month, int day)
        {
            return EthCopticToJdn(year, month, day, JD_EPOCH_OFFSET_COPTIC);
        }
    }

    public class EthiopicDateTime
    {
        private int _incomingDay;
        private int _incomingMonth;
        private int _incomingYear;
        private int _incomingEra;

        private int _day;
        private int _month;
        private int _year;
        private int _era;
        private string _dayOfWeek;

        public int Day { get { return _day; } }
        public int Month { get { return _month; } }
        public int Year { get { return _year; } }

        private Calendar ec = new Calendar();
        private string[] monthNames = { "መስከረም", "ጥቅምት", "ኅዳር", "ታህሣሥ", "ጥር", "የካቲት", "መጋቢት", "ሚያዝያ", "ግንቦት", "ሰኔ", "ሐምሌ", "ነሐሴ", "ጳጉሜ" };
        private string[] dayNames = { "እሑድ", "ሰኞ", "ማክሰኞ", "ረቡዕ", "ሓሙስ", "ዓርብ", "ቅዳሜ" };
        private string[] eraNames = { "ዓ/ም", "ዓ/ዓ" };

        public EthiopicDateTime(DateTime gcDateTime)
        {
            _incomingDay = gcDateTime.Day;
            _incomingMonth = gcDateTime.Month;
            _incomingYear = gcDateTime.Year;
            _incomingEra = EthiopicCalendar.Calendar.JD_EPOCH_OFFSET_AMETE_MIHRET;
            _dayOfWeek = GetETDayOfWeek(gcDateTime);

            DoConvertion();
        }

        public EthiopicDateTime(int day, int month, int year)
        {
            _incomingDay = day;
            _incomingMonth = month;
            _incomingYear = year;
            DoConvertion();
        }
        /// <summary>
        /// String in Ethiopic date format.
        /// </summary>
        /// <param name="dateString">Ethiopic date in dd/mm/yyyy format</param>
        public EthiopicDateTime(string dateString)
        {
            try
            {
                string[] items = dateString.Split('/');
                if (items.Length == 3)
                {
                    _incomingDay = System.Convert.ToInt32(items[0]);
                    _incomingMonth = System.Convert.ToInt32(items[1]);
                    _incomingYear = System.Convert.ToInt32(items[2]);
                    DoConvertion();
                }
            }
            catch(Exception e)
            {
               throw new ApplicationException("Invalid date supplied.",e);
            }

        }
        private string GetETDayOfWeek(DateTime gcDateTime)
        {
            string dow = string.Empty;

            switch (gcDateTime.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    dow = dayNames[0];
                    break;
                case DayOfWeek.Monday:
                    dow = dayNames[1];
                    break;
                case DayOfWeek.Tuesday:
                    dow = dayNames[2];
                    break;
                case DayOfWeek.Wednesday:
                    dow = dayNames[3];
                    break;
                case DayOfWeek.Thursday:
                    dow = dayNames[4];
                    break;
                case DayOfWeek.Friday:
                    dow = dayNames[5];
                    break;
                case DayOfWeek.Saturday:
                    dow = dayNames[6];
                    break;
            }
            return dow;
        }

        private string GetETMonthName(int month)
        {
            return monthNames[month - 1];
        }

        private void DoConvertion()
        {
            var result = ec.GregorianToEthiopic(_incomingYear, _incomingMonth, _incomingDay);
            _year = result[0];
            _month = result[1];
            _day = result[2];
        }

        public DateTime ToGcDate()
        {
            var result = ec.EthiopicToGregorian(_incomingYear, _incomingMonth, _incomingDay);
            var year = result[0];
            var month = result[1];
            var day = result[2];
            return new DateTime(year, month, day);
        }

        static public EthiopicDateTime Now
        {
            get
            {
                return new EthiopicDateTime(DateTime.Now);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", _dayOfWeek, GetETMonthName(_month), _day, _year, eraNames[0]);
        }

        public string ToShortDate()
        {
            return string.Format("{0} {1} {2}", _day, _month, _year);
        }
        public string ToMonthAndYearName()
        {
            return string.Format("{0} {1}", GetETMonthName(_month), _year);
        }
    }

}
