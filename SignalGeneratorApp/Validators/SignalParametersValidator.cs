using FluentValidation;
using SignalGeneratorApp.Models;

namespace SignalGeneratorApp.Validators;

public class SignalParametersValidator : AbstractValidator<SignalParameter>
{
    public SignalParametersValidator()
    {
        RuleFor(x => x.Amplitude)
            .NotNull().WithMessage("Амплитуда обязательна.")
            .GreaterThanOrEqualTo(0).WithMessage("Амплитуда не может быть отрицательной.")
            .LessThanOrEqualTo(1000).WithMessage("Амплитуда не должна превышать 1000.");

        RuleFor(x => x.Frequency)
            .NotNull().WithMessage("Частота обязательна.")
            .GreaterThanOrEqualTo(0).WithMessage("Частота не может быть отрицательной.")
            .LessThanOrEqualTo(10000).WithMessage("Частота не должна превышать 10000 Гц.");

        RuleFor(x => x.PointCount)
            .NotNull().WithMessage("Количество точек должно быть указано.")
            .GreaterThan(0).WithMessage("Количество точек должно быть больше 0.")
            .LessThanOrEqualTo(100000).WithMessage("Количество точек не должно превышать 100000.");

        RuleFor(x => x.TimeInterval)
            .NotNull().WithMessage("Временной интервал должен быть указан.")
            .GreaterThan(0).WithMessage("Временной интервал должен быть больше 0.")
            .LessThanOrEqualTo(3600).WithMessage("Временной интервал не должен превышать 3600 секунд (1 час).");

        RuleFor(x => x.NoiseLevel)
            .InclusiveBetween(0, 100).WithMessage("Уровень шума должен быть от 0% до 100%.");
    }
}
