using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using Serilog;

namespace QuestBot;

public static class Utils
{
    public static void SendCalendarInvite(string toEmail, DateTime startTime, TimeSpan duration, string subject,
        string description, string location = null)
    {
        if (Config.Smtp == null || string.IsNullOrEmpty(Config.Smtp.Host) ||
            string.IsNullOrEmpty(Config.Smtp.Username) || string.IsNullOrEmpty(Config.Smtp.Password))
        {
            Log.Warning("SMTP config is not set, skipping calendar invite");
            return;
        }

        // Create a new MailMessage object
        var message = new MailMessage();
        message.From = new MailAddress(Config.Smtp.From);
        message.To.Add(new MailAddress(toEmail));

        message.Subject = subject;
        message.Body = description;
        message.IsBodyHtml = true;

        // Create a new iCalendar object
        var iCal = new StringBuilder();

        iCal.AppendLine("BEGIN:VCALENDAR");
        iCal.AppendLine("PRODID:-//Schedule a Meeting");
        iCal.AppendLine("VERSION:2.0");
        iCal.AppendLine("METHOD:REQUEST");
        iCal.AppendLine("BEGIN:VEVENT");
        iCal.AppendLine($"DTSTART:{startTime.ToUniversalTime():yyyyMMddTHHmmssZ}");
        iCal.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
        iCal.AppendLine($"DTEND:{(startTime + duration).ToUniversalTime():yyyyMMddTHHmmssZ}");
        if (location != null)
        {
            iCal.AppendLine("LOCATION: " + location);
        }

        iCal.AppendLine($"UID:{Guid.NewGuid()}");
        iCal.AppendLine($"DESCRIPTION:{description.Replace("\n", "<br>")}");
        iCal.AppendLine($"SUMMARY:{subject}");
        iCal.AppendLine($"ORGANIZER:MAILTO:{message.From.Address}");

        iCal.AppendLine($"ATTENDEE;CN=\"{toEmail}\";RSVP=TRUE:mailto:{toEmail}");

        iCal.AppendLine("BEGIN:VALARM");
        iCal.AppendLine("TRIGGER:-PT15M");
        iCal.AppendLine("ACTION:DISPLAY");
        iCal.AppendLine("DESCRIPTION:Reminder");
        iCal.AppendLine("END:VALARM");
        iCal.AppendLine("END:VEVENT");
        iCal.AppendLine("END:VCALENDAR");

        // Add the calendar event to the message
        var calendarBytes = Encoding.UTF8.GetBytes(iCal.ToString());
        var calendarAttachment = new Attachment(new MemoryStream(calendarBytes), "invite.ics", "text/calendar");
        message.Attachments.Add(calendarAttachment);

        // Send the email
        using (var client = new SmtpClient(Config.Smtp.Host, Config.Smtp.Port))
        {
            client.Credentials = new NetworkCredential(Config.Smtp.Username, Config.Smtp.Password);
            client.EnableSsl = true;
            client.Send(message);
        }
    }
}