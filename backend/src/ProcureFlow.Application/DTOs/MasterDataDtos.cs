namespace ProcureFlow.Application.DTOs;
public record LookupDto(Guid Id, string Name);
public record DashboardSummaryDto(int TotalPRs,int Draft,int Submitted,int Approved,int Rejected,int POCreated,int Delivered,int Completed,int TotalPOs);