namespace TMG.ManagerClasses
{
    public static class DateTimeManager
    {

        public static string Today => DateToHtmlFormat(DateTime.Now);

        public static DateTime ParseHtmlDate(string date)
        {
            string[] splitDate = date.Split('-');
            DateTime dt = new DateTime(
                    Convert.ToInt32(splitDate[0]),
                    Convert.ToInt32(splitDate[1]),
                    Convert.ToInt32(splitDate[2])
                );
            return dt;
        }

        public static string DateToHtmlFormat(DateTime dt)
        {
            string month = dt.Month < 10 ? "0" + dt.Month.ToString() : dt.Month.ToString();
            string day = dt.Day < 10 ? "0" + dt.Day.ToString() : dt.Day.ToString();
            return $"{dt.Year}-{month}-{day}";
        }

        public static string DateToHtmlFormat(DateTime? dt)
        {
            if (dt == null) return "";

            string month = dt?.Month < 10 ? "0" + dt?.Month.ToString() : dt?.Month.ToString();
            string day = dt?.Day < 10 ? "0" + dt?.Day.ToString() : dt?.Day.ToString();
            return $"{dt?.Year}-{month}-{day}";
        }

        public static string DateTimeToHtmlFormat(DateTime dt)
        {
            string month = dt.Month < 10 ? "0" + dt.Month.ToString() : dt.Month.ToString();
            string day = dt.Day < 10 ? "0" + dt.Day.ToString() : dt.Day.ToString();

            string hour = dt.Hour < 10 ? "0" + dt.Hour.ToString() : dt.Hour.ToString();
            string min = dt.Minute < 10 ? "0" + dt.Minute.ToString() : dt.Minute.ToString();

            return $"{dt.Year}-{month}-{day}T{hour}:{min}";
        }

        public static string DateTimeToHtmlFormat(DateTime? dt)
        {
            if (dt == null) return "";

            string month = dt?.Month < 10 ? "0" + dt?.Month.ToString() : dt?.Month.ToString();
            string day = dt?.Day < 10 ? "0" + dt?.Day.ToString() : dt?.Day.ToString();

            string hour = dt?.Hour < 10 ? "0" + dt?.Hour.ToString() : dt?.Hour.ToString();
            string min = dt?.Minute < 10 ? "0" + dt?.Minute.ToString() : dt?.Minute.ToString();

            return $"{dt?.Year}-{month}-{day}T{hour}:{min}";
        }

        public static string GetDate(DateTime dt)
        {
            return $"{dt.Day}/{dt.Month}/{dt.Year}";
        }
        public static string GetDate(DateTime? dt)
        {
            if (dt == null) return "";
            return $"{dt?.Day}/{dt?.Month}/{dt?.Year}";
        }

        public static string DateTimeToLocalString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-ddTHH:mm");
        }

        public static string DateTimeToLocalString(DateTime? dt)
        {
            if (dt.HasValue)
            {
                return dt.Value.ToString("yyyy-MM-ddTHH:mm");
            }
            return "";

        }

        public static string TimeToHtmlFormat(DateTime dt)
        {
            string hour = dt.Hour < 10 ? "0" + dt.Hour.ToString() : dt.Hour.ToString();
            string minute = dt.Minute < 10 ? "0" + dt.Minute.ToString() : dt.Minute.ToString();
            return $"{hour}:{minute}";
        }

        public static string TimeToHtmlFormat(DateTime? dt)
        {
            if (dt == null) return "";

            string hour = dt?.Hour < 10 ? "0" + dt?.Hour.ToString() : dt?.Hour.ToString();
            string minute = dt?.Minute < 10 ? "0" + dt?.Minute.ToString() : dt?.Minute.ToString();
            return $"{hour}:{minute}";
        }

        public static string TimeToLocalString(DateTime dt)
        {
            return dt.ToString("HH:mm");
        }

        public static string TimeToLocalString(DateTime? dt)
        {
            return dt.HasValue ? dt.Value.ToString("HH:mm") : "";
        }

        public static int CalculateAge(DateTime dt)
        {
            var today = DateTime.Today;
            var age = today.Year - dt.Year;

            if (dt.Date > today.AddYears(-age)) age--;

            return age;
        }

        public static List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> allDates = new List<DateTime>();

            allDates.Add(startDate);

            DateTime currentDate = startDate;
            while (currentDate < endDate.AddDays(-1))
            {
                currentDate = currentDate.AddDays(1);
                allDates.Add(currentDate);
            }

            return allDates;
        }

        public static bool IsDateInRange(DateTime? dateToCheck, DateTime? startDate, DateTime? endDate)
        {
            return dateToCheck >= startDate && dateToCheck <= endDate;
        }

    }
}
