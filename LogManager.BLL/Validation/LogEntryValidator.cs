using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using LogManager.BLL.Utilities;
using LogManager.Core.Settings;
using Microsoft.Extensions.Options;
using System.Net;

namespace LogManager.BLL.Validation
{
    public class LogEntryValidator : AbstractValidator<ParsedLogEntry>
    {
        private RequestSettings requestSettings;

        public LogEntryValidator(IOptionsSnapshot<RequestSettings> requestSettingOptions)
        {
            this.requestSettings = requestSettingOptions.Value;

            this.RuleFor(x => x.IpAddress)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.IpAddress)
                        .Must(IsMatchIpFormat);
                });

            this.RuleFor(x => x.Amount)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.Amount)
                        .Must(IsPositiveInt);
                });

            this.RuleFor(x => x.Date)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.Date)
                        .Must(IsDateTimeOffset);
                });

            this.RuleFor(x => x.Method)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.Method)
                        .Must(IsAllowableMethod);
                });

            this.RuleFor(x => x.Path)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.Path)
                        .Must(IsAllowableExtension);
                });

            this.RuleFor(x => x.Status)
                .NotEmpty()
                .DependentRules(() =>
                {
                    this.RuleFor(x => x.Status)
                        .Must(IsValidStatusCode);
                });
        }

        private bool IsPositiveInt(string amountStr)
        {
            return int.TryParse(amountStr, out int amount) && amount > 0;
        }

        private bool IsValidStatusCode(string statusStr)
        {
            var statusCodes = Enum.GetValues<HttpStatusCode>().Select(x => (short)x);

            return short.TryParse(statusStr, out short status) && statusCodes.Contains(status);
        }

        private bool IsDateTimeOffset(string dateStr)
        {
            return DateTimeOffset.TryParseExact(
                dateStr,
                LogSettings.DateTimeOffsetPattern,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTimeOffset date);
        }

        private bool IsAllowableMethod(string method)
        {
            return requestSettings.HttpMethods
                .Select(x => x.ToLower())
                .Contains(method.ToLower());
        }

        private bool IsAllowableExtension(string path)
        {
            var uri = new Uri(requestSettings.DefaultUrl + path);
            var pathWithExtension = uri.AbsoluteUri;

            if (uri.Fragment != string.Empty)
                pathWithExtension = pathWithExtension.Replace(uri.Fragment, string.Empty);

            if (uri.Query != string.Empty)
                pathWithExtension = pathWithExtension.Replace(uri.Query, string.Empty);

            var extension = Path.GetExtension(pathWithExtension);

            return requestSettings.AllowableExtensions
                .Select(x => x.ToLower())
                .Contains(extension.ToLower());
        }

        private bool IsMatchIpFormat(string ipStr)
        {
            var ipV4Regex = new Regex(LogSettings.IpV4Pattern);
            var ipV6Regex = new Regex(LogSettings.IpV6Pattern);

            var ipV4Result = ipV4Regex.Match(ipStr);
            var ipV6Result = ipV6Regex.Match(ipStr);

            return ipV4Result.Success || ipV6Result.Success;
        }
    }
}
