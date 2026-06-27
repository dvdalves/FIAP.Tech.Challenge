using System;
using System.Text.RegularExpressions;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.ValueObjects;

public record Cnpj
{
    private string Valor { get; }

    public Cnpj(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DominioException("CNPJ não pode ser vazio.");

        string apenasDigitos = Regex.Replace(valor, @"[^\d]", "");

        if (apenasDigitos.Length != 14 || !Validar(apenasDigitos))
            throw new DominioException($"CNPJ '{valor}' é inválido.");

        Valor = apenasDigitos;
    }

    private static bool Validar(string cnpj)
    {
        if (new string(cnpj[0], 14) == cnpj) return false;

        int[] multiplicador1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var tempCnpj = cnpj[..12];
        var soma = 0;

        for (int i = 0; i < 12; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        tempCnpj += digito1;
        soma = 0;
        for (var i = 0; i < 13; i++)
            soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cnpj.EndsWith($"{digito1}{digito2}");
    }

    public override string ToString() => Valor;
}
