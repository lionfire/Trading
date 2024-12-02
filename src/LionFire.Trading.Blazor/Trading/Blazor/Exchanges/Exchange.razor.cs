using LionFire.Trading.Exchanges;
using MudBlazor;
using Parameter = Microsoft.AspNetCore.Components.ParameterAttribute;

namespace LionFire.Trading.Blazor.Exchanges;

public partial class Exchange(ExchangeInfos exchangeInfos)
{
    #region Dependencies

    public ExchangeInfos ExchangeInfos { get; } = exchangeInfos;

    #endregion

    #region Parameters

    [Microsoft.AspNetCore.Components.Parameter]
    public string? ExchangeId { get; set; }

    #endregion

    #region Lifecycle

    protected override Task OnParametersSetAsync()
    {
        if (ExchangeId != null)
        {
            Areas = ExchangeInfos.Areas(ExchangeId);
        }
        return base.OnParametersSetAsync();
    }

    #endregion

    #region State

    public IEnumerable<string>? Areas { get; set; }

    #endregion


}