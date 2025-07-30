using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rotativa.AspNetCore;
using ServiceDashBoard1.Data;
using ServiceDashBoard1.Service;
using ServiceDashBoard1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using static ServiceDashBoard1.Service.TokenGenerator;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;




var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddDbContext<ServiceDashBoard1Context>(options =>
//  options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceDashBoard1Context") ?? throw new InvalidOperationException("Connection string 'ServiceDashBoard1Context' not found.")));


//  1. Add Localization Services
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

//  2. Supported Cultures
var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("hi"),
    new CultureInfo("fr"),
    new CultureInfo("ur"),
};


builder.Services.AddDbContext<ServiceDashBoard1Context>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ServiceDashBoard1Context"),
        new MySqlServerVersion(new Version(8, 0, 0))
    )
);

builder.Services.AddDbContext<ServiceReportContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ServiceReportContext"),
        new MySqlServerVersion(new Version(8, 0, 0))
    )
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information)
);


// Add services to the container.
builder.Services.AddControllersWithViews().AddViewLocalization()
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // Enable culture switching via ?culture=hi or cookie
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
    options.RequestCultureProviders.Insert(1, new CookieRequestCultureProvider());
});

#if DEBUG

builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

#endif




builder.Services.AddScoped<ComplaintService>();
builder.Services.AddScoped<PdfService>(); 
builder.Services.AddSingleton<SapService>();

builder.Services.AddScoped<TokenGenerator>(); // ✅ Register service
builder.Services.AddScoped<EmployeeIdGenerator>();


// Add session service
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout if needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

var locOptions = app.Services.GetService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Inject the IWebHostEnvironment parameter to access the environment
    app.UseDeveloperExceptionPage();


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
