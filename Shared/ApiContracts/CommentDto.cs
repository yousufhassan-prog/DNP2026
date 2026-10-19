namespace ApiContracts;

public class CommentDto
{
    public int Id { get; set; }
    public required string Body { get; set; }
    public required int PostId { get; set; }
    public required int UserId { get; set; }
}