using System.Net.Http.Json;
using Dumpify;

var tipoBusca = TipoBusca.ViaCep;

ICepAdapter cepAdapter = tipoBusca switch
{
    TipoBusca.ViaCep => new ViaCepAdapter(),
    TipoBusca.OpenCep => new OpenCepAdapter(),
    _ => throw new NotImplementedException(),
};

var endereco = await cepAdapter.GetEndereco("60810820");

endereco.Dump();

//Adapter
public interface ICepAdapter
{
    public Task<Endereco> GetEndereco(string cep);
}

//Adapter do Via Cep
public class ViaCepAdapter : ICepAdapter
{
    private static readonly HttpClient _httpClient = new();

    public async Task<Endereco> GetEndereco(string cep)
    {
        var response = await _httpClient.GetAsync($"https://viacep.com.br/ws/{cep}/json/");

        var cepOpenCep = await response.Content.ReadFromJsonAsync<EnderecoViaCep>();
        return cepOpenCep switch
        {
            null => throw new Exception("O endereco retornado foi vazio"),
            _ => new Endereco(
                cepOpenCep.Logradouro,
                cepOpenCep.Complemento,
                cepOpenCep.Bairro,
                cepOpenCep.Localidade,
                cepOpenCep.Uf
            )
        };
    }
}

//Adapter do OpenCep
public class OpenCepAdapter : ICepAdapter
{
    private static readonly HttpClient _httpClient = new();

    public async Task<Endereco> GetEndereco(string cep)
    {
        var response = await _httpClient.GetAsync($"https://opencep.com/v1/{cep}.json");

        var cepOpenCep = await response.Content.ReadFromJsonAsync<EnderecoOpenCep>();

        return cepOpenCep switch
        {
            null => throw new Exception("O endereco retornado foi vazio"),
            _ => new Endereco(
                cepOpenCep.Logradouro,
                cepOpenCep.Complemento,
                cepOpenCep.Bairro,
                cepOpenCep.Localidade,
                cepOpenCep.Uf
            )
        };
    }
}

//RetornosApis
public record EnderecoViaCep(string Logradouro, string? Complemento, string Bairro, string Localidade, string Uf);
public record EnderecoOpenCep(string Logradouro, string? Complemento, string Bairro, string Localidade, string Uf);

//Endereco comum
public record Endereco(string Logradouro, string? Complemento, string Bairro, string Cidade, string Uf);

//Tipo busca para definicao de adapter utilizado
public enum TipoBusca
{
    ViaCep,
    OpenCep
}
