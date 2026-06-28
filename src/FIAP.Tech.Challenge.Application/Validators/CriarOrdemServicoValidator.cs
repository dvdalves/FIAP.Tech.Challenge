using FIAP.Tech.Challenge.Application.DTOs.Requests;
using FluentValidation;

namespace FIAP.Tech.Challenge.Application.Validators;

public class CriarOrdemServicoValidator : AbstractValidator<CriarOrdemServicoRequest>
{
    public CriarOrdemServicoValidator()
    {
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .MaximumLength(100).WithMessage("O nome do cliente deve ter no máximo 100 caracteres.");

        RuleFor(x => x.ClienteCpf)
            .NotEmpty().WithMessage("O CPF do cliente é obrigatório.");

        RuleFor(x => x.ClienteEmail)
            .NotEmpty().WithMessage("O e-mail do cliente é obrigatório.")
            .EmailAddress().WithMessage("O e-mail informado é inválido.");

        RuleFor(x => x.ClienteTelefone)
            .NotEmpty().WithMessage("O telefone do cliente é obrigatório.");

        RuleFor(x => x.VeiculoPlaca)
            .NotEmpty().WithMessage("A placa do veículo é obrigatória.");

        RuleFor(x => x.VeiculoMarca)
            .NotEmpty().WithMessage("A marca do veículo é obrigatória.");

        RuleFor(x => x.VeiculoModelo)
            .NotEmpty().WithMessage("O modelo do veículo é obrigatório.");

        RuleFor(x => x.VeiculoAno)
            .GreaterThan(1886).WithMessage("O ano do veículo é inválido.");

        RuleFor(x => x.DescricaoProblema)
            .NotEmpty().WithMessage("A descrição do problema é obrigatória.")
            .MinimumLength(5).WithMessage("A descrição do problema deve ter pelo menos 5 caracteres.");
    }
}