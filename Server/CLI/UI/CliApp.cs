using Entities;
using InMemoryRepositories;
using RepositoryContracts;

namespace CLI.UI;

public class CliApp
{
    
    private readonly IUserRepository userRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public CliApp(IUserRepository userRepository, ICommentRepository commentRepository, IPostRepository postRepository)
    {
        this.userRepository = userRepository;
        this.commentRepository = commentRepository;
        this.postRepository = postRepository;
    }
    
    public async Task StartAsync()
    {
        bool exit = false;
        while (!exit)
        {
            Console.WriteLine("\n--- Forum Application ---");
            Console.WriteLine("1. Create new user");
            Console.WriteLine("2. Create new post");
            Console.WriteLine("3. Add comment to existing post");
            Console.WriteLine("4. View posts overview");
            Console.WriteLine("5. View specific post");
            Console.WriteLine("0. Exit");
            Console.Write("Please choose an option: ");

            string? input = Console.ReadLine(); // Reads the user's choice

            switch (input)
            {
                case "1":
                    await CreateUserAsync(); //  must await async methods
                    break;
                case "2": await CreatePostAsync();
                    break;
                case "3": await AddCommentAsync(); 
                    break;
                case "4":
                    ViewPostsOverview(); // GetMany() is not async, so no await needed
                    break;
                case "5":
                    await ViewSinglePostAsync();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("Exiting...");
                    break;
                default:
                    Console.WriteLine("Invalid input, please try again.");
                    break;
            }
        }
    }
    
    private async Task CreateUserAsync()
    {
        Console.WriteLine("\n--- Create New User ---");
    
        Console.Write("Enter username: ");
        string? username = Console.ReadLine();

        Console.Write("Enter password: ");
        string? password = Console.ReadLine();

        
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Username and password cannot be empty.");
            return;
        }
        
        
        bool usernameTaken = userRepository.GetMany().Any(u => u.Username == username);
        if (usernameTaken)
        {
            Console.WriteLine($"Error: The username '{username}' is already taken. Please choose another.");
            return;
        }
        
        User newUser = new User
        {
            Username = username,
            Password = password
        };
        
        User createdUser = await userRepository.AddAsync(newUser);
    
        Console.WriteLine($"Success! User '{createdUser.Username}' created with ID: {createdUser.Id}");
    }
    
    
    private async Task CreatePostAsync()
    {
        Console.WriteLine("\n--- Create New Post ---");
        Console.Write("Enter Post Title: ");
        string? title = Console.ReadLine();
        Console.Write("Enter Post Body: ");
        string? body = Console.ReadLine();

        Console.Write("Enter your User ID: ");
        string? userIdInput = Console.ReadLine();
        
        if (!int.TryParse(userIdInput, out int userId))
        {
            Console.WriteLine("Error: User ID must be a valid number.");
            return;
        }

        bool userExists = userRepository.GetMany().Any(u => u.Id == userId);
        if (!userExists)
        {
            Console.WriteLine($"Error: No user found with ID {userId}. Cannot create post.");
            return;
        }

        Post newPost = new Post { Title = title, Body = body, UserId = userId };
        Post createdPost = await postRepository.AddAsync(newPost);
        Console.WriteLine($"Success! Post created with ID: {createdPost.Id}");
    }
    
    
    private async Task AddCommentAsync()
    {
        Console.WriteLine("\n--- Add Comment ---");
        Console.Write("Enter Comment Body: ");
        string? body = Console.ReadLine();

        Console.Write("Enter your User ID: ");
        if (!int.TryParse(Console.ReadLine(), out int userId))
        {
            Console.WriteLine("Error: User ID must be a valid number.");
            return;
        }

        Console.Write("Enter the Post ID you are commenting on: ");
        if (!int.TryParse(Console.ReadLine(), out int postId))
        {
            Console.WriteLine("Error: Post ID must be a valid number.");
            return;
        }

        bool userExists = userRepository.GetMany().Any(u => u.Id == userId);
        bool postExists = postRepository.GetMany().Any(p => p.Id == postId);

        if (!userExists)
        {
            Console.WriteLine($"Error: No user found with ID {userId}.");
            return;
        }
        if (!postExists)
        {
            Console.WriteLine($"Error: No post found with ID {postId}.");
            return;
        }

        Comment newComment = new Comment { Body = body, UserId = userId, PostId = postId };
        Comment createdComment = await commentRepository.AddAsync(newComment);
        Console.WriteLine($"Success! Comment created with ID: {createdComment.Id}");
    }
    
    
    private async Task ViewSinglePostAsync()
    {
        Console.WriteLine("\n--- View Specific Post ---");
        Console.Write("Enter Post ID: ");
    
        if (!int.TryParse(Console.ReadLine(), out int postId))
        {
            Console.WriteLine("Error: Post ID must be a valid number.");
            return;
        }

        try
        {
            Post post = await postRepository.GetSingleAsync(postId);
        
            Console.WriteLine($"\nTitle: {post.Title}");
            Console.WriteLine($"Body: {post.Body}");
            Console.WriteLine("--- Comments ---");

            
            var comments = commentRepository.GetMany().Where(c => c.PostId == postId).ToList();
        
            if (!comments.Any())
            {
                Console.WriteLine("No comments yet.");
            }
            else
            {
                foreach (var comment in comments)
                {
                    Console.WriteLine($"- {comment.Body} (Written by User ID: {comment.UserId})");
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"\n{ex.Message}");
        }
    }
    
    private void ViewPostsOverview()
    {
        Console.WriteLine("\n--- Posts Overview ---");

        IQueryable<Post> posts = postRepository.GetMany();

        if (!posts.Any())
        {
            Console.WriteLine("No posts available.");
            return;
        }

        foreach (Post post in posts)
        {
            Console.WriteLine($"[ID: {post.Id}] {post.Title}");
        }
    }
}