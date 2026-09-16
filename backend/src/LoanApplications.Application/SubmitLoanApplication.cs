using LoanApplications.Application.CustomerSync;
using LoanApplications.Domain;
using LoanApplications.Domain.Decisions;

namespace LoanApplications.Application;

public sealed class SubmitLoanApplication(
    LoanDecisionEngine decisionEngine,
    ICustomerRepository customers,
    IOutbox outbox,
    IUnitOfWork unitOfWork)
{
    public async Task<SubmissionResult> HandleAsync(LoanRequest request, CancellationToken cancellationToken)
    {
        var decision = await decisionEngine.DecideAsync(request, cancellationToken);
        if (!decision.IsApproved)
        {
            return new SubmissionResult.Denied(decision.DenialReasons);
        }

        var customer = await customers.FindBySsnAsync(request.Ssn, cancellationToken);
        var isReturningCustomer = customer is not null;

        if (customer is null)
        {
            customer = Customer.Register(request);
            customers.Add(customer);
        }
        else
        {
            customer.UpdateDetails(request);
        }

        outbox.Enqueue(
            isReturningCustomer ? CustomerSyncOperation.Update : CustomerSyncOperation.Create,
            CustomerSnapshot.From(customer));

        // The customer, the loan application and the outbox message are committed in one transaction.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmissionResult.Approved(customer.Id, customer.LoanApplication.Id, isReturningCustomer);
    }
}
