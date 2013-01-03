Ethiopic date conversion in C# based on JDN
============

A class library for converting Ethiopian dates to Gregorian and vice versa. The algorithm is based on Julian Day Numbers (JDN). The C# version
is a port of the Java one found at ethiopic.org (http://ethiopic.org/calendars/EthiopicCalendar.java).

#### Usage

            private EthiopicCalendar.Calendar ec = new Calendar();
            var selected = this.dateTimePicker1.Value;            
            
            // Using the ec instance of the Calendar class
            textBox.Text = ec.GregorianToEthiopic(selected.Date);
            
            // Directly using method chaining
            label.Text = new EthiopicDateTime(selected.Date).ToString();
            
            
#### More Resources

* Julian Day Numbers (http://www.hermetic.ch/cal_stud/jdn.htm)
* The Ethiopic Calendar (http://ethiopic.org/Calendars)
* A compilation of resources for Ethiopic date conversion (http://addisagile.wordpress.com/2010/05/07/ethiopic-calendar-resources)



