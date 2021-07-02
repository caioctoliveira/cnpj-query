using System;
using System.Text.RegularExpressions;
using CaioOliveira.CnpjFinder.Extensions.String;

namespace CaioOliveira.CnpjFinder.Models
{
    public class QueryResult
    {
        public string CNPJ { get; set; }
        public string CompanyName { get; set; }
        public string Name { get; set; }
        public string CEP { get; set; }
        public string Number { get; set; }
        public string Complement { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }

        internal void SetCnpj(string content)
        {
            CNPJ = content.OnlyNumbers();
        }

        internal void SetCompanyName(string content)
        {
            CompanyName = RemoveLineBreaks(content);
        }

        internal void SetName(string content)
        {
            Name = RemoveLineBreaks(content);
        }

        internal void SetCEP(string content)
        {
            if (Regex.IsMatch(content, @"\d{5}-\d{3}"))
                CEP = content.OnlyNumbers();
        }

        internal void SetNumber(string content)
        {
            var regex = new Regex(@".*,\s?(\d+)");
            var match = regex.Match(RemoveLineBreaks(content));

            if (match.Success)
                Number = match.Groups[1].Value;
        }

        internal void SetComplement(string content)
        {
            Complement = RemoveLineBreaks(content);
        }

        internal void SetCreationDate(string content)
        {
            var rgx = new Regex(@"\d{4}-\d{2}-\d{2}");
            var match = rgx.Match(content);

            if (match.Success && DateTime.TryParse(match.Groups[0].Value, out var date))
                CreationDate = date;
        }

        internal void SetIsActive(string content)
        {
            var rgx = new Regex(@"[A-Z]+");
            var match = rgx.Match(content);

            if (match.Success)
                IsActive = match.Groups[0].Value.Equals("ATIVA");
        }

        private string RemoveLineBreaks(string content)
        {
            return content
                .Replace("/n", string.Empty)
                .Replace("/r", string.Empty)
                .Trim();
        }
    }
}