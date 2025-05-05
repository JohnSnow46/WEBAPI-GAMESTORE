namespace Gamestore.Entities;
public class GameFile
{
    public Guid Id { get; set; }

    public byte[] Content { get; set; } = Array.Empty<byte>();

    public Guid GameId { get; set; }

    public Game Game { get; set; } = null!;
}
