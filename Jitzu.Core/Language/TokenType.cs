using NetEscapades.EnumGenerators;

namespace Jitzu.Core.Language;

[EnumExtensions]
public enum TokenType
{
    None,
    Int,
    Double,
    Boolean,
    Keyword,
    Identifier,
    Char,
    String,
    Punctuation,
    Operator,
    Comment,
    RangeOperator,
    Interpolation,
    InterpolationStringStart,
    InterpolationStringEnd,
    InterpolationTextToken,
    Tag,
    Version,
}
