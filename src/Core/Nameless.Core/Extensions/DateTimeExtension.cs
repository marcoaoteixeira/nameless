namespace Nameless {

    /// <summary>
    /// Extension methods for <see cref="DateTime"/>.
    /// </summary>
    public static class DateTimeExtension {

        #region Public Static Methods

        /// <summary>
        /// Gets the difference, in years, between the <paramref name="self"/> <see cref="DateTime"/> 
        /// and <see cref="DateTime.Today"/>.
        /// </summary>
        /// <param name="self">The self <see cref="DateTime"/>.</param>
        /// <returns>An integer representation of the difference.</returns>
        public static int GetYearsToToday(this DateTime self) {
            return self.GetYears(DateTime.Today);
        }

        /// <summary>
        /// Gets the difference, in years, between the <paramref name="self"/>
        /// and the <paramref name="date"/>.
        /// </summary>
        /// <param name="self">The current <see cref="DateTime"/>.</param>
        /// <param name="date">The other <see cref="DateTime"/>.</param>
        /// <returns>An integer representation of the difference.</returns>
        public static int GetYears(this DateTime self, DateTime date) {
            var years = date.Year - self.Year;

            if (date.Month < self.Month) { --years; }
            if (date.Month == self.Month && date.Day < self.Day) { --years; }

            return Math.Abs(years);
        }

        /// <summary>
        /// Retrieves the first working day of the given month and year.
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <param name="ordinal">The ordinal number for the working day, example 5.</param>
        /// <param name="holidays">An list of holidays</param>
        /// <returns>The number of the first working day of the given month and year.</returns>
        public static int GetFirstWorkingDay(int year, int month, int ordinal, params int[] holidays) {
            var day = 1;
            var workingDay = ordinal;
            while (workingDay != 0) {
                var date = new DateTime(year, month, day);
                var weekend = date.DayOfWeek switch {
                    DayOfWeek.Saturday => true,
                    DayOfWeek.Sunday => true,
                    _ => false,
                };
                if (!weekend) {
                    if (!holidays.Contains(day)) {
                        workingDay--;
                        day++;
                    }
                } else { day++; }
            }
            return day - 1;
        }

        /// <summary>
        /// Converts a date into a Unix epoch date.
        /// </summary>
        /// <param name="self">The source <see cref="DateTime"/></param>
        /// <returns>A <see cref="long"/> representing the Unix epoch date.</returns>
        public static long ToUnixEpochDate(this DateTime self) {
            return (long)Math.Round((self.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        }

        #endregion
    }
}