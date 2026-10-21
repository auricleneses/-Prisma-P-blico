using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseDefaultFiles();
app.UseStaticFiles();

var dataDirectory = Environment.GetEnvironmentVariable("PRISMA_DATA_DIR");
if (string.IsNullOrWhiteSpace(dataDirectory))
    dataDirectory = Path.Combine(app.Environment.ContentRootPath, "data");

Directory.CreateDirectory(dataDirectory);
var arquivoEvidencias = Path.Combine(dataDirectory, "evidencias.json");
var travaPersistencia = new object();
var evidencias = CarregarEvidencias();
var proximoId = evidencias.Keys.DefaultIfEmpty(0).Max();
var chaveAdministrativa = Environment.GetEnvironmentVariable("PRISMA_ADMIN_KEY");
var tiposValidos = new[]
{
    "Fato confirmado",
    "Alegação",
    "Investigação",
    "Decisão judicial",
    "Condenação anulada",
    "Ação anulada",
    "Absolvição",
    "Denúncia rejeitada"
};

ConcurrentDictionary<int, Evidencia> CarregarEvidencias()
{
    var iniciais = new[]
    {
        new Evidencia(
            1,
            "Luiz Inácio Lula da Silva",
            "No caso do triplex do Guarujá e do sítio de Atibaia, o Ministério Público acusou Lula de receber vantagens indevidas. Em abril de 2021, o STF anulou as ações julgadas em Curitiba por entender que aquela vara não era competente para esses casos.",
            "https://portal.stf.jus.br/noticias/verNoticiaDetalhe.asp?idConteudo=464261&ori=1",
            new DateOnly(2021, 4, 15),
            "Lava Jato / Petrolão: triplex e sítio",
            "Condenação anulada",
            "O STF anulou as condenações porque os casos não deveriam ter sido julgados pela vara de Curitiba. Anular uma condenação por competência do juízo não é o mesmo que uma decisão de mérito sobre inocência.",
            "Em 2021, o STF confirmou a anulação das decisões de Curitiba."),
        new Evidencia(
            2,
            "Luiz Inácio Lula da Silva",
            "O Ministério Público acusou Lula de corrupção passiva em uma investigação sobre suposto favorecimento a montadoras na edição da Medida Provisória 471, de 2009. Lula negou a acusação. Em junho de 2021, a Justiça Federal o absolveu por falta de provas suficientes.",
            "https://g1.globo.com/politica/noticia/2021/06/21/justica-federal-absolve-lula-e-gilberto-carvalho-em-acao-da-operacao-zelotes.ghtml",
            new DateOnly(2021, 6, 21),
            "Operação Zelotes: MP 471",
            "Absolvição",
            "Neste caso, houve julgamento e absolvição: o juiz concluiu que não havia provas suficientes para condenar Lula. Isso é diferente de uma condenação anulada por uma questão processual.",
            "A reportagem do G1 informou a absolvição em 21 de junho de 2021."),
        new Evidencia(
            3,
            "Luiz Inácio Lula da Silva",
            "Houve ações penais relacionadas à sede e a doações do Instituto Lula. Em abril de 2021, o STF incluiu esses processos na decisão que anulou as ações julgadas pela 13ª Vara Federal de Curitiba, por falta de competência daquela vara.",
            "https://portal.stf.jus.br/noticias/verNoticiaDetalhe.asp?idConteudo=464261&ori=1",
            new DateOnly(2021, 4, 15),
            "Lava Jato: Instituto Lula",
            "Ação anulada",
            "O STF decidiu que esses casos não deveriam ter sido julgados pela vara de Curitiba. A anulação por competência não confirma a acusação nem é uma absolvição sobre os fatos.",
            "Em abril de 2021, o STF confirmou a anulação dos atos da 13ª Vara Federal de Curitiba."),
        new Evidencia(
            4,
            "Flávio Bolsonaro",
            "O Ministério Público do Rio acusou Flávio Bolsonaro e outras pessoas de participar de um esquema de devolução de parte dos salários de assessores de seu gabinete na Alerj, chamado de “rachadinha”. A denúncia citava peculato, lavagem de dinheiro e organização criminosa. A defesa contestou as acusações.",
            "https://www.infomoney.com.br/politica/tj-do-rio-arquiva-denuncia-do-mp-contra-flavio-bolsonaro-em-caso-das-rachadinhas/",
            new DateOnly(2022, 5, 17),
            "Rachadinha: gabinete na Alerj",
            "Denúncia rejeitada",
            "Em maio de 2022, o TJ-RJ rejeitou e arquivou a denúncia depois que provas usadas para apresentá-la foram anuladas pelos tribunais superiores. A denúncia não chegou a uma condenação de mérito.",
            "A denúncia foi rejeitada em 2022 após a anulação de provas. A reportagem registrou que o Ministério Público não descartava reiniciar a investigação."),
        new Evidencia(
            5,
            "Flávio Bolsonaro",
            "Segundo o G1, Flávio foi incluído como investigado em inquérito do STF sobre suspeitas relacionadas ao financiamento do filme “Dark Horse”, sobre Jair Bolsonaro. A apuração inclui suspeitas de lavagem de dinheiro, corrupção e evasão de divisas ligadas a repasses de Daniel Vorcaro. Investigação não é prova de culpa.",
            "https://g1.globo.com/politica/noticia/2026/09/11/flavio-bolsonaro-e-investigado-em-inquerito-da-pf-aberto-para-apurar-financiamento-do-filme-dark-horse.ghtml",
            new DateOnly(2026, 9, 11),
            "Financiamento do filme Dark Horse",
            "Investigação",
            "O STF autorizou uma investigação. Isso significa que autoridades estão apurando suspeitas; não significa que houve condenação ou prova definitiva de crime.",
            "O G1 informou em 11 de setembro de 2026 que o inquérito estava em andamento."),
        new Evidencia(
            6,
            "Flávio Bolsonaro",
            "Em outubro de 2026, o STF abriu um inquérito para apurar suspeitas de lavagem de dinheiro envolvendo Flávio Bolsonaro, o advogado Willer Tomaz e empresas ligadas a ele. A Polícia Federal relatou indícios de vínculo com uma conta empresarial; Flávio negou ter mantido essa relação financeira e disse que os dois são amigos.",
            "https://g1.globo.com/politica/noticia/2026/10/18/stf-abre-inquerito-para-apurar-suposta-lavagem-de-dinheiro-envolvendo-flavio-bolsonaro-e-o-advogado-willer-tomaz.ghtml",
            new DateOnly(2026, 10, 18),
            "Inquérito sobre Willer Tomaz e lavagem de dinheiro",
            "Investigação",
            "O STF abriu uma investigação após pedido da Polícia Federal e manifestação da PGR. Um inquérito apura suspeitas; não é uma condenação. O advogado também negou ter cometido crimes.",
            "Investigação aberta em outubro de 2026, conforme reportagem do G1. As suspeitas e as negativas atribuídas aos investigados estão resumidas na fonte."),
        new Evidencia(
            7,
            "Luiz Inácio Lula da Silva",
            "A Ação Penal 470, conhecida como Mensalão, julgou pessoas acusadas de comprar apoio político no Congresso. Em 2008, o STF rejeitou um pedido da defesa de um réu para incluir Lula nesse processo.",
            "https://noticias.stf.jus.br/postsnoticias/stf-adota-medidas-contra-manobras-destinadas-a-retardar-o-processo-do-mensalao/",
            new DateOnly(2011, 2, 3),
            "Mensalão: Lula não foi réu na Ação Penal 470",
            "Fato confirmado",
            "Mensalão foi um processo sobre compra de apoio político. O STF registra que um pedido para incluir Lula como réu foi rejeitado. Portanto, ele não foi réu nem condenado na Ação Penal 470. Isso não resume os outros processos dele.",
            "O STF rejeitou em 2008 o pedido para incluir Lula como réu na AP 470, conforme notícia publicada em 2011."),
        new Evidencia(
            8,
            "Severino Cavalcanti",
            "No chamado “mensalinho” da Câmara, o Ministério Público acusou o então deputado Severino Cavalcanti de cobrar dinheiro de um empresário para manter a concessão de um restaurante na Câmara. Lula não era o acusado nesse caso.",
            "https://g1.globo.com/pe/pernambuco/noticia/2020/07/15/relembre-o-que-foi-o-mensalinho-da-camara-que-levou-a-renuncia-de-severino-cavalcanti.ghtml",
            new DateOnly(2020, 7, 15),
            "Mensalinho da Câmara: caso de Severino Cavalcanti",
            "Alegação",
            "Este “mensalinho” não era um processo contra Lula. A denúncia contra Severino foi recebida pela Justiça, mas não chegou a julgamento; a reportagem diz que o processo deveria ser encerrado após a morte dele. Não houve condenação nesse processo.",
            "A denúncia foi recebida em 2007 e não havia sido julgada quando Severino morreu em 2020, segundo o G1."),
        new Evidencia(
            9,
            "Roberta Luchsinger",
            "Na Operação Sem Desconto, a Polícia Federal investiga se Roberta atuava como operadora financeira e política de Antônio Carlos Camilo Antunes, conhecido como Careca do INSS. Reportagens dizem que uma consultoria ligada ao lobista transferiu cerca de R$ 1,5 milhão para uma empresa em nome de Roberta. Ela nega irregularidades.",
            "https://timesbrasil.com.br/brasil/quem-e-roberta-luchsinger-empresaria-investigada-no-caso-do-inss-e-amiga-de-lulinha/",
            new DateOnly(2026, 10, 18),
            "Operação Sem Desconto: Roberta Luchsinger e o Careca do INSS",
            "Investigação",
            "A investigação apura se Roberta ajudou a movimentar dinheiro ligado ao esquema de descontos indevidos no INSS. Ser investigada não significa que foi condenada. Roberta disse à PF que apresentou Lulinha a Antunes em um encontro social e negou ter repassado dinheiro a ele.",
            "A reportagem informa que Roberta foi alvo de busca e apreensão em dezembro de 2025 e cumpre medidas cautelares. Não relata condenação."),
        new Evidencia(
            10,
            "Fábio Luís Lula da Silva (Lulinha)",
            "Em outubro de 2026, o G1 informou que Lulinha e Roberta Luchsinger eram investigados em inquéritos no STF que apuram suspeitas de tráfico de influência, corrupção e lavagem de dinheiro ligadas a possíveis negócios com órgãos públicos e ao esquema de fraudes no INSS. A reportagem também registra que os investigados negam irregularidades.",
            "https://g1.globo.com/fantastico/noticia/2026/10/18/em-entrevista-lobista-investigada-por-trafico-de-influencia-cita-lulinha-e-propoe-delacao.ghtml",
            new DateOnly(2026, 10, 18),
            "Inquéritos sobre suspeitas ligadas ao INSS e a órgãos públicos",
            "Investigação",
            "O G1 relata que autoridades estão apurando suspeitas. Isso não significa que houve condenação. Roberta afirmou ter apresentado Lulinha ao Careca do INSS e negou ter repassado dinheiro a ele.",
            "O G1 informou em 18 de outubro de 2026 que os inquéritos estavam em andamento e sob sigilo."),
        new Evidencia(
            11,
            "Luiz Inácio Lula da Silva",
            "Em entrevista ao G1, Roberta Luchsinger disse que pretendia fazer delação premiada e afirmou que poderia falar sobre Lula e Lulinha. Ela também alegou que Lulinha se reunia com ministros. Lula negou interlocução com Roberta, disse que nunca recebeu demandas dela e afirmou que o governo não tinha contrato com ela. O ministro Camilo Santana também negou conversas sobre governo ou negócios com Lulinha.",
            "https://g1.globo.com/fantastico/noticia/2026/10/18/em-entrevista-lobista-investigada-por-trafico-de-influencia-cita-lulinha-e-propoe-delacao.ghtml",
            new DateOnly(2026, 10, 18),
            "Declarações de Roberta Luchsinger sobre Lula e Lulinha",
            "Alegação",
            "Este registro resume declarações feitas em uma entrevista, não uma conclusão judicial. A reportagem não diz que Lula é investigado nos inquéritos citados; registra as negativas de Lula e de Camilo Santana. A intenção de negociar uma delação não significa que um acordo foi aceito nem que as declarações foram comprovadas.",
            "Publicada em 18 de outubro de 2026. A reportagem descreve investigações envolvendo Roberta e Lulinha; Lula negou ligação com Roberta.")
    };

    var registros = File.Exists(arquivoEvidencias)
        ? new ConcurrentDictionary<int, Evidencia>(
            (JsonSerializer.Deserialize<Evidencia[]>(File.ReadAllText(arquivoEvidencias)) ?? [])
                .ToDictionary(e => e.Id))
        : new ConcurrentDictionary<int, Evidencia>();

    var adicionouRegistros = false;
    foreach (var inicial in iniciais)
        adicionouRegistros |= registros.TryAdd(inicial.Id, inicial);

    if (adicionouRegistros)
        SalvarEvidencias(registros);

    return registros;
}

