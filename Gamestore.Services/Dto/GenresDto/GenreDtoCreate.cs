using System.ComponentModel.DataAnnotations;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreDtoCreate
{
    [Required(ErrorMessage = "The Name field is required.")]
    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
