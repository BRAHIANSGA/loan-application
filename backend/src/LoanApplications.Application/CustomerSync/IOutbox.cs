namespace LoanApplications.Application.CustomerSync;

public interface IOutbox
{
    /// <summary>
    /// Adds the message to the current unit of work. Nothing is stored or sent until
    /// <see cref="IUnitOfWork.SaveChangesAsync"/> commits.
    /// </summary>
    void Enqueue(CustomerSyncOperation operation, CustomerSnapshot customer);
}
