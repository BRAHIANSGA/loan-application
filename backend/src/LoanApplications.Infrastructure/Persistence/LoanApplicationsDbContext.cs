using LoanApplications.Application;
using LoanApplications.Domain;
using LoanApplications.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace LoanApplications.Infrastructure.Persistence;

public sealed class LoanApplicationsDbContext(DbContextOptions<LoanApplicationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(customer =>
        {
            customer.Property(c => c.Id).ValueGeneratedNever();
            customer.Property(c => c.FirstName).HasMaxLength(Customer.MaxNameLength);
            customer.Property(c => c.LastName).HasMaxLength(Customer.MaxNameLength);
            customer.Property(c => c.CompanyName).HasMaxLength(Customer.MaxCompanyNameLength);
            customer.Property(c => c.Ssn).HasConversion(ssn => ssn.Value, value => Ssn.Parse(value)).HasMaxLength(9);
            customer.HasIndex(c => c.Ssn).IsUnique();
            customer.ComplexProperty(c => c.Address, address =>
            {
                address.Property(a => a.Street).HasMaxLength(Address.MaxStreetLength);
                address.Property(a => a.City).HasMaxLength(Address.MaxCityLength);
                address.Property(a => a.State).HasMaxLength(2);
                address.Property(a => a.ZipCode).HasMaxLength(10);
            });
            customer.HasOne(c => c.LoanApplication).WithOne().HasForeignKey<LoanApplication>(a => a.CustomerId);
        });

        modelBuilder.Entity<LoanApplication>(application =>
        {
            application.Property(a => a.Id).ValueGeneratedNever();
            application.Property(a => a.RequestedAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OutboxMessage>(message =>
        {
            message.Property(m => m.Operation).HasConversion<string>().HasMaxLength(20);
            message.Property(m => m.Payload).HasColumnType("jsonb");
        });
    }
}
