using FilerRepository;
using RepositoryContracts;
using WebAPI; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPostRepository, PostFileRepository>();
builder.Services.AddScoped<IUserRepository, UserFileRepository>();
builder.Services.AddScoped<ICommentRepository, CommentFileRepository>();

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>(); 

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); 

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();