var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "home",
        pattern: "Home/{action=Index}/{id?}",
        defaults: new { controller = "Home" }
    );
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=TranscriptionUI}/{action=Index}/{id?}"
    );
});

app.Run();
