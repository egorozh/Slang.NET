using Slang.Gpt.Data;

namespace Slang.Gpt.Domain.Models;

public static class GptModel
{
    /// <param name="Id">
    /// The id of this model.
    /// Will be sent to the GPT API.
    /// </param>
    /// <param name="Provider">The provider of this model.</param>
    /// <param name="DefaultInputLength">
    /// Each model has a limited context until this model starts to "forget".
    ///
    /// The default input length is calculated as follows:
    /// 1 token = 4 characters (English)
    /// input context = 1 / 3 of the model's context (Assuming 2x output context)
    /// Therefore, input_length = 1.33 * model_context
    /// </param>
    /// <param name="CostPer1KInputToken">The cost per input token in USD.</param>
    /// <param name="CostPer1KOutputToken">he cost per output token in USD.</param>
    public record GptModelInfo(
        string Id,
        GptProvider Provider,
        int DefaultInputLength,
        double CostPer1KInputToken,
        double CostPer1KOutputToken
    )
    {
        public double CostPerInputToken => CostPer1KInputToken / 1000;

        public double CostPerOutputToken => CostPer1KOutputToken / 1000;
    }

    internal static readonly GptModelInfo gpt3_5_4k = new("gpt-3.5-turbo", GptProvider.OpenAi,
        DefaultInputLength: 2000,
        CostPer1KInputToken: 0.0005,
        CostPer1KOutputToken: 0.0015);

    private static readonly GptModelInfo gpt3_5_16k = new("gpt-3.5-turbo-16k", GptProvider.OpenAi,
        DefaultInputLength: 8000,
        CostPer1KInputToken: 0.003,
        CostPer1KOutputToken: 0.004);

    private static readonly GptModelInfo gpt4_8k = new("gpt-4", GptProvider.OpenAi,
        DefaultInputLength: 4000,
        CostPer1KInputToken: 0.03,
        CostPer1KOutputToken: 0.06);

    private static readonly GptModelInfo gpt4_turbo = new("gpt-4-turbo", GptProvider.OpenAi,
        DefaultInputLength: 64000,
        CostPer1KInputToken: 0.01,
        CostPer1KOutputToken: 0.03);

    private static readonly GptModelInfo gpt4o = new("gpt-4o", GptProvider.OpenAi,
        DefaultInputLength: 128000,
        CostPer1KInputToken: 0.005,
        CostPer1KOutputToken: 0.015);

    private static readonly GptModelInfo gpt4o_mini = new("gpt-4o-mini", GptProvider.OpenAi,
        DefaultInputLength: 128000,
        CostPer1KInputToken: 0.00015,
        CostPer1KOutputToken: 0.0006);

    public static readonly int DefaultInputLength = 128000;
    
    public static readonly HashSet<GptModelInfo> Values =
    [
        gpt3_5_4k, gpt3_5_16k, gpt4_8k, gpt4_turbo, gpt4o, gpt4o_mini
    ];
}