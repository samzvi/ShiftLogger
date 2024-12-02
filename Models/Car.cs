using System.ComponentModel.DataAnnotations;

namespace ShiftLogger.Models
{
    public class Car
    {
        [Required(ErrorMessage = "Vyber auto")]
        public int Id { get; set; }

        [Required(ErrorMessage = "SPZ je vyžadovaná.")]
        public string SPZ { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jméno je vyžadováno.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Označení je vžadováno.")]
        public string Marker { get; set; } = string.Empty;
    }
}
