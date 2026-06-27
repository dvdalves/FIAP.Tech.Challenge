using System;
using System.Text.RegularExpressions;
using FIAP.Tech.Challenge.Domain.Exceptions;

namespace FIAP.Tech.Challenge.Domain.ValueObjects;

public record Placa
{
    public string Valor { get; }

    public Placa(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DominioException("Placa do veículo não pode ser vazia.");

        string placaFormatada = valor.Trim().ToUpper().Replace("-", "");

        if (!Validar(placaFormatada))
            throw new DominioException($"Placa '{valor}' é inválida. Use o formato clássico (AAA-1234) ou Mercosul (AAA1A23).");

        Valor = placaFormatada;
    }

    private static bool Validar(string placa)
    {
        // Tradicional: AAA1234 (7 caracteres, 3 letras e 4 números)
        // Mercosul: AAA1A23 (7 caracteres, 3 letras, 1 número, 1 letra, 2 números)
        string padraoTradicional = @"^[A-Z]{3}\d{4}$";
        string padraoMercosul = @"^[A-Z]{3}\d[A-Z]\d{2}$";

        return Regex.IsMatch(placa, padraoTradicional) || Regex.IsMatch(placa, padraoMercosul);
    }

    public override string ToString() => Valor;
}
