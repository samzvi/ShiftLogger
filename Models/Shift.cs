using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.Models
{
    public class Shift
    {
        public int Id { get; set; }

        public User? Driver { get; set; }

        public Car? Car { get; set; }
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Nesmí být prázdné")]
        public int TaxiPort { get; set; }

        [Required(ErrorMessage = "Nesmí být prázdné")]
        public int Liftago { get; set; }

        [Required(ErrorMessage = "Nesmí být prázdné")]
        public int Bolt { get; set; }

        [Required(ErrorMessage = "Nesmí být prázdné")]
        public int Other { get; set; }

        [Required(ErrorMessage = "Jméno je vyžadováno.")]
        public int Distance { get; set; }
    }
}
