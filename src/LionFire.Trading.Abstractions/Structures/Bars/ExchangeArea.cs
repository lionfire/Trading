#nullable enable
using LionFire.Validation;
using Orleans;

namespace LionFire.Trading;

[Alias("exchange-area")]
[GenerateSerializer]
public record ExchangeArea(string Exchange, string Area) 
    : IKeyed<string>
    , IValidatable
{

    public virtual string Key => $"{Exchange}.{Area}";

    #region Validation

    public virtual ValidationContext ValidateThis(ValidationContext v) => v
            .PropertyNotNullOrEmptyString(nameof(Exchange), Exchange)
            .PropertyNotNullOrEmptyString(nameof(Area), Area)
            ;

    #endregion
}

