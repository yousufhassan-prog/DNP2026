using CLI.UI;
//using FileRepositories;
using FilerRepository;
using RepositoryContracts;


Console.WriteLine("Starting CLI app...");
IUserRepository userRepository = new UserFileRepository(); // old: UserInMemoryRepository();
ICommentRepository commentRepository = new CommentFileRepository(); // old: CommentInMemoryRepository();
IPostRepository postRepository = new PostFileRepository(); // old: PostInMemoryRepository();

CliApp cliApp = new CliApp(userRepository, commentRepository, postRepository);
await cliApp.StartAsync();