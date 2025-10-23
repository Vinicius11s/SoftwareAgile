using Infraestructure.Repository;
using Interfaces.Repository;
using Interfaces.Service;
using Services;
using Infraestructure.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });
builder.Services.AddScoped<Interfaces.Service.ILearningService, Services.DatabaseLearningService>();
builder.Services.AddScoped<Services.CsvServices>();
builder.Services.AddTransient<Services.PdfServices>();

// Configurar sessões
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IFundoPersonalizadoRepository, FundoPersonalizadoRepository>();
builder.Services.AddScoped<IFundoPersonalizadoService, FundoPersonalizadoService>();
builder.Services.AddScoped<IImagemProdutoRepository, ImagemProdutoRepository>();
builder.Services.AddScoped<IImagemProdutoService, ImagemProdutoService>();
builder.Services.AddHttpClient<ImageSearchService>();
builder.Services.AddScoped<ImageSearchService>();
builder.Services.AddScoped<PostWebGeneratorService>();
builder.Services.AddDbContext<EmpresaContexto>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

// Adicionar middleware para capturar erros de serialização
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro capturado pelo middleware: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Agile}/{action=Index}/{id?}");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
