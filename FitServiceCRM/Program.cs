using FitServiceCRM.Data;
using FitServiceCRM.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=fitservicecrm.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "FitServiceCRM.Auth";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AnalyticsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var requiredTables = new[] { "ServiceCenters", "WorkOrderVehicles", "AppointmentVehicles" };
    if (await db.Database.CanConnectAsync() && !await HasExpectedDemoStateAsync(db, requiredTables))
    {
        await db.Database.EnsureDeletedAsync();
    }

    await db.Database.EnsureCreatedAsync();
    await DemoSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "login",
    pattern: "login",
    defaults: new { controller = "Auth", action = "Login" });

app.MapControllerRoute(
    name: "logout",
    pattern: "logout",
    defaults: new { controller = "Auth", action = "Logout" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

static async Task<bool> HasExpectedDemoStateAsync(AppDbContext db, IEnumerable<string> tableNames)
{
    if (await db.Users.AnyAsync())
    {
        if (!await db.WorkOrders.AnyAsync() || !await db.Appointments.AnyAsync() || !await db.DiagnosticConclusions.AnyAsync())
        {
            return false;
        }
    }

    await db.Database.OpenConnectionAsync();
    try
    {
        foreach (var tableName in tableNames)
        {
            await using var command = db.Database.GetDbConnection().CreateCommand();
            command.CommandText = "select count(*) from sqlite_master where type = 'table' and name = $name";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            if (Convert.ToInt32(result) == 0)
            {
                return false;
            }
        }

        return true;
    }
    finally
    {
        await db.Database.CloseConnectionAsync();
    }
}
