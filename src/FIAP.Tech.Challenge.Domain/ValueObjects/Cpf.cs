using System;
using System.Text.RegularExpressions;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.ValueObjects;

public record Cpf
{
    public string Valor { get; }

    public Cpf(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DominioException("CPF não pode ser vazio.");

        string apenasDigitos = Regex.Replace(valor, @"[^\d]", "");

        if (apenasDigitos.Length != 11 || !Validar(apenasDigitos))
            throw new DominioException($"CPF '{valor}' é inválido.");

        Valor = apenasDigitos;
    }

    private static bool Validar(string cpf)
    {
        if (new string(cpf[0], 11) == cpf) return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf[..9];
        int soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCpf += digito1;
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith($"{digito1}{digito2}");
    }

    public override string ToString() => Valor;
}
