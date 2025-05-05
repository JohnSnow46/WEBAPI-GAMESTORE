using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;

public class GenreDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The Name field is required.")]
    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