void SalvarEvidencias(ConcurrentDictionary<int, Evidencia> registros)
{
    lock (travaPersistencia)
    {
        var json = JsonSerializer.Serialize(registros.Values.OrderBy(e => e.Id));
        File.WriteAllText(arquivoEvidencias, json);
    }
}

bool TemAcessoAdministrativo(HttpRequest request)
{
    if (string.IsNullOrEmpty(chaveAdministrativa))
        return false;

    var chaveRecebida = Encoding.UTF8.GetBytes(request.Headers["X-Admin-Key"].ToString());
    var chaveEsperada = Encoding.UTF8.GetBytes(chaveAdministrativa);
    return chaveRecebida.Length == chaveEsperada.Length &&
        CryptographicOperations.FixedTimeEquals(chaveRecebida, chaveEsperada);
}

app.MapGet("/api/status", () => Results.Ok(new
{
    sistema = "Prisma Público",
    mensagem = "API de auditoria de evidências funcionando!",
    versao = "0.3.0"
}));

app.MapGet("/api/evidencias", () =>
    Results.Ok(evidencias.Values.OrderBy(e => e.Id)));

app.MapGet("/api/evidencias/{id:int}", (int id) =>
{
    if (evidencias.TryGetValue(id, out var evidencia))
        return (IResult)Results.Ok(evidencia);

    return Results.NotFound(new { erro = "Evidência não encontrada." });
});

