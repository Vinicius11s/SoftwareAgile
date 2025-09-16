using Infraestructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class ContextoEmpresaFactory : IDesignTimeDbContextFactory<EmpresaContexto>
{
    public EmpresaContexto CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EmpresaContexto>();

        // Defina a string de conexão de forma que o EF possa usar durante o processo de migração.
        optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;
               DataBase=dbAgile;integrated security=true;TrustServerCertificate=True;");
        return new EmpresaContexto(optionsBuilder.Options);
    }
}
