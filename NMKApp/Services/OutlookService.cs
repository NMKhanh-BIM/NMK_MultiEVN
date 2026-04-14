using System.Runtime.InteropServices;

namespace NMKApp.Services;

public class OutlookService
{
    private dynamic? _outlookApp;
    private string? _currentUserEmail;

    public string? CurrentUserEmail => _currentUserEmail;

    public bool Initialize()
    {
        try
        {
            var outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null) return false;
            
            _outlookApp = Activator.CreateInstance(outlookType);
            if (_outlookApp == null) return false;

            dynamic ns = _outlookApp.GetNamespace("MAPI");
            ns.Logon(Type.Missing, Type.Missing, false, false);

            try
            {
                _currentUserEmail = ns.CurrentUser?.AddressEntry?.GetExchangeUser()?.PrimarySmtpAddress;
            }
            catch { }

            if (string.IsNullOrEmpty(_currentUserEmail))
            {
                try
                {
                    _currentUserEmail = ns.Accounts[1]?.SmtpAddress;
                }
                catch { }
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Outlook initialization failed: {ex.Message}");
            return false;
        }
    }

    public void SendTaskAssignedEmail(string toEmail, string taskName, string projectName, 
        DateTimeOffset startDate, DateTimeOffset endDate, string assignerEmail)
    {
        SendEmail(
            toEmail,
            $"[NMKApp] Task Assigned: {taskName}",
            $@"<html><body style='font-family: Segoe UI, sans-serif;'>
<h2 style='color: #1976D2;'>New Task Assigned</h2>
<table style='border-collapse: collapse;'>
<tr><td style='padding: 8px; font-weight: bold;'>Task:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(taskName)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Project:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(projectName)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Start:</td><td style='padding: 8px;'>{startDate:dd/MM/yyyy HH:mm}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>End:</td><td style='padding: 8px;'>{endDate:dd/MM/yyyy HH:mm}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Assigned By:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(assignerEmail)}</td></tr>
</table>
<p>Please check the NMKApp for more details.</p>
</body></html>");
    }

    public void SendTaskCompletedEmail(string toEmail, string taskName, string projectName,
        string completedByEmail)
    {
        SendEmail(
            toEmail,
            $"[NMKApp] Task Completed: {taskName}",
            $@"<html><body style='font-family: Segoe UI, sans-serif;'>
<h2 style='color: #4CAF50;'>Task Completed</h2>
<table style='border-collapse: collapse;'>
<tr><td style='padding: 8px; font-weight: bold;'>Task:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(taskName)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Project:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(projectName)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Completed By:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(completedByEmail)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Completed At:</td><td style='padding: 8px;'>{DateTimeOffset.Now:dd/MM/yyyy HH:mm}</td></tr>
</table>
</body></html>");
    }

    public void SendLeaveRequestEmail(string toEmail, string requesterEmail, string leaveType,
        DateTimeOffset dateFrom, DateTimeOffset dateTo, decimal? totalDays, string? reason)
    {
        SendEmail(
            toEmail,
            $"[NMKApp] Leave Request from {requesterEmail}",
            $@"<html><body style='font-family: Segoe UI, sans-serif;'>
<h2 style='color: #FF9800;'>Leave Request</h2>
<table style='border-collapse: collapse;'>
<tr><td style='padding: 8px; font-weight: bold;'>Requester:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(requesterEmail)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Type:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(leaveType)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>From:</td><td style='padding: 8px;'>{dateFrom:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>To:</td><td style='padding: 8px;'>{dateTo:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Total Days:</td><td style='padding: 8px;'>{totalDays}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Reason:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(reason ?? "N/A")}</td></tr>
</table>
<p>Please review this request in NMKApp.</p>
</body></html>");
    }

    public void SendLeaveApprovedEmail(string toEmail, string approverEmail, string leaveType,
        DateTimeOffset dateFrom, DateTimeOffset dateTo)
    {
        SendEmail(
            toEmail,
            $"[NMKApp] Leave Approved",
            $@"<html><body style='font-family: Segoe UI, sans-serif;'>
<h2 style='color: #4CAF50;'>Leave Request Approved</h2>
<table style='border-collapse: collapse;'>
<tr><td style='padding: 8px; font-weight: bold;'>Approved By:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(approverEmail)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Type:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(leaveType)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>From:</td><td style='padding: 8px;'>{dateFrom:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>To:</td><td style='padding: 8px;'>{dateTo:dd/MM/yyyy}</td></tr>
</table>
<p>Your leave request has been approved.</p>
</body></html>");
    }

    public void SendLeaveRejectedEmail(string toEmail, string approverEmail, string leaveType,
        DateTimeOffset dateFrom, DateTimeOffset dateTo, string? note)
    {
        SendEmail(
            toEmail,
            $"[NMKApp] Leave Rejected",
            $@"<html><body style='font-family: Segoe UI, sans-serif;'>
<h2 style='color: #F44336;'>Leave Request Rejected</h2>
<table style='border-collapse: collapse;'>
<tr><td style='padding: 8px; font-weight: bold;'>Rejected By:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(approverEmail)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Type:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(leaveType)}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>From:</td><td style='padding: 8px;'>{dateFrom:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>To:</td><td style='padding: 8px;'>{dateTo:dd/MM/yyyy}</td></tr>
<tr><td style='padding: 8px; font-weight: bold;'>Note:</td><td style='padding: 8px;'>{System.Net.WebUtility.HtmlEncode(note ?? "N/A")}</td></tr>
</table>
</body></html>");
    }

    private void SendEmail(string to, string subject, string htmlBody)
    {
        if (_outlookApp == null)
            throw new InvalidOperationException("Outlook is not initialized.");

        // olMailItem = 0
        dynamic mailItem = _outlookApp.CreateItem(0);
        mailItem.To = to;
        mailItem.Subject = subject;
        mailItem.HTMLBody = htmlBody;
        mailItem.Send();
    }

    public void Dispose()
    {
        if (_outlookApp != null)
        {
            Marshal.ReleaseComObject(_outlookApp);
            _outlookApp = null;
        }
    }
}
