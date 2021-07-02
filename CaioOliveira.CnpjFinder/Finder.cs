using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CaioOliveira.CnpjFinder.Configuration;
using CaioOliveira.CnpjFinder.Extensions.String;
using CaioOliveira.CnpjFinder.Interfaces;
using CaioOliveira.CnpjFinder.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CaioOliveira.CnpjFinder
{
    public class Finder : ICnpjFinder
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;
        private readonly ServiceConfiguration _serviceConfiguration;

        public Finder(ILogger<Finder> logging,
            IOptions<ServiceConfiguration> configuration,
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _serviceConfiguration = configuration?.Value;
            _logger = logging;

            if (_serviceConfiguration == null)
                throw new ArgumentException("Configuração inválida");
        }

        public async Task<QueryResult> Get(string cnpj)
        {
            _logger.LogDebug($"Recebendo uma solicitação para consultar o CNPJ {cnpj}");
            _logger.LogDebug("Normalizando CNPJ");
            var normalizedCnpj = cnpj.OnlyNumbers();
            _logger.LogDebug($"CPJ normalizado {normalizedCnpj}");

            QueryResult result = null;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var rq = new HttpRequestMessage(HttpMethod.Get,
                    $"{_serviceConfiguration.WebBasePath}/{normalizedCnpj}");
                rq.Headers.Add("User-Agent", "Caio Oliveira Cnpj Finder Service");

                _logger.LogDebug($"Enviando solicitação de consulta ao endpoint {_serviceConfiguration.WebBasePath}");
                var rs = await client.SendAsync(rq);
                _logger.LogDebug($"Resposta recebida (Status: {rs.StatusCode}) do endpoint consultado");

                if (rs.StatusCode == HttpStatusCode.OK)
                {
                    _logger.LogDebug("Processando resultado e procurando informações necessárias");
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    result = FindInformations(await rs.Content.ReadAsStringAsync());
                }
                else
                {
                    _logger.LogError("A resposta do servidor não foi a que esperamos, o processo não pode continuar");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao obter as informações necessárias", ex);
            }

            return result;
        }

        private QueryResult FindInformations(string rawHtml)
        {
            QueryResult result;
            var html = new HtmlDocument();
            html.LoadHtml(rawHtml);

            _logger.LogDebug("Procurando div#content");
            var content = html.GetElementbyId("content");

            if (content != null)
            {
                var table = (from tb in content.Descendants("table") select tb).FirstOrDefault();

                if (table != null)
                {
                    result = new QueryResult();
                    var tbody = table.Descendants("tbody").FirstOrDefault();

                    var trs = tbody != null
                        ? tbody.Descendants("tr").ToList()
                        : table.Descendants("tr").ToList();

                    if (trs.Any())
                    {
                        _logger.LogDebug("Linhas da tabela recuperadas, recuperando informações");
                        trs.ForEach(tr =>
                        {
                            var tds = tr
                                .Descendants("td")
                                .ToList();

                            var tdIdentifier = tds.FirstOrDefault();
                            var tdValue = tds.LastOrDefault();

                            if (tdIdentifier == null || tdValue == null) return;

                            switch (tdIdentifier.InnerText
                                .Replace(@"\r", string.Empty)
                                .Replace(@"\n", string.Empty)
                                .Trim())
                            {
                                case "CNPJ":
                                    result.SetCnpj(tdValue.InnerText);
                                    break;
                                case "Nome da empresa":
                                    var a = tdValue.Descendants("a").FirstOrDefault();
                                    var text = a != null ? a.InnerText : tdValue.InnerText;
                                    result.SetCompanyName(text);
                                    break;
                                case "Fantasia nome":
                                    result.SetName(tdValue.InnerText);
                                    break;
                                case "Inicio atividade data":
                                    result.SetCreationDate(tdValue.InnerText);
                                    break;
                                case "Situação cadastral":
                                    result.SetIsActive(tdValue.InnerText);
                                    break;
                            }
                        });
                    }

                    var h3S = table
                        .ParentNode
                        .Descendants("h3")
                        .Where(x => x.InnerText.Equals("Endereço") || x.InnerText.Equals("Contatos"))
                        .ToList();

                    var addressH3 = h3S.FirstOrDefault();
                    var contactsH3 = h3S.LastOrDefault();

                    if (addressH3 != null && contactsH3 != null)
                    {
                        bool stop;
                        var count = 0;
                        var nodes = new List<HtmlNode>();
                        var current = addressH3.NextSibling;
                        do
                        {
                            if (current.NodeType == HtmlNodeType.Text)
                                nodes.Add(current);

                            stop = current.Line.Equals(contactsH3.Line);
                            count++;
                            current = current.NextSibling;
                        } while (!stop || count > 15);

                        nodes
                            .ForEach(i =>
                            {
                                switch (nodes.IndexOf(i))
                                {
                                    case 0:
                                        result.SetNumber(i.InnerText);
                                        break;
                                    case 1:
                                        if ((contactsH3.Line - addressH3.Line).Equals(6))
                                            result.SetComplement(i.InnerText);
                                        break;
                                    default:
                                        result.SetCEP(i.InnerText);
                                        break;
                                }
                            });
                    }
                    else
                    {
                        _logger.LogDebug("dados de endereço não encontrados");
                    }

                    return result;
                }

                _logger.LogWarning("table não localizada, o processo não deve continuar");
            }
            else
            {
                _logger.LogWarning("div#content não localizada, o processo não pode continuar");
            }

            return null;
        }
    }
}