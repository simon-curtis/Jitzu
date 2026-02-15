namespace Jitzu.Core.Types;

public interface ICanFallback
{
    object Fallback(object fallbackValue);
}