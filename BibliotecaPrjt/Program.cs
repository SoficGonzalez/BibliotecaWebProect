using BibliotecaPrjt.Repositories;
using BibliotecaPrjt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddSingleton<IRepositorioLibro, RepositorioEn>();
builder.Services.AddSingleton<IAutorService, AutorService>();
builder.Services.AddDbContext<BibliotecaContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BibliotecaDB"))
);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
}).AddEntityFrameworkStores<BibliotecaContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied"; 
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    foreach (var rol in new[] { "Administrador", "Usuario" })
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    
    var admin = await userManager.FindByNameAsync("Sofia");
    if (admin != null && !await userManager.IsInRoleAsync(admin, "Administrador"))
    {
        await userManager.AddToRoleAsync(admin, "Administrador");
    }


    var usuarios = await userManager.Users.ToListAsync();
    foreach(var usuario in usuarios)
    {
        if((await userManager.GetRolesAsync(usuario)).Count ==0)
        {
            var resultado = await userManager.AddToRoleAsync(usuario, "Usuario");
            if(!resultado.Succeeded)
            {
                throw new InvalidOperationException(
                    $"No se pudo asignar el rol Usuario a {usuario.UserName}:{string.Join(",", resultado.Errors.Select(e => e.Description))}");
            }
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();