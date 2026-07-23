using System;
using System.Collections.Generic;

namespace BarangayCMS.Web.Areas.Admin.Models
{
    public class ReportsDashboardViewModel
    {
        // --- SUMMARY COUNTERS ---
        public int TotalResidents { get; set; }
        public int TotalComplaints { get; set; }
        public decimal TotalBudget { get; set; }
        public int ActiveProjects { get; set; }
        public int TotalCertificatesIssued { get; set; }
        public int ActiveEvacuees { get; set; }
        public int TotalHealthRecords { get; set; }

        // --- MONTHLY STATS ---
        public int CertificatesIssuedThisMonth { get; set; }
        public int ComplaintsThisMonth { get; set; }
        public int ComplaintsLastMonth { get; set; }
        public string ComplaintTrendStatus { get; set; } = string.Empty;
        public int NewResidentsThisMonth { get; set; }
        public int DeceasedOrMovedThisMonth { get; set; }
        public string ResidentTrendStatus { get; set; } = string.Empty;

        // 📊 CHART DATA PROPERTIES
        public List<string> Months { get; set; } = new List<string>();
        public List<int> MonthlyComplaints { get; set; } = new List<int>();
        public List<int> MonthlyCertificates { get; set; } = new List<int>();
        public List<int> MonthlyResidents { get; set; } = new List<int>();
    }

    // --- SUB-REPORT ITEM MODELS (MAY ALIASES PARA SA LAHAT NG CS1061 ERRORS) ---

    public class ResidentReportItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Name => FullName; // 👈 Fix para sa @item.Name
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string CivilStatus { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Purok => Address; // 👈 Fix para sa @item.Purok
        public string ContactNumber { get; set; } = string.Empty;
        public string VoterStatus { get; set; } = string.Empty;
        public bool IsResident { get; set; }
        public string Status => IsResident ? "Active" : "Inactive"; // 👈 Fix para sa @item.Status
        public DateTime CreatedAt { get; set; }
    }

    public class ComplaintReportItem
    {
        public int Id { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string CaseId => CaseNumber; // 👈 Fix para sa @item.CaseId
        public string ComplainantName { get; set; } = string.Empty;
        public string Complainant => ComplainantName; // 👈 Fix para sa @item.Complainant
        public string RespondentName { get; set; } = string.Empty;
        public string Respondent => RespondentName; // 👈 Fix para sa @item.Respondent
        public string CaseType { get; set; } = "General"; // 👈 Fix para sa @item.CaseType
        public string IncidentLocation { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public DateTime DateFiled => IncidentDate; // 👈 Fix para sa @item.DateFiled
        public string Status { get; set; } = string.Empty;
        public DateTime DateSubmitted { get; set; }
    }

    public class CertificateReportItem
    {
        public int Id { get; set; }
        public string ControlNumber { get; set; } = string.Empty;
        public string ReferenceNo => ControlNumber; // 👈 Fix para sa @item.ReferenceNo
        public string ResidentName { get; set; } = string.Empty;
        public string RequestorName => ResidentName; // 👈 Fix para sa @item.RequestorName
        public string CertificateType { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; } = 0m; // 👈 Fix para sa @item.AmountPaid
        public DateTime DateRequested { get; set; }
        public DateTime? DateIssued { get; set; }
    }

    public class BudgetReportItem
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal TotalAllocation { get; set; }
        public decimal DisbursedAmount { get; set; }
        public decimal RemainingBalance => TotalAllocation - DisbursedAmount;
        public string LoggedBy { get; set; } = string.Empty;
    }

    public class ProjectReportItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal BudgetAllocated { get; set; }
        public decimal TotalExpenses { get; set; }
        public string Contractor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProjectBudgetReportItem
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal AllocatedBudget { get; set; }
        public decimal ExpensesToDate { get; set; }
        public decimal RemainingBalance => AllocatedBudget - ExpensesToDate;
        public string FundingSource { get; set; } = string.Empty;
        public string Contractor { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class DisasterReportItem
    {
        public int Id { get; set; }
        public string IncidentName { get; set; } = string.Empty;
        public string DisasterType { get; set; } = string.Empty;
        public DateTime OccurrenceDate { get; set; }
        public int AffectedHouseholdsCount { get; set; }
        public int DisplacedIndividualsCount { get; set; }
        public int FamiliesAccommodated => DisplacedIndividualsCount; // 👈 Fix para sa @item.FamiliesAccommodated
        public int CasualtiesCount { get; set; }
        public string EvacuationCenter { get; set; } = "Barangay Gym"; // 👈 Fix para sa @item.EvacuationCenter
        public string EvacuationCenterStatus { get; set; } = string.Empty;
        public string ReliefDistributionStatus { get; set; } = string.Empty;
        public string Status => EvacuationCenterStatus; // 👈 Fix para sa @item.Status
    }

    public class HealthReportItem
    {
        public int Id { get; set; }
        public string ResidentName { get; set; } = string.Empty;
        public double WeightKg { get; set; }
        public double HeightCm { get; set; }
        public string BloodType { get; set; } = string.Empty;
        public bool IsVaccinated { get; set; }
        public string MedicalCondition { get; set; } = string.Empty;
        public string Condition => MedicalCondition; // 👈 Fix para sa @item.Condition
        public string HealthClassification { get; set; } = string.Empty;
        public string AssistanceReceived { get; set; } = "Free Checkup & Medicine"; // 👈 Fix para sa @item.AssistanceReceived
        public DateTime LastCheckupDate { get; set; }
        public DateTime DateChecked => LastCheckupDate; // 👈 Fix para sa @item.DateChecked
        public string AttendingHealthWorker { get; set; } = string.Empty;
    }
}