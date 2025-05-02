using System.ComponentModel.DataAnnotations;

public class Usuario
{
    public int Id { get; set; }

    [Required]
    public string UsuarioNombre { get; set; }

    [Required]
    public string Clave { get; set; }

    public string Rol { get; set; } = "Admin";
}
