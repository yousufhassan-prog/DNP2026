namespace Entities;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public String Body { get; set; }
    public int PostId { get; set; }
}