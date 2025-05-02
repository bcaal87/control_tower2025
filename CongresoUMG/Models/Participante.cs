using System.ComponentModel.DataAnnotations;

namespace CongresoUMG.Models
{
    public class Participante
    {
        public int Id { get; set; }

        [Required]
        public string Carne { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        [Required]
        public string Dpi { get; set; }

        [Phone]
        public string Telefono { get; set; }

        [Required]
        [EmailAddress]
        public string CorreoElectronico { get; set; }

        [Required]
        public string CicloOSemestre { get; set; }

        public bool CheckIn { get; set; } = false;
    }
}


