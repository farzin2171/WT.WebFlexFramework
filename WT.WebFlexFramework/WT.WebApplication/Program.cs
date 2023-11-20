using Microsoft.AspNetCore.Authorization;
using WT.WebApplication.Data;
using WT.WebApplication.Infrastructure.Authentication;
using WT.WebApplication.Infrastructure.Authorization;
using Microsoft.EntityFrameworkCore;
using WT.WebApplication.Infrastructure.Extentions;
using WT.WebApplication.Data.Account;
using Microsoft.AspNetCore.Identity;
using WT.WebApplication.Settings;
using WT.WebApplication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<IAuthToken, AuthToken>();

builder.Services.Configure<SmtpSetting>(builder.Configuration.GetSection("SMTP"));
builder.Services.AddSingleton<IEmailService, EmailService>();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "WT.WebApplication";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddAuthentication().AddCookie("temp")
    .AddGoogle("Google",o =>
   {
       o.ClientId = "635672167628-qc09tqq0rhe4hlf431ivqp5g30ab1e4r.apps.googleusercontent.com";
       o.ClientSecret = "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";
       o.SignInScheme = "temp";

   });

//builder.Services.AddAuthentication("cookies")
//    .AddCookie("cookies", o =>
//    {
//        o.Cookie.Name = "WT.WebApplication";
//        o.ExpireTimeSpan = TimeSpan.FromHours(8);

//        o.LoginPath = "/account/login";
//        o.AccessDeniedPath = "/account/login";
//    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("Admin"));
    options.AddPolicy("MustBelongToHRDepartment", policy => policy.RequireClaim("Department", "HR"));
    options.AddPolicy("HRManagerOnly", policy => policy
        .RequireClaim("Department", "HR")
        .RequireClaim("Manager")
        .Requirements.Add(new HRManagerProbationRequirement(3)));
});

builder.Services.AddSingleton<IAuthorizationHandler, HRManagerProbationRequirementHandler>();

builder.Services.AddHttpClient("MainWebAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7130/");
});

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.RunDatabaseMigrations<ApplicationDbContext>();
app.Run();
