# CaioOliveira.CnpjFinder

Este serviço sem por objetivo realizar consulta de informações de CNPJ utilizando o site http://cnpj.info como fonte de dados de forma gratuita.

A ideia desse serviço é que você utilize em seu sistema para facilitar cadastro de empresas conultando seus dados automaticamente através de um CNPJ válido.

###Limitações

1. Este site bloqueia o IP do solicitante após uma quantidade de consultas, se seu volume é baixo pode usar sem problemas, mas se há um volume muito intenso entre em contato com o desenvolvedor do site, ele vende essas informações.

2. Esta consulta recupera as informações da página, logo se houver uma mudança no layout da página por parte do desenvolvedor o serivço pode parar de funcionar. Vou procurar manter esse pacote atualizado, mas isso pode ser feito pela comunidade também. Qualquer contribuição é muito bem vinda :smiley:

Existe um pacote disponível no [Nuget.org](https://www.nuget.org/packages/CaioOliveira.CnpjFinder) para instalação em seu projeto.

Ciente da limitação acima, para usar basta segui os passos abaixo:

Para começar a usar o serviço basta instalar o pacote supracitado e realizar a configuração abaixo no startup da sua aplicação:

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.UseCnpjFinder();
    ...
}
```

Depois de configurado basta solicitar a interface ICnpjFinder em qualquer classe da sua aplicação que esteja configurada na injeção de dependência do ASP.NET Core, como no exemplo abaixo:

```c#
public class ConsultaAppService
{
    private readonly _cnpjFinder;

    public ConsultaAppService(ICnpjFinder cnpjFinder)
    {
        _cnpjFinder = cnpjFinder;
    }

    public void Consultar()
    {
        ...
        var cnpjInfo = _cnpjFinder.Get("02930076000141");
        ...
    }
}
```

###Configuração alternativa

A forma de configurar já descrita utiliza uma url padrão para consultar o site, porém se essa mudar em algum momento, desde que não se altere o layout da página é possível configurar o apontamento, como no exemplo abaixo:

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.UseCnpjFinder(opt => 
    {
        opt.WebBasePath = "http://www.urlnova.com.br";
    });
    ...
}
```