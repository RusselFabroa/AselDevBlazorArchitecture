using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazorArchitecture.Domain.Entities
{
    public class TemperatureData
    {
        public int id { get; set; }
        public string deviceID { get; set; } = "ESP32-001";
        public double temperature { get; set; } = 0;
        public double humidity { get; set; } = 0;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;


        public string GetTemperatureStatus()
        {
            if (temperature < 15)
                return "Cold";
            else if (temperature >= 15 && temperature < 25)
                return "Comfortable";
            else
                return "Hot";
        }

        public string GetHumidityStatus()
        {
            if (humidity < 30)
                return "Dry";
            else if (humidity >= 30 && humidity < 60)
                return "Comfortable";
            else
                return "Humid";
        }

    }
}
