using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Service;
using ServiceDashBoard1.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ServiceDashBoard1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceDashBoard1Context") ?? throw new InvalidOperationException("Connection string 'ServiceDashBoard1Context' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();





#if DEBUG

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

#endif


builder.Services.AddScoped<ComplaintService>();


builder.Services.AddScoped<TokenGenerator>(); // ✅ Register service

// Add session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(300);  // Set session timeout if needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Users}/{action=Login}/{id?}");

app.Run();
