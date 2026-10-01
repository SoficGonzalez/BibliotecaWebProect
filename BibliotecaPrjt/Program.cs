using BibliotecaPrjt.Repositories;
using BibliotecaPrjt.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IRepositorioLibro, RepositorioEn>();
builder.Services.AddSingleton<IAutorService, AutorService>();
builder.Services.AddDbContext<BibliotecaContext>(options => 
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BibliotecaDB"))
);

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<BibliotecaContext>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});
    

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
