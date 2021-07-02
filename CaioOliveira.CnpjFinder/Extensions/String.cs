using System.Linq;
using System.Text.RegularExpressions;

namespace CaioOliveira.CnpjFinder.Extensions.String
{
    public static class Extensions
    {
        public static bool ValidateCPNJ(this string strCNPJ)
        {
            var CNPJ = strCNPJ.Replace(".", "");

            CNPJ = CNPJ.Replace("/", "");
            CNPJ = CNPJ.Replace("-", "");

            int[] digits, sum, result;
            int nrDig;
            string ftmt;
            bool[] CNPJOk;

            ftmt = "6543298765432";
            digits = new int[14];
            sum = new int[2];
            sum[0] = 0;
            sum[1] = 0;
            result = new int[2];
            result[0] = 0;
            result[1] = 0;
            CNPJOk = new bool[2];
            CNPJOk[0] = false;
            CNPJOk[1] = false;

            try
            {
                for (nrDig = 0; nrDig < 14; nrDig++)
                {
                    digits[nrDig] = int.Parse(
                        CNPJ.Substring(nrDig, 1));

                    if (nrDig <= 11)
                        sum[0] += digits[nrDig] *
                                  int.Parse(ftmt.Substring(
                                      nrDig + 1, 1));

                    if (nrDig <= 12)
                        sum[1] += digits[nrDig] *
                                  int.Parse(ftmt.Substring(
                                      nrDig, 1));
                }

                for (nrDig = 0; nrDig < 2; nrDig++)
                {
                    result[nrDig] = sum[nrDig] % 11;

                    if (result[nrDig] == 0 || result[nrDig] == 1)
                        CNPJOk[nrDig] = digits[12 + nrDig] == 0;
                    else
                        CNPJOk[nrDig] = digits[12 + nrDig] == 11 - result[nrDig];
                }

                return CNPJOk[0] && CNPJOk[1];
            }
            catch
            {
                return false;
            }
        }

        public static string OnlyNumbers(this string input)
        {
            var regex = new Regex(@"\d");
            var matches = regex.Matches(input);

            if (matches?.Count > 0)
                return string.Join("", matches
                    .Select(x => x.Success ? x.Value : string.Empty));
            return string.Empty;
        }

        public static string CNPJFormat(this string input)
        {
            var rgx = new Regex(@"^\s?(\d{2})\.?(\d{3})\.?(\d{3})\/?(\d{4})-?(\d{2})\s?$");

            if (rgx.IsMatch(input))
                return rgx.Replace(input, "$1.$2.$3/$4-$5");
            return input;
        }
    }
}