app.MapPost("/api/evidencias", (NovaEvidencia? entrada, HttpRequest request) =>
{
    if (!TemAcessoAdministrativo(request))
        return (IResult)Results.Unauthorized();

    if (entrada is null ||
        string.IsNullOrWhiteSpace(entrada.Candidato) ||
        string.IsNullOrWhiteSpace(entrada.Afirmacao) ||
        string.IsNullOrWhiteSpace(entrada.Fonte))
    {
        return (IResult)Results.BadRequest(new
        {
            erro = "Candidato, afirmação e fonte são obrigatórios."
        });
    }

    var tipo = tiposValidos.FirstOrDefault(t =>
        string.Equals(t, entrada.Tipo, StringComparison.OrdinalIgnoreCase));

    if (tipo is null)
    {
        return (IResult)Results.BadRequest(new
        {
            erro = "Tipo inválido. Use: Fato confirmado, Alegação, Investigação ou Decisão judicial."
        });
    }

    var id = Interlocked.Increment(ref proximoId);
    var evidencia = new Evidencia(
        id,
        entrada.Candidato.Trim(),
        entrada.Afirmacao.Trim(),
        entrada.Fonte.Trim(),
        entrada.Data,
        entrada.Categoria?.Trim() ?? "",
        tipo,
        entrada.ExplicacaoSimples?.Trim() ?? entrada.Afirmacao.Trim(),
        entrada.SituacaoAtual?.Trim() ?? tipo);

    evidencias[id] = evidencia;
    SalvarEvidencias(evidencias);
    return Results.Created($"/api/evidencias/{id}", evidencia);
});

