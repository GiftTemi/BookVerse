using BookVerse.Application.Data;
using BookVerse.Application.Repository.IRepository;
using BookVerse.Domain;
using BookVerse.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BookVerse.Application.Repository;

public class ApplicationSeed : IApplicationSeed
{

    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<ApplicationSeed> _logger;
    private readonly IUserStore<IdentityUser> _userStore;

    public ApplicationDbContext _context { get; }

    public ApplicationSeed(ApplicationDbContext context, ILogger<ApplicationSeed> logger, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore)    {
        _context = context;
        _logger = logger;
        _roleManager = roleManager;
        _userManager = userManager;
        _userStore = userStore;
    }

    public void SeedData()
    {
        try
        {
            _logger.LogInformation("About to start data seeding");

            // Check if the data has already been seeded
            if (_context.Roles.Any() )
            {
                Console.WriteLine("Data has already been seeded. Skipping data seeding process.");
                return;
            }
            // Perform your data seeding logic here
            SeedRoles();
            SeedCategories();
            _context.SaveChangesAsync(new CancellationToken());

            _ = SeedAdminUser();

            _logger.LogInformation("Data seeding completed successfully.");
        }
        catch (Exception ex)
        {
            // Log and handle the exception
            _logger.LogError($"Error seeding data: {ex.Message}");
        }
    }

    private  void SeedRoles()
    {
        if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
        {
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();
        }
    }
    private async Task SeedAdminUser()
    {
        // Create the admin user

        var user = new ApplicationUser
        {
            Email = "admin@yopmail.com", // Replace with the desired email
            Name = "Admin Admin",
            PhoneNumber = "08131098765",
            StreetAddress = "Address def",
            City = "City",
            State = "State",
            PostalCode = "PostalCode",
        };

        await _userStore.SetUserNameAsync(user, user.Email, CancellationToken.None);

        if (user.Email != null)
        {
            user.EmailConfirmed = true;
            user.Email = user.Email;
            user.NormalizedEmail = user.Email;
        }

        //await _userStore.SetNormalizedUserNameAsync(user, user.Email, CancellationToken.None);

        var result = await _userManager.CreateAsync(user, "Password1$");

        if (result.Succeeded)
        {
            _logger.LogInformation("User created a new account with password.");


            // Assign the admin role to the admin user
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == SD.Role_Admin);

            if (adminRole != null)
            {
                await _userManager.AddToRoleAsync(user, SD.Role_Admin);
            }
        }
    }
    private void SeedCategories()
    {
        var categories = new List<Category>
        {
            new Category { Name = "Law", DisplayOrder = 1 },
                new Category { Name = "Marriage", DisplayOrder = 2 },
                new Category { Name = "Children", DisplayOrder = 3 },
                new Category { Name = "Education", DisplayOrder = 4 },
                new Category { Name = "Business", DisplayOrder = 5 }
            };

        // Save the items to the database
        _context.Categories.AddRange(categories);
    }
}
