using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Gamestore.Services.Dto.GenresDto;
public class GenreGetAllDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The Name field is required.")]
    public string? Name { get; set; }

    public Guid? ParentGenreId { get; set; }
}