app.MapPut("/api/evidencias/{id:int}", (int id, NovaEvidencia? entrada, HttpRequest request) =>
{
    if (!TemAcessoAdministrativo(request))
        return (IResult)Results.Unauthorized();

    if (entrada is null)
        return (IResult)Results.BadRequest(new { erro = "Dados obrigatórios." });

    if (!evidencias.ContainsKey(id))
        return (IResult)Results.NotFound(new { erro = "Evidência não encontrada." });

    if (string.IsNullOrWhiteSpace(entrada.Candidato) ||
        string.IsNullOrWhiteSpace(entrada.Afirmacao) ||
        string.IsNullOrWhiteSpace(entrada.Fonte))
    {
        return (IResult)Results.BadRequest(new
        {
            erro = "Candidato, afirmação e fonte são obrigatórios."
        });
    }

    var tipo = tiposValidos.FirstOrDefault(t =>
        string.Equals(t, entrada.Tipo, StringComparison.OrdinalIgnoreCase));

    if (tipo is null)
        return (IResult)Results.BadRequest(new { erro = "Tipo de evidência inválido." });

    var atualizada = new Evidencia(
        id,
        entrada.Candidato.Trim(),
        entrada.Afirmacao.Trim(),
        entrada.Fonte.Trim(),
        entrada.Data,
        entrada.Categoria?.Trim() ?? "",
        tipo,
        entrada.ExplicacaoSimples?.Trim() ?? entrada.Afirmacao.Trim(),
        entrada.SituacaoAtual?.Trim() ?? tipo);

    evidencias[id] = atualizada;
    SalvarEvidencias(evidencias);
    return Results.Ok(atualizada);
});

app.MapDelete("/api/evidencias/{id:int}", (int id, HttpRequest request) =>
{
    if (!TemAcessoAdministrativo(request))
        return (IResult)Results.Unauthorized();

    if (evidencias.TryRemove(id, out _))
    {
        SalvarEvidencias(evidencias);
        return (IResult)Results.NoContent();
    }

    return Results.NotFound(new { erro = "Evidência não encontrada." });
});

app.Run();

internal record NovaEvidencia(
    string? Candidato,
    string? Afirmacao,
    string? Fonte,
    DateOnly Data,
    string? Tipo,
    string? Categoria,
    string? ExplicacaoSimples = null,
    string? SituacaoAtual = null);

internal record Evidencia(
    int Id,
    string Candidato,
    string Afirmacao,
    string Fonte,
    DateOnly Data,
    string Categoria,
    string Tipo,
    string ExplicacaoSimples,
    string SituacaoAtual);