using System;
using System.Collections.Generic;

namespace MyApi.Models
{
    public class KpzCdrsZaDan
    {
        public int Id { get; set; }
        public int BrojZatvora { get; set; }
        public DateTime Datum { get; set; }
        public int BrojPoziva { get; set; } = 0;
        public int BrojJavljanja { get; set; } = 0;
        public int BrojSekundiRazgovora { get; set; } = 0;
        public int BrojSekundiFiksna { get; set; } = 0;
        public int BrojSekundiMobilna { get; set; } = 0;
        public int BrojSekundiInternacionalna { get; set; } = 0;
    }
}